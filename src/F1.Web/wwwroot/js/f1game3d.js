(() => {
  const { THREE } = window;
  const TRACKS = window.F1TrackData?.circuits || {};
  if (!THREE || !Object.keys(TRACKS).length) return;

  // Tuned driving constants for a responsive but controllable feel
  const PHYS = {
    engine_power: 24.0,
    braking_power: 42.0,
    max_speed: 78.0,
    max_speed_reverse: 11.0,
    friction_coefficient: 0.55,
    drag_coefficient: 0.3,
    lateral_grip: 7.5,
    max_steering_angle: 16.0,
    steer_speed: 2.6,
    wheel_base: 2.8,
    velocity_eps: 0.05,
    steering_response: 7.5,
    ai_throttle: 0.82,
  };

  const KMH = 3.6;
  const MODE = { TIME: "time", RACE: "race" };
  const TRACK_SCALE = 2.8;
  const TRACK_WIDTH = 13.5; // scaled to match the ~5:1 width-to-car ratio used by the Godot reference pieces (road_straight.tscn scaled x5 vs ~0.75m car width)
  const KERB_EXTRA = 1.4;
  const GUARD_EXTRA = 1.0;
  const BARRIER_OFFSET = 1.35;
  const BARRIER_THICKNESS = 1.05;
  const CAR_RADIUS = 1.2;
  const BRIDGE_HEIGHT = 3.4;

  const PALETTE = {
    asphalt: 0x2f3138,
    kerbRed: 0xc43b36,
    kerbWhite: 0xf2f2f2,
    grass: 0x6ca16b,
    sand: 0xd2b48c,
    rail: 0xc7b19a,
    treeTrunk: 0x6b4b2f,
    treeLeaves: 0x4e8d52,
    stand: 0x8b8c90,
    skyFallback: 0xb5c9e6,
    cars: {
      red: 0xe0463c,
      green: 0x39bf74,
      orange: 0xf08d2f,
      white: 0xf5f5f5,
    },
  };

  const clamp = (v, lo, hi) => Math.min(hi, Math.max(lo, v));
  const degToRad = (deg) => (deg * Math.PI) / 180;
  const vec3FromArr = (arr) => new THREE.Vector3(arr[0] * TRACK_SCALE, 0, arr[2] * TRACK_SCALE);

  class LapCounter {
    constructor(roadsCount, lapsCount, trackLength) {
      this.roadsCount = Math.max(1, roadsCount);
      this.totalLaps = clamp(Math.round(lapsCount || 1), 1, 6);
      this.trackLength = Math.max(1, trackLength);
      this.completedLaps = 0;
      this.distanceSinceLap = 0;
      this.unwrappedDistance = null;
      this.wrappedDistance = null;
      this.lastRoadIdx = 0;
      this.lapStartTime = 0;
      this.lapTimes = [];
      this.totalProgress = 0;
    }

    updateProgress(distance, elapsedSeconds, forwardDot) {
      const len = this.trackLength;
      if (this.finished()) return;
      let dist = distance;
      if (Number.isFinite(this.unwrappedDistance)) {
        let diff = dist - (this.unwrappedDistance % len);
        if (diff < -len * 0.5) dist += len;
        else if (diff > len * 0.5) dist -= len;
      }

      const wrapped = ((dist % len) + len) % len;
      const prevWrapped = this.wrappedDistance ?? wrapped;
      const movingForward = forwardDot >= 0.05;

      let delta = 0;
      if (Number.isFinite(this.unwrappedDistance)) {
        delta = dist - this.unwrappedDistance;
        if (movingForward && delta < -len * 0.25) delta += len;
        if (!movingForward && delta > 0) delta = 0;
      }

      if (movingForward) {
        delta = Math.max(0, delta);
        this.distanceSinceLap += delta;
        this.totalProgress += delta;
      }

      this.unwrappedDistance = dist;
      this.wrappedDistance = wrapped;
      this.lastRoadIdx = Math.floor((wrapped / len) * this.roadsCount) % this.roadsCount;

      const crossedStart = movingForward && prevWrapped > len * 0.7 && wrapped < len * 0.3;
      const minLapDistance = len * 0.9;
      if (!this.finished() && crossedStart && this.distanceSinceLap >= minLapDistance) {
        const lapTime = Math.max(0, elapsedSeconds - this.lapStartTime);
        this.lapTimes.push(lapTime);
        this.completedLaps += 1;
        this.distanceSinceLap = 0;
        this.lapStartTime = elapsedSeconds;
      }
    }

    current_road_idx() {
      return this.lastRoadIdx;
    }

    lap_idx() {
      return this.completedLaps;
    }

    currentLapDisplay() {
      return Math.min(this.totalLaps, this.completedLaps + 1);
    }

    finished() {
      return this.completedLaps >= this.totalLaps;
    }

    runningLapTime(elapsedSeconds) {
      if (this.finished() && this.lapTimes.length) return this.lapTimes[this.lapTimes.length - 1];
      return Math.max(0, elapsedSeconds - this.lapStartTime);
    }

    bestLap() {
      if (!this.lapTimes.length) return null;
      return Math.min(...this.lapTimes);
    }

    progressFraction() {
      return this.completedLaps + Math.min(1, this.distanceSinceLap / this.trackLength);
    }
  }

  const buildCarMesh = (color) => {
    const group = new THREE.Group();
    const bodyMat = new THREE.MeshStandardMaterial({ color, roughness: 0.38, metalness: 0.22 });
    const darkMat = new THREE.MeshStandardMaterial({ color: 0x111318, roughness: 0.82, metalness: 0.05 });
    const wingMat = new THREE.MeshStandardMaterial({ color: 0xf3f3f3, roughness: 0.45, metalness: 0.18 });

    const chassis = new THREE.Mesh(new THREE.BoxGeometry(0.95, 0.32, 2.4), bodyMat);
    chassis.position.set(0, 0.36, 0.2);
    group.add(chassis);

    const nose = new THREE.Mesh(new THREE.BoxGeometry(0.42, 0.18, 1.1), bodyMat);
    nose.position.set(0, 0.3, -1.5);
    group.add(nose);

    const intake = new THREE.Mesh(new THREE.CylinderGeometry(0.18, 0.22, 0.45, 12), bodyMat);
    intake.rotation.z = Math.PI / 2;
    intake.position.set(0, 0.68, -0.2);
    group.add(intake);

    const sidePodL = new THREE.Mesh(new THREE.BoxGeometry(0.26, 0.26, 1.1), bodyMat);
    sidePodL.position.set(0.6, 0.34, 0.25);
    const sidePodR = sidePodL.clone();
    sidePodR.position.x *= -1;
    group.add(sidePodL, sidePodR);

    const cockpit = new THREE.Mesh(new THREE.BoxGeometry(0.48, 0.45, 0.7), darkMat);
    cockpit.position.set(0, 0.7, -0.15);
    group.add(cockpit);

    const halo = new THREE.Mesh(new THREE.TorusGeometry(0.34, 0.05, 8, 14), darkMat);
    halo.rotation.x = Math.PI / 2;
    halo.position.set(0, 0.9, -0.15);
    group.add(halo);

    const rollHoop = new THREE.Mesh(new THREE.BoxGeometry(0.32, 0.22, 0.5), darkMat);
    rollHoop.position.set(0, 0.95, 0.35);
    group.add(rollHoop);

    const wingFront = new THREE.Mesh(new THREE.BoxGeometry(2.4, 0.14, 0.35), wingMat);
    wingFront.position.set(0, 0.2, -1.95);
    group.add(wingFront);

    const wingFrontPlateL = new THREE.Mesh(new THREE.BoxGeometry(0.12, 0.4, 0.7), wingMat);
    wingFrontPlateL.position.set(1.2, 0.28, -1.85);
    const wingFrontPlateR = wingFrontPlateL.clone();
    wingFrontPlateR.position.x *= -1;
    group.add(wingFrontPlateL, wingFrontPlateR);

    const wingRear = new THREE.Mesh(new THREE.BoxGeometry(1.25, 0.55, 0.22), wingMat);
    wingRear.position.set(0, 1.05, 1.35);
    group.add(wingRear);

    const wingPillar = new THREE.Mesh(new THREE.BoxGeometry(0.18, 0.65, 0.18), darkMat);
    wingPillar.position.set(0, 0.7, 1.05);
    group.add(wingPillar);

    const fin = new THREE.Mesh(new THREE.BoxGeometry(0.12, 0.62, 0.95), bodyMat);
    fin.position.set(0, 0.72, 0.65);
    group.add(fin);

    const floor = new THREE.Mesh(new THREE.BoxGeometry(1.4, 0.08, 3.0), darkMat);
    floor.position.set(0, 0.1, 0.3);
    group.add(floor);

    const wheels = [];
    const addWheel = (x, z) => {
      const wheel = new THREE.Mesh(new THREE.CylinderGeometry(0.2, 0.2, 0.3, 14), darkMat);
      wheel.rotation.z = Math.PI / 2;
      wheel.position.set(x, 0.2, z);
      wheel.castShadow = true;
      group.add(wheel);
      wheels.push(wheel);
    };
    const halfBase = PHYS.wheel_base * 0.5;
    const halfTrack = 0.72;
    addWheel(halfTrack, -halfBase);
    addWheel(-halfTrack, -halfBase);
    addWheel(halfTrack, halfBase);
    addWheel(-halfTrack, halfBase);

    group.castShadow = true;
    group.receiveShadow = true;
    return { group, wheels };
  };

  class RaceCar {
    constructor(color, isPlayer) {
      const { group, wheels } = buildCarMesh(color);
      this.mesh = group;
      this.wheels = wheels;
      this.isPlayer = isPlayer;
      this.velocity = new THREE.Vector3();
      this.acceleration = new THREE.Vector3();
      this.steerInput = 0;
      this.radius = CAR_RADIUS;
      this.maxSteeringRad = degToRad(PHYS.max_steering_angle);
      this.stats = null;
      this.lastRoadIdx = null;
      this.distance = 0;
      this._steerCommand = 0;
      this.modelAlignQuat = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 1, 0), Math.PI);
      this.baseForward = new THREE.Vector3(0, 0, 1);
      this.lastBlocked = false;
      this.lastNormal = null;
    }

    setColor(color) {
      this.mesh.traverse((child) => {
        if (child.isMesh && child.material && child.material.color) {
          if (child.material === this.wheels[0]?.material) return;
          child.material = child.material.clone();
          child.material.color.setHex(color);
        }
      });
    }

    forward() {
      return new THREE.Vector3(0, 0, -1).applyQuaternion(this.mesh.quaternion).normalize();
    }

    back() {
      return new THREE.Vector3(0, 0, 1).applyQuaternion(this.mesh.quaternion).normalize();
    }

    reset(position, forwardDir) {
      const alignQuat = new THREE.Quaternion().setFromUnitVectors(this.baseForward, forwardDir.clone().normalize());
      this.mesh.position.copy(position);
      this.mesh.position.y = Math.max(this.mesh.position.y, 0.22);
      this.mesh.quaternion.copy(this.modelAlignQuat);
      this.mesh.quaternion.multiply(alignQuat);
      this.velocity.set(0, 0, 0);
      this.acceleration.set(0, 0, 0);
      this.steerInput = 0;
      this.lastRoadIdx = null;
      this.distance = null;
      this._steerCommand = 0;
      this.lastBlocked = false;
      this.lastNormal = null;
    }

    applyGrip(forward, dt) {
      const forwardComponent = forward.clone().multiplyScalar(this.velocity.dot(forward));
      const lateral = this.velocity.clone().sub(forwardComponent);
      const gripFactor = Math.max(0, 1 - PHYS.lateral_grip * dt);
      this.velocity.copy(forwardComponent.add(lateral.multiplyScalar(gripFactor)));
    }

    spinWheels(speed, dt, forwardSpeed) {
      if (!speed || !Number.isFinite(speed)) return;
      const rollDir = forwardSpeed >= 0 ? 1 : -1;
      const dv = speed * dt;
      this.wheels.forEach((w) => {
        w.rotation.x += dv * rollDir;
      });
    }

    update(dt, input, desiredDir = null, track = null) {
      const forward = this.forward();
      const up = new THREE.Vector3(0, 1, 0);

      let steerInput = 0;
      let throttle = 0;
      let brake = 0;

      if (input) {
        const left = input.left;
        const right = input.right;
        if (left) steerInput = -1;
        else if (right) steerInput = 1;
        else steerInput = 0;
        throttle = input.up ? 1 : 0;
        brake = input.down ? 1 : 0;
      } else if (desiredDir) {
        const aimDir = desiredDir.clone().normalize();
        steerInput = clamp(aimDir.clone().cross(forward).y * 2.0, -1, 1);
        throttle = PHYS.ai_throttle;
      }

      steerInput = clamp(steerInput, -1, 1);
      const steerSmooth = 1 - Math.exp(-PHYS.steering_response * dt);
      this.steerInput = THREE.MathUtils.lerp(this.steerInput, steerInput, steerSmooth);

      const forwardSpeed = this.velocity.dot(forward);
      let accelForward = throttle * PHYS.engine_power;
      if (brake > 0) {
        const brakingForce = forwardSpeed > 1.4 ? PHYS.braking_power : PHYS.engine_power * 0.7;
        accelForward -= brake * brakingForce;
      }

      const drag = this.velocity.clone().multiplyScalar(PHYS.drag_coefficient);
      const rolling = this.velocity.clone().multiplyScalar(PHYS.friction_coefficient);
      this.acceleration.copy(forward.clone().multiplyScalar(accelForward)).sub(drag).sub(rolling);

      this.applyGrip(forward, dt);
      this.velocity.add(this.acceleration.clone().multiplyScalar(dt));

      let speed = this.velocity.length();
      if (speed > PHYS.max_speed) {
        this.velocity.setLength(PHYS.max_speed);
        speed = PHYS.max_speed;
      }
      if (speed < PHYS.velocity_eps) {
        this.velocity.set(0, 0, 0);
        speed = 0;
      }

      const speedFactor = clamp(Math.abs(forwardSpeed) / 18, 0.35, 1.4);
      const yaw = this.steerInput * PHYS.steer_speed * dt * speedFactor;
      if (Math.abs(yaw) > 1e-5) {
        const rot = new THREE.Quaternion().setFromAxisAngle(up, yaw);
        this.mesh.quaternion.multiply(rot);
        this.velocity.applyQuaternion(rot);
      }

      const proposedPos = this.mesh.position.clone().add(this.velocity.clone().multiplyScalar(dt));
      let hitWall = false;
      let wallNormal = null;
      if (track) {
        const resolved = track.resolveMovement(this.mesh.position, proposedPos, this.radius);
        this.mesh.position.copy(resolved.position);
        hitWall = resolved.blocked;
        wallNormal = resolved.normal;
      } else {
        this.mesh.position.copy(proposedPos);
      }

      if (hitWall && wallNormal) {
        const n = wallNormal.clone().normalize();
        const intoWall = this.velocity.dot(n);
        if (intoWall > 0) this.velocity.addScaledVector(n, -intoWall);
        this.velocity.multiplyScalar(0.8);
      }

      this.lastBlocked = hitWall;
      this.lastNormal = wallNormal ? wallNormal.clone() : null;

      const currentForward = this.forward();
      const newForwardSpeed = this.velocity.dot(currentForward);
      if (newForwardSpeed < -PHYS.max_speed_reverse) {
        this.velocity.add(currentForward.multiplyScalar(-PHYS.max_speed_reverse - newForwardSpeed));
      }

      this.spinWheels(speed, dt, forwardSpeed);
    }
  }

  class Track {
    constructor(key, data, scene) {
      this.key = key;
      this.data = data;
      this.meta = data.meta || {};
      this.scene = scene;
      this.roadsCount = this.meta.roadsCount || (data.roads || []).length || 1;
      this.curve = this.buildCurve();
      const baseSamples = this.sampleCurve(Math.max(900, this.roadsCount * 44));
      const elevated = this.applyBridgeElevation(baseSamples);
      this.samples = elevated.samples;
      this.bridges = elevated.bridges;
      this.length = this.samples.totalLength;
      this.roadLength = this.length / this.roadsCount;
      this.group = new THREE.Group();
      this.startPoint = this.samples.points[0] || this.curve.getPointAt(0);
      const startNext = this.samples.points[1] || this.samples.points[0] || this.curve.getPointAt(0.001);
      const startDir = startNext.clone().sub(this.startPoint);
      this.startTangent = startDir.lengthSq() > 0 ? startDir.normalize() : new THREE.Vector3(1, 0, 0);
      this.halfTrackWidth = TRACK_WIDTH * 0.5;
      this.barrierClearance = this.halfTrackWidth + BARRIER_OFFSET;
      this.barrierSegments = { left: [], right: [] };
      this.waypoints = this.samples.points.slice(0, -1);
      this.waypointCount = this.waypoints.length;
      this.buildMeshes();
      this.scene.add(this.group);
    }

    dispose() {
      this.scene.remove(this.group);
    }

    buildCurve() {
      const ordered = [];
      const ranges = this.meta.pathRanges || [];
      const pathMap = {};
      (this.data.paths || []).forEach((p) => {
        pathMap[p.name] = p;
      });
      if (ranges.length) {
        ranges.forEach((r) => {
          const p = pathMap[r.path];
          if (p) ordered.push(p);
        });
      } else {
        ordered.push(...(this.data.paths || []));
      }

      const points = [];
      ordered.forEach((p, idx) => {
        const pts = [];
        for (let i = 2; i < p.points.length; i += 3) pts.push(vec3FromArr(p.points[i]));
        if (!pts.length) return;
        pts.forEach((pt, idp) => {
          if (idx > 0 && idp === 0) return;
          points.push(pt);
        });
      });
      if (!points.length) points.push(new THREE.Vector3(0, 0, 0));
      if (points[0].distanceTo(points[points.length - 1]) > 0.01) points.push(points[0].clone());
      let minX = Infinity;
      let maxX = -Infinity;
      let minZ = Infinity;
      let maxZ = -Infinity;
      points.forEach((p) => {
        minX = Math.min(minX, p.x);
        maxX = Math.max(maxX, p.x);
        minZ = Math.min(minZ, p.z);
        maxZ = Math.max(maxZ, p.z);
      });
      this.bounds = {
        min: [minX, 0, minZ],
        max: [maxX, 0, maxZ],
        center: new THREE.Vector3((minX + maxX) * 0.5, 0, (minZ + maxZ) * 0.5),
      };
      return new THREE.CatmullRomCurve3(points, true, "catmullrom", 0);
    }

    sampleCurve(segments) {
      const pts = [];
      const lengths = [0];
      let total = 0;
      let prev = this.curve.getPointAt(0);
      pts.push(prev);
      for (let i = 1; i <= segments; i += 1) {
        const t = i / segments;
        const p = this.curve.getPointAt(t);
        total += p.distanceTo(prev);
        lengths.push(total);
        pts.push(p);
        prev = p;
      }
      return { points: pts, lengths, totalLength: total, segments };
    }

    applyBridgeElevation(samples) {
      const pts = samples.points.map((p) => p.clone());
      const segCount = pts.length - 1;
      if (BRIDGE_HEIGHT <= 0 || segCount < 4) return { samples, bridges: [] };
      const crossings = this.detectCrossings(pts);
      if (!crossings.length) return { samples, bridges: [] };
      const toRaise = new Set();
      crossings.forEach((cross) => {
        toRaise.add(Math.max(cross.aIndex, cross.bIndex));
      });

      const ramp = Math.max(12, Math.floor(segCount * 0.015));
      const flat = Math.max(6, Math.floor(segCount * 0.006));
      const elevation = new Array(pts.length).fill(0);
      const bridges = [];

      const applySpan = (centerIdx) => {
        const peakHeight = BRIDGE_HEIGHT;
        const spanEnd = ramp + flat;
        for (let o = -spanEnd; o <= spanEnd; o += 1) {
          const idx = (centerIdx + o + pts.length) % pts.length;
          const dist = Math.max(0, Math.abs(o) - flat);
          const t = Math.min(1, 1 - dist / Math.max(1, ramp));
          const eased = 0.5 - 0.5 * Math.cos(t * Math.PI);
          elevation[idx] = Math.max(elevation[idx], eased * peakHeight);
        }
        bridges.push({ index: centerIdx, height: peakHeight });
      };

      toRaise.forEach(applySpan);

      let total = 0;
      const lengths = [0];
      for (let i = 0; i < pts.length; i += 1) {
        pts[i].y += elevation[i];
        if (i > 0) {
          total += pts[i].distanceTo(pts[i - 1]);
          lengths.push(total);
        }
      }

      return { samples: { points: pts, lengths, totalLength: total, segments: pts.length - 1 }, bridges };
    }

    detectCrossings(points) {
      const segCount = points.length - 1;
      const minGap = Math.max(10, Math.floor(segCount * 0.015));
      const crossings = [];
      for (let i = 0; i < segCount; i += 1) {
        const a1 = new THREE.Vector2(points[i].x, points[i].z);
        const a2 = new THREE.Vector2(points[i + 1].x, points[i + 1].z);
        for (let j = i + minGap; j < segCount; j += 1) {
          if (Math.abs(i - j) < minGap || (i === 0 && j === segCount - 1)) continue;
          const b1 = new THREE.Vector2(points[j].x, points[j].z);
          const b2 = new THREE.Vector2(points[j + 1].x, points[j + 1].z);
          if (Track.segmentsIntersect(a1, a2, b1, b2)) {
            crossings.push({ aIndex: i, bIndex: j });
          }
        }
      }
      return crossings;
    }

    static segmentsIntersect(a1, a2, b1, b2) {
      const den = (b2.y - b1.y) * (a2.x - a1.x) - (b2.x - b1.x) * (a2.y - a1.y);
      if (Math.abs(den) < 1e-6) return false;
      const ua = ((b2.x - b1.x) * (a1.y - b1.y) - (b2.y - b1.y) * (a1.x - b1.x)) / den;
      const ub = ((a2.x - a1.x) * (a1.y - b1.y) - (a2.y - a1.y) * (a1.x - b1.x)) / den;
      return ua > 0 && ua < 1 && ub > 0 && ub < 1;
    }

    nearestSample(position) {
      let best = 0;
      let bestDist = Infinity;
      const pts = this.samples.points;
      for (let i = 0; i < pts.length; i += 1) {
        const d = pts[i].distanceToSquared(position);
        if (d < bestDist) {
          bestDist = d;
          best = i;
        }
      }
      return { index: best, distance: this.samples.lengths[best], point: pts[best] };
    }

    roadIndexAtDistance(distance) {
      const wrapped = ((distance % this.length) + this.length) % this.length;
      return Math.floor((wrapped / this.length) * this.roadsCount) % this.roadsCount;
    }

    tangentAt(distance) {
      if (!this.samples?.points?.length) return new THREE.Vector3(1, 0, 0);
      const wrapped = ((distance % this.length) + this.length) % this.length;
      const segIdx = Math.floor((wrapped / this.length) * this.samples.segments) % this.samples.segments;
      const p1 = this.samples.points[segIdx];
      const p2 = this.samples.points[segIdx + 1];
      const dir = p2.clone().sub(p1);
      if (dir.lengthSq() === 0) return new THREE.Vector3(1, 0, 0);
      return dir.normalize();
    }

    pathDirectionForRoad(roadIdx, pos) {
      const ranges = this.meta.pathRanges || [];
      const pathName = ranges.find((r) => roadIdx >= r.from && roadIdx <= r.to)?.path || (this.data.paths?.[0]?.name ?? null);
      const pathData = (this.data.paths || []).find((p) => p.name === pathName) || this.data.paths?.[0];
      if (!pathData) return this.tangentAt(roadIdx * this.roadLength);
      const pts = [];
      for (let i = 2; i < pathData.points.length; i += 3) pts.push(vec3FromArr(pathData.points[i]));
      if (!pts.length) return this.tangentAt(roadIdx * this.roadLength);
      const curve = new THREE.CatmullRomCurve3(pts, true, "catmullrom", 0);
      let closestT = 0;
      let bestDist = Infinity;
      const steps = 120;
      for (let i = 0; i <= steps; i += 1) {
        const t = i / steps;
        const p = curve.getPointAt(t);
        const d = p.distanceToSquared(pos);
        if (d < bestDist) {
          bestDist = d;
          closestT = t;
        }
      }
      return curve.getTangentAt(closestT).normalize();
    }

    closestWaypointIndex(position) {
      if (!this.waypoints.length) return 0;
      let best = 0;
      let bestDist = Infinity;
      for (let i = 0; i < this.waypoints.length; i += 1) {
        const d = this.waypoints[i].distanceToSquared(position);
        if (d < bestDist) {
          bestDist = d;
          best = i;
        }
      }
      return best;
    }

    waypointDirection(idx, position) {
      if (!this.waypoints.length) return { dir: new THREE.Vector3(0, 0, 0), dist: 0 };
      const wp = this.waypoints[idx % this.waypointCount];
      const dir = wp.clone().sub(position);
      dir.y = 0;
      const dist = dir.length();
      if (dist > 0) dir.divideScalar(dist);
      return { dir, dist };
    }

    nextWaypoint(idx) {
      if (!this.waypoints.length) return 0;
      return (idx + 1) % this.waypoints.length;
    }

    resolveMovement(currentPos, proposedPos, radius = CAR_RADIUS) {
      const nearest = this.nearestSample(proposedPos);
      const idx = nearest.index % this.sampled.normals2.length;
      const center = this.sampled.pts[idx];
      const normal2 = this.sampled.normals2[idx];
      const offset = new THREE.Vector2(proposedPos.x - center.x, proposedPos.z - center.z);
      const trackHeight = Math.max(center.y, 0);
      const lateral = offset.dot(normal2);
      const lateralLimit = Math.max(this.barrierClearance - (radius + BARRIER_THICKNESS * 0.5), this.halfTrackWidth * 0.85);

      let blocked = false;
      let normal = null;
      let corrected = proposedPos.clone();

      if (Math.abs(lateral) > lateralLimit) {
        blocked = true;
        const sign = Math.sign(lateral) || 1;
        const clamped = sign * lateralLimit;
        corrected = new THREE.Vector3(
          center.x + normal2.x * clamped,
          trackHeight + 0.18,
          center.z + normal2.y * clamped
        );
        normal = new THREE.Vector3(normal2.x * -sign, 0, normal2.y * -sign);
      } else {
        const pos2 = new THREE.Vector2(proposedPos.x, proposedPos.z);
        const leftSegs = this.barrierSegments.left;
        const rightSegs = this.barrierSegments.right;
        if (leftSegs.length && rightSegs.length) {
          const segIdx = Math.max(0, Math.min(leftSegs.length - 1, idx));
          const rightIdx = Math.max(0, Math.min(rightSegs.length - 1, idx));
          const segmentsToCheck = [
            leftSegs[segIdx],
            rightSegs[rightIdx],
            leftSegs[(segIdx + 1) % leftSegs.length],
            rightSegs[(rightIdx + 1) % rightSegs.length],
            leftSegs[(segIdx + leftSegs.length - 1) % leftSegs.length],
            rightSegs[(rightIdx + rightSegs.length - 1) % rightSegs.length],
          ];
          for (const seg of segmentsToCheck) {
            if (!seg) continue;
            const local = pos2.clone().sub(seg.center);
            const along = local.dot(seg.tangent);
            const across = local.dot(seg.normal);
            if (Math.abs(along) <= seg.halfLength && Math.abs(across) <= seg.halfDepth + radius) {
              blocked = true;
              corrected = new THREE.Vector3(
                seg.center.x + seg.normal.x * (seg.halfDepth + radius) * Math.sign(across || 1),
                trackHeight + 0.18,
                seg.center.y + seg.normal.y * (seg.halfDepth + radius) * Math.sign(across || 1)
              );
              normal = new THREE.Vector3(seg.normal.x, 0, seg.normal.y);
              break;
            }
          }
        }
      }

      const targetY = trackHeight + 0.18;
      corrected.y = targetY;
      return { position: corrected, blocked, normal };
    }

    buildStrip(left, right, color, height = 0.02) {
      const geo = new THREE.BufferGeometry();
      const verts = [];
      const normals = [];
      const colors = [];
      const c = new THREE.Color(color);
      for (let i = 0; i < left.length - 1; i += 1) {
        const l1 = left[i];
        const l2 = left[i + 1];
        const r1 = right[i];
        const r2 = right[i + 1];
        const quad = [l1, l2, r2, l1, r2, r1];
        quad.forEach((p) => {
          verts.push(p.x, p.y + height, p.z);
          normals.push(0, 1, 0);
          colors.push(c.r, c.g, c.b);
        });
      }
      geo.setAttribute("position", new THREE.Float32BufferAttribute(verts, 3));
      geo.setAttribute("normal", new THREE.Float32BufferAttribute(normals, 3));
      geo.setAttribute("color", new THREE.Float32BufferAttribute(colors, 3));
      return geo;
    }

    buildKerbStrip(left, right, height = 0.02) {
      const geo = new THREE.BufferGeometry();
      const verts = [];
      const normals = [];
      const colors = [];
      for (let i = 0; i < left.length - 1; i += 1) {
        const l1 = left[i];
        const l2 = left[i + 1];
        const r1 = right[i];
        const r2 = right[i + 1];
        const c = new THREE.Color(i % 2 === 0 ? PALETTE.kerbRed : PALETTE.kerbWhite);
        const quad = [l1, l2, r2, l1, r2, r1];
        quad.forEach((p) => {
          verts.push(p.x, p.y + height, p.z);
          normals.push(0, 1, 0);
          colors.push(c.r, c.g, c.b);
        });
      }
      geo.setAttribute("position", new THREE.Float32BufferAttribute(verts, 3));
      geo.setAttribute("normal", new THREE.Float32BufferAttribute(normals, 3));
      geo.setAttribute("color", new THREE.Float32BufferAttribute(colors, 3));
      return geo;
    }

    buildMeshes() {
      const group = this.group;
      const env = this.meta.environment || {};
      const skyBottomColor = env.sky_horizon_color ? new THREE.Color().fromArray(env.sky_horizon_color.slice(0, 3)) : new THREE.Color(PALETTE.skyFallback);
      const skyTopColor = env.sky_top_color ? new THREE.Color().fromArray(env.sky_top_color.slice(0, 3)) : new THREE.Color(0x9fc3ff);
      this.scene.background = skyBottomColor.clone();
      if (!this.scene.fog) {
        this.scene.fog = new THREE.Fog(this.scene.background, 220, 760);
      } else {
        this.scene.fog.color = this.scene.background;
        this.scene.fog.near = 220;
        this.scene.fog.far = 760;
      }
      const skyMaterial = new THREE.ShaderMaterial({
        uniforms: {
          topColor: { value: skyTopColor },
          bottomColor: { value: skyBottomColor },
        },
        vertexShader: `varying vec3 vWorldPosition; void main(){ vec4 wp = modelMatrix * vec4(position,1.0); vWorldPosition = wp.xyz; gl_Position = projectionMatrix * viewMatrix * wp; }`,
        fragmentShader: `varying vec3 vWorldPosition; uniform vec3 topColor; uniform vec3 bottomColor; void main(){ float h = clamp(normalize(vWorldPosition).y*0.5+0.5,0.0,1.0); gl_FragColor = vec4(mix(bottomColor, topColor, h),1.0); }`,
        side: THREE.BackSide,
        depthWrite: false,
      });
      const sky = new THREE.Mesh(new THREE.SphereGeometry(520, 24, 16), skyMaterial);
      group.add(sky);

      const pts = this.samples.points;
      const tangents = [];
      const normals2 = [];
      const count = pts.length;
      for (let i = 0; i < count; i += 1) {
        const prev = pts[(i - 1 + count) % count];
        const next = pts[(i + 1) % count];
        const tan = next.clone().sub(prev);
        if (tan.lengthSq() === 0) tan.set(1, 0, 0);
        else tan.normalize();
        const dir2 = new THREE.Vector2(tan.x, tan.z).normalize();
        const normal2 = new THREE.Vector2(-dir2.y, dir2.x);
        tangents.push(tan);
        normals2.push(normal2);
      }
      this.sampled = { pts, tangents, normals2 };

      const width = TRACK_WIDTH * 0.5;
      const left = [];
      const right = [];
      const kerbLeft = [];
      const kerbRight = [];
      const barrierLeft = [];
      const barrierRight = [];
      for (let i = 0; i < pts.length; i += 1) {
        const p = pts[i];
        const n2 = normals2[i];
        left.push(new THREE.Vector3(p.x + n2.x * width, p.y, p.z + n2.y * width));
        right.push(new THREE.Vector3(p.x - n2.x * width, p.y, p.z - n2.y * width));
        kerbLeft.push(new THREE.Vector3(p.x + n2.x * (width + KERB_EXTRA), p.y + 0.01, p.z + n2.y * (width + KERB_EXTRA)));
        kerbRight.push(new THREE.Vector3(p.x - n2.x * (width + KERB_EXTRA), p.y + 0.01, p.z - n2.y * (width + KERB_EXTRA)));
        barrierLeft.push(new THREE.Vector3(p.x + n2.x * this.barrierClearance, p.y + 0.05, p.z + n2.y * this.barrierClearance));
        barrierRight.push(new THREE.Vector3(p.x - n2.x * this.barrierClearance, p.y + 0.05, p.z - n2.y * this.barrierClearance));
      }

      const roadGeo = this.buildStrip(left, right, PALETTE.asphalt, 0.001);
      const roadMat = new THREE.MeshStandardMaterial({ vertexColors: true, roughness: 0.6, metalness: 0.05 });
      const road = new THREE.Mesh(roadGeo, roadMat);
      road.receiveShadow = true;
      group.add(road);

      const kerbGeo = this.buildKerbStrip(kerbLeft, kerbRight, 0.002);
      const kerbMat = new THREE.MeshStandardMaterial({ vertexColors: true, roughness: 0.5, metalness: 0.04 });
      const kerb = new THREE.Mesh(kerbGeo, kerbMat);
      kerb.receiveShadow = true;
      kerb.castShadow = true;
      group.add(kerb);

      const railMat = new THREE.MeshStandardMaterial({ color: PALETTE.rail, roughness: 0.35, metalness: 0.35 });
      const guard = (points) => {
        const curve = new THREE.CatmullRomCurve3(points.map((p) => new THREE.Vector3(p.x, p.y + 0.3, p.z)), true);
        const geo = new THREE.TubeGeometry(curve, pts.length, 0.12, 8, true);
        const mesh = new THREE.Mesh(geo, railMat);
        mesh.castShadow = true;
        return mesh;
      };
      group.add(guard(barrierLeft), guard(barrierRight));

      if (this.bridges.length) {
        const supportGroup = new THREE.Group();
        const pillarMat = new THREE.MeshStandardMaterial({ color: 0x8a8c94, roughness: 0.62, metalness: 0.28 });
        const beamMat = new THREE.MeshStandardMaterial({ color: 0x676a72, roughness: 0.45, metalness: 0.35 });
        const beamGeo = new THREE.BoxGeometry(TRACK_WIDTH * 1.8, 0.24, 0.7);
        const pillarGeo = new THREE.CylinderGeometry(0.55, 0.7, 1, 12);
        const spanSteps = [-4, 0, 4];
        const pillarLateralOffset = this.barrierClearance + 3.5;
        const lateralOffsets = [-pillarLateralOffset, pillarLateralOffset];
        this.bridges.forEach((bridge) => {
          spanSteps.forEach((step) => {
            const idx = (bridge.index + step + pts.length) % pts.length;
            const p = pts[idx];
            const n2 = normals2[idx];
            const beam = new THREE.Mesh(beamGeo, beamMat);
            beam.position.set(p.x, Math.max(0.05, p.y - 0.05), p.z);
            beam.castShadow = true;
            beam.receiveShadow = true;
            supportGroup.add(beam);
            lateralOffsets.forEach((lat) => {
              const pillarHeight = Math.max(1.4, p.y + 0.4);
              const pillar = new THREE.Mesh(pillarGeo, pillarMat);
              pillar.scale.y = pillarHeight;
              pillar.position.set(p.x + n2.x * lat, pillarHeight * 0.5 - 0.02, p.z + n2.y * lat);
              pillar.castShadow = true;
              pillar.receiveShadow = true;
              supportGroup.add(pillar);
            });
          });
        });
        group.add(supportGroup);
      }

      const margin = 80;
      const bounds = this.bounds || this.meta.bounds;
      const minX = bounds?.min?.[0] ?? -60;
      const maxX = bounds?.max?.[0] ?? 60;
      const minZ = bounds?.min?.[2] ?? -60;
      const maxZ = bounds?.max?.[2] ?? 60;
      const centerX = (minX + maxX) * 0.5;
      const centerZ = (minZ + maxZ) * 0.5;
      const groundW = (maxX - minX) + margin;
      const groundH = (maxZ - minZ) + margin;

      const ground = new THREE.Mesh(
        new THREE.PlaneGeometry(groundW, groundH),
        new THREE.MeshStandardMaterial({ color: PALETTE.grass, roughness: 0.95 })
      );
      ground.rotation.x = -Math.PI / 2;
      ground.position.set(centerX, -0.02, centerZ);
      ground.receiveShadow = true;
      group.add(ground);

      const sand = new THREE.Mesh(
        new THREE.PlaneGeometry(groundW * 1.1, groundH * 1.1),
        new THREE.MeshStandardMaterial({ color: PALETTE.sand, roughness: 1 })
      );
      sand.rotation.x = -Math.PI / 2;
      sand.position.set(centerX, -0.04, centerZ);
      sand.receiveShadow = true;
      group.add(sand);

      const startPerp = new THREE.Vector2(-this.startTangent.z, this.startTangent.x).normalize();
      const stripe = new THREE.Mesh(
        new THREE.PlaneGeometry(TRACK_WIDTH + 1.2, 0.45),
        new THREE.MeshStandardMaterial({ color: 0xffffff, transparent: true, opacity: 0.9 })
      );
      stripe.rotation.x = -Math.PI / 2;
      stripe.rotation.y = Math.atan2(startPerp.x, startPerp.y);
      stripe.position.set(this.startPoint.x, this.startPoint.y + 0.01, this.startPoint.z);
      group.add(stripe);

      const standMat = new THREE.MeshStandardMaterial({ color: PALETTE.stand, roughness: 0.7, metalness: 0.12 });
      const standGeo = new THREE.BoxGeometry(6, 2.4, 2.6);
      const standA = new THREE.Mesh(standGeo, standMat);
      standA.position.set(this.startPoint.x + startPerp.x * (TRACK_WIDTH * 3.2), 1.2, this.startPoint.z + startPerp.y * (TRACK_WIDTH * 3.2));
      standA.castShadow = true;
      standA.receiveShadow = true;
      const standB = standA.clone();
      standB.position.set(this.startPoint.x - startPerp.x * (TRACK_WIDTH * 3.6), 1.2, this.startPoint.z - startPerp.y * (TRACK_WIDTH * 3.6));
      group.add(standA, standB);

      const lightsRig = new THREE.Group();
      lightsRig.position.copy(this.startPoint);
      const startYaw = Math.atan2(-this.startTangent.x, -this.startTangent.z);
      lightsRig.rotation.y = startYaw;
      const mastHeight = 3.2;
      const span = this.barrierClearance + BARRIER_THICKNESS;
      const mastMat = new THREE.MeshStandardMaterial({ color: 0x333333, metalness: 0.35, roughness: 0.5 });
      const mastGeo = new THREE.BoxGeometry(0.28, mastHeight, 0.28);
      const mastLeft = new THREE.Mesh(mastGeo, mastMat);
      mastLeft.position.set(span, mastHeight * 0.5, 0);
      const mastRight = mastLeft.clone();
      mastRight.position.x = -span;
      const bar = new THREE.Mesh(new THREE.BoxGeometry(span * 2, 0.24, 0.35), new THREE.MeshStandardMaterial({ color: 0x444444, metalness: 0.45, roughness: 0.42 }));
      bar.position.set(0, mastHeight - 0.2, 0);
      lightsRig.add(mastLeft, mastRight, bar);
      const lightSpread = Math.min(3.6, span * 1.4);
      const lightStart = -lightSpread * 0.5;
      for (let i = 0; i < 5; i += 1) {
        const light = new THREE.Mesh(new THREE.SphereGeometry(0.14, 14, 10), new THREE.MeshStandardMaterial({ color: 0x550000, emissive: 0x220000 }));
        light.position.set(lightStart + (lightSpread / 4) * i, mastHeight - 0.2, 0);
        lightsRig.add(light);
      }
      group.add(lightsRig);

      this.barrierSegments = { left: [], right: [] };
      const addSegments = (points, target, inwardSign) => {
        for (let i = 0; i < points.length - 1; i += 1) {
          const a = points[i];
          const b = points[i + 1];
          const dir = new THREE.Vector2(b.x - a.x, b.z - a.z);
          const length = dir.length();
          if (length < 1e-3) continue;
          const tangent = dir.clone().normalize();
          const normal = new THREE.Vector2(-tangent.y, tangent.x).multiplyScalar(inwardSign);
          target.push({
            center: new THREE.Vector2((a.x + b.x) * 0.5, (a.z + b.z) * 0.5),
            tangent,
            normal,
            halfLength: length * 0.5,
            halfDepth: BARRIER_THICKNESS * 0.5,
          });
        }
      };
      addSegments(barrierLeft, this.barrierSegments.left, -1);
      addSegments(barrierRight, this.barrierSegments.right, 1);

      const envGroup = new THREE.Group();
      envGroup.name = "track-environment";
      const treeProto = new THREE.Group();
      const trunk = new THREE.Mesh(new THREE.CylinderGeometry(0.25, 0.35, 1.1, 8), new THREE.MeshStandardMaterial({ color: PALETTE.treeTrunk, roughness: 0.9 }));
      trunk.position.y = 0.55;
      const leaves = new THREE.Mesh(new THREE.ConeGeometry(0.95, 2.6, 10), new THREE.MeshStandardMaterial({ color: PALETTE.treeLeaves, roughness: 0.65 }));
      leaves.position.y = 2.1;
      leaves.castShadow = true;
      trunk.castShadow = true;
      trunk.receiveShadow = true;
      treeProto.add(trunk, leaves);

      const tentProto = new THREE.Mesh(new THREE.ConeGeometry(1.2, 1.1, 6), new THREE.MeshStandardMaterial({ color: 0xdbe7ff, roughness: 0.6, metalness: 0.08 }));
      tentProto.position.y = 0.55;
      tentProto.castShadow = true;
      tentProto.receiveShadow = true;

      const lightProto = new THREE.Group();
      const pole = new THREE.Mesh(new THREE.CylinderGeometry(0.08, 0.1, 4.2, 10), new THREE.MeshStandardMaterial({ color: 0x555b66, metalness: 0.32, roughness: 0.55 }));
      pole.position.y = 2.1;
      const lamp = new THREE.Mesh(new THREE.SphereGeometry(0.18, 10, 8), new THREE.MeshStandardMaterial({ color: 0xfdf7c3, emissive: 0x5f5633 }));
      lamp.position.y = 4.1;
      lightProto.add(pole, lamp);

      const ringOffset = this.barrierClearance + 9;
      const sampleStep = Math.max(14, Math.floor(pts.length / 18));
      for (let i = 0; i < pts.length; i += sampleStep) {
        const p = pts[i];
        const n2 = normals2[i];
        const sideJitter = (i / sampleStep) % 2 === 0 ? 2.8 : -2.1;
        const offset = ringOffset + sideJitter;
        const leftTree = treeProto.clone();
        leftTree.position.set(p.x + n2.x * offset, 0, p.z + n2.y * offset);
        const rightTree = treeProto.clone();
        rightTree.position.set(p.x - n2.x * offset, 0, p.z - n2.y * offset);
        envGroup.add(leftTree, rightTree);

        if (i % (sampleStep * 2) === 0) {
          const tent = tentProto.clone();
          tent.position.set(p.x + n2.x * (ringOffset + 5.5), 0, p.z + n2.y * (ringOffset + 5.5));
          envGroup.add(tent);
        }
      }

      const lightStep = Math.max(18, Math.floor(pts.length / 24));
      for (let i = 0; i < pts.length; i += lightStep) {
        const p = pts[i];
        const n2 = normals2[i];
        const offset = this.barrierClearance + 5.5;
        const poleLeft = lightProto.clone();
        poleLeft.position.set(p.x + n2.x * offset, 0, p.z + n2.y * offset);
        const poleRight = lightProto.clone();
        poleRight.position.set(p.x - n2.x * offset, 0, p.z - n2.y * offset);
        envGroup.add(poleLeft, poleRight);
      }

      const buildingGeo = new THREE.BoxGeometry(7.5, 4.2, 10.5);
      const buildingMat = new THREE.MeshStandardMaterial({ color: 0x9b9ea6, roughness: 0.5, metalness: 0.2 });
      const buildingSpots = [
        [minX - 14, centerZ],
        [maxX + 16, centerZ + 24],
        [centerX, minZ - 18],
        [centerX - 12, maxZ + 22],
      ];
      buildingSpots.forEach(([x, z], idx) => {
        const b = new THREE.Mesh(buildingGeo, buildingMat);
        b.position.set(x, 2.1 + (idx % 2 === 0 ? 0.4 : 0), z);
        b.castShadow = true;
        b.receiveShadow = true;
        envGroup.add(b);
      });

      group.add(envGroup);
    }
  }

  class Game {
    constructor(config) {
      this.canvas = config.canvas;
      this.modeInputs = config.modeInputs;
      this.circuitSelect = config.circuitSelect;
      this.lapSelect = config.lapSelect;
      this.carButtons = config.carButtons;
      this.startButton = config.startButton;
      this.restartButton = config.restartButton;
      this.hud = config.hud || {};
      this.leaderboard = config.leaderboard;
      this.countdownEl = config.countdownEl;
      this.isAuthenticated = !!config.isAuthenticated;
      this.bestResult = config.bestResult || null;
      this.bestResultEls = config.bestResultEls || {};
      this.resultSubmitted = false;

      this.mode = MODE.TIME;
      this.selectedCar = "red";
      this.circuitKey = Object.keys(TRACKS)[0];
      this.totalLaps = 2;

      this.scene = new THREE.Scene();
      this.camera = new THREE.PerspectiveCamera(60, 16 / 9, 0.1, 900);
      this.camera.up.set(0, 1, 0);
      this.renderer = new THREE.WebGLRenderer({ canvas: this.canvas, antialias: true });
      this.renderer.shadowMap.enabled = true;
      this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
      this.clock = new THREE.Clock();

      this.input = { up: false, down: false, left: false, right: false };
      this.state = "idle";
      this.countdown = 0;
      this.goTimer = 0;
      this.racers = [];
      this.circuit = null;

      this.setupLights();
      this.bindUI();
      this.populateCircuits();
      this.loadCircuit(this.circuitKey);
      this.resetSession();
      this.resize();
      this.renderBestResult(this.bestResult);
      window.addEventListener("resize", () => this.resize());
      this.loop();
    }

    setupLights() {
      const ambient = new THREE.AmbientLight(0xffffff, 0.18);
      this.scene.add(ambient);
      const hemi = new THREE.HemisphereLight(0xffffff, 0xb4b4b4, 0.6);
      this.scene.add(hemi);
      const dir = new THREE.DirectionalLight(0xffffff, 0.9);
      dir.position.set(45, 70, 30);
      dir.castShadow = true;
      dir.shadow.mapSize.set(1024, 1024);
      dir.shadow.camera.near = 10;
      dir.shadow.camera.far = 300;
      dir.shadow.camera.left = -120;
      dir.shadow.camera.right = 120;
      dir.shadow.camera.top = 120;
      dir.shadow.camera.bottom = -120;
      this.scene.add(dir);
    }

    bindUI() {
      this.modeInputs.forEach((i) =>
        i.addEventListener("change", () => {
          this.mode = i.value === "race" ? MODE.RACE : MODE.TIME;
          this.resetSession();
        })
      );
      this.carButtons.forEach((btn) => {
        btn.addEventListener("click", () => {
          this.carButtons.forEach((b) => b.classList.remove("active"));
          btn.classList.add("active");
          this.selectedCar = btn.dataset.f1Car;
          this.resetSession();
        });
      });
      this.startButton?.addEventListener("click", () => this.start());
      this.restartButton?.addEventListener("click", () => this.resetSession());
      this.canvas.addEventListener("click", () => this.canvas.focus());
      window.addEventListener("keydown", (e) => this.onKey(e, true));
      window.addEventListener("keyup", (e) => this.onKey(e, false));
      this.circuitSelect?.addEventListener("change", () => {
        this.circuitKey = this.circuitSelect.value;
        this.loadCircuit(this.circuitKey);
        this.resetSession();
      });
      this.lapSelect?.addEventListener("change", () => {
        this.totalLaps = this.lapCount();
        this.resetSession();
      });
    }

    populateCircuits() {
      if (!this.circuitSelect) return;
      this.circuitSelect.innerHTML = "";
      Object.entries(TRACKS).forEach(([key, val]) => {
        const opt = document.createElement("option");
        opt.value = key;
        opt.textContent = val.meta?.displayName || key;
        this.circuitSelect.appendChild(opt);
      });
      this.circuitSelect.value = this.circuitKey;
    }

    lapCount() {
      const val = parseInt(this.lapSelect?.value ?? "0", 10);
      return clamp(Number.isNaN(val) ? 2 : val, 1, 6);
    }

    loadCircuit(key) {
      if (this.circuit) this.circuit.dispose();
      this.circuit = new Track(key, TRACKS[key], this.scene);
    }

    resize() {
      const rect = this.canvas.getBoundingClientRect();
      const ratio = rect.width / rect.height;
      this.camera.aspect = ratio || 16 / 9;
      this.camera.updateProjectionMatrix();
      this.renderer.setSize(rect.width, rect.height, false);
    }

    onKey(e, down) {
      const key = e.key.toLowerCase();
      if (key === "w" || key === "arrowup") this.input.up = down;
      if (key === "s" || key === "arrowdown") this.input.down = down;
      if (key === "a" || key === "arrowleft") this.input.left = down;
      if (key === "d" || key === "arrowright") this.input.right = down;
    }

    resetSession() {
      this.totalLaps = this.lapCount();
      this.racers.forEach((r) => this.scene.remove(r.mesh));
      this.racers = [];
      const player = new RaceCar(PALETTE.cars[this.selectedCar] || PALETTE.cars.red, true);
      this.scene.add(player.mesh);
      this.racers.push(player);

      if (this.mode === MODE.RACE) {
        const aiColors = [PALETTE.cars.green, PALETTE.cars.orange, PALETTE.cars.white];
        for (let i = 0; i < 3; i += 1) {
          const ai = new RaceCar(aiColors[i % aiColors.length], false);
          this.scene.add(ai.mesh);
          this.racers.push(ai);
        }
      }

      const starts = this.circuit.data.start_positions || [];
      const startForward = this.circuit.startTangent.clone();
      const startPos = (idx) => {
        const sp = starts[idx % starts.length];
        if (!sp) return new THREE.Vector3();
        return vec3FromArr(sp.transform.slice(9, 12));
      };
      this.racers.forEach((r, idx) => {
        r.stats = new LapCounter(this.circuit.roadsCount, this.totalLaps, this.circuit.length);
        r.reset(startPos(idx), startForward);
        r.targetWaypoint = this.circuit.closestWaypointIndex(r.mesh.position);
      });
      this.state = "idle";
      this.countdown = 0;
      this.goTimer = 0;
      this.circuitStartTime = 0;
      this.resultSubmitted = false;
      this.setStatus("Ready");
      this.renderCountdown("", true);
      this.updateHUD();
    }

    start() {
      this.resetSession();
      this.countdown = 3.0;
      this.state = "countdown";
      this.renderCountdown("3");
      this.setStatus("3...");
    }

    loop() {
      requestAnimationFrame(() => this.loop());
      const dt = this.clock.getDelta();
      this.update(dt);
      this.renderer.render(this.scene, this.camera);
    }

    update(dt) {
      if (this.state === "countdown") {
        this.countdown -= dt;
        const tick = Math.max(1, Math.ceil(this.countdown));
        this.setStatus(`${tick}...`);
        this.renderCountdown(`${tick}`);
        if (this.countdown <= 0) {
          this.state = "running";
          this.circuitStartTime = 0;
          this.renderCountdown("GO");
          this.goTimer = 0.8;
          this.setStatus("Go!");
        }
      }

      if (this.state === "running") {
        this.circuitStartTime += dt;
        this.racers.forEach((r, idx) => {
          if (!r.stats.finished()) {
            if (r.isPlayer) {
              r.update(dt, this.input, null, this.circuit);
            } else {
              let targetIdx = typeof r.targetWaypoint === "number" ? r.targetWaypoint : this.circuit.closestWaypointIndex(r.mesh.position);
              let wpInfo = this.circuit.waypointDirection(targetIdx, r.mesh.position);
              const advanceThreshold = Math.max(this.circuit.halfTrackWidth * 0.8, 6);
              let guard = 0;
              while (wpInfo.dist < advanceThreshold && guard < 4) {
                targetIdx = this.circuit.nextWaypoint(targetIdx);
                wpInfo = this.circuit.waypointDirection(targetIdx, r.mesh.position);
                guard += 1;
              }
              r.targetWaypoint = targetIdx;
              const lookAheadIdx = this.circuit.waypointCount ? (targetIdx + 6) % this.circuit.waypointCount : targetIdx;
              const lookTarget = this.circuit.waypoints[lookAheadIdx] ?? null;
              let desiredDir = wpInfo.dir.lengthSq() > 0 ? wpInfo.dir.clone() : this.circuit.pathDirectionForRoad(r.stats.current_road_idx(), r.mesh.position).clone();
              if (lookTarget) {
                const lookVec = lookTarget.clone().sub(r.mesh.position);
                if (lookVec.lengthSq() > 1e-4) desiredDir = lookVec.normalize();
              }
              const nearest = this.circuit.nearestSample(r.mesh.position);
              const tangentCount = this.circuit.sampled?.tangents?.length ?? 0;
              const tangent = tangentCount > 0 ? this.circuit.sampled.tangents[nearest.index % tangentCount] : this.circuit.tangentAt(nearest.distance || 0);
              const centerCount = this.circuit.sampled?.pts?.length ?? 0;
              const normalCount = this.circuit.sampled?.normals2?.length ?? 0;
              const center = centerCount ? this.circuit.sampled.pts[nearest.index % centerCount] : null;
              const normal2 = normalCount ? this.circuit.sampled.normals2[nearest.index % normalCount] : null;
              if (tangent) desiredDir = desiredDir.multiplyScalar(0.7).add(tangent.clone().multiplyScalar(0.3));
              if (center && normal2) {
                const lateral = new THREE.Vector2(r.mesh.position.x - center.x, r.mesh.position.z - center.z).dot(normal2);
                desiredDir.add(new THREE.Vector3(-normal2.x * lateral * 0.06, 0, -normal2.y * lateral * 0.06));
              }
              if (r.lastBlocked && r.lastNormal) desiredDir.add(r.lastNormal.clone().multiplyScalar(-0.6));
              if (desiredDir.lengthSq() === 0) desiredDir = this.circuit.tangentAt(nearest.distance || 0);
              desiredDir.normalize();
              r._steerCommand = desiredDir;
              r.update(dt, null, desiredDir, this.circuit);
            }
          } else {
            r.update(dt, { up: false, down: false, left: false, right: false }, null, this.circuit);
            r.velocity.multiplyScalar(0.96);
          }

          const nearest = this.circuit.nearestSample(r.mesh.position);
          let distance = nearest.distance;
          if (Number.isFinite(r.distance)) {
            const diff = distance - (r.distance % this.circuit.length);
            if (diff < -this.circuit.length * 0.5) distance += this.circuit.length;
            else if (diff > this.circuit.length * 0.5) distance -= this.circuit.length;
          }
          r.distance = distance;
          const wrapped = ((distance % this.circuit.length) + this.circuit.length) % this.circuit.length;
          const tangent = this.circuit.tangentAt(wrapped);
          const forwardDot = r.forward().dot(tangent);
          r.stats.updateProgress(distance, this.circuitStartTime, forwardDot);
        });

        if (this.racers[0].stats.finished()) {
          this.state = "finished";
          this.setStatus("Finished");
          this.renderCountdown("", true);
          this.submitBestResult();
        }
        if (this.goTimer > 0) {
          this.goTimer -= dt;
          if (this.goTimer <= 0) this.renderCountdown("", true);
        }
      } else if (this.goTimer > 0) {
        this.goTimer -= dt;
        if (this.goTimer <= 0) this.renderCountdown("", true);
      }

      this.updateCamera(dt);
      this.updateHUD();
    }

    updateCamera(dt) {
      const player = this.racers[0];
      const forward = player.forward();
      const desiredPos = player.mesh.position.clone().sub(forward.clone().multiplyScalar(10)).add(new THREE.Vector3(0, 4.5, 0));
      const followLerp = 1 - Math.exp(-6 * dt);
      this.camera.position.lerp(desiredPos, followLerp);

      const rawTarget = player.mesh.position.clone().add(forward.clone().multiplyScalar(6)).add(new THREE.Vector3(0, 1.2, 0));
      const toTarget = rawTarget.clone().sub(this.camera.position);
      const planar = Math.max(0.001, Math.hypot(toTarget.x, toTarget.z));
      const pitch = Math.atan2(toTarget.y, planar);
      const clampedPitch = clamp(pitch, degToRad(-5), degToRad(65));
      const yaw = Math.atan2(toTarget.z, toTarget.x);
      const dist = toTarget.length() || 1;
      const lookDir = new THREE.Vector3(
        Math.cos(yaw) * Math.cos(clampedPitch),
        Math.sin(clampedPitch),
        Math.sin(yaw) * Math.cos(clampedPitch)
      ).multiplyScalar(dist);
      const lookPoint = this.camera.position.clone().add(lookDir);

      this.camera.up.set(0, 1, 0);
      this.camera.lookAt(lookPoint);
    }

    updateHUD() {
      const player = this.racers[0];
      const speed = player.velocity.length() * KMH;
      if (this.hud.speed) this.hud.speed.textContent = Math.max(0, speed).toFixed(0);
      const lapIdx = player.stats.currentLapDisplay();
      const totalLaps = player.stats.totalLaps;
      if (this.hud.lap) this.hud.lap.textContent = `${lapIdx}/${totalLaps}`;
      const lapTime = player.stats.runningLapTime(this.circuitStartTime);
      if (this.hud.lapTime) this.hud.lapTime.textContent = lapTime.toFixed(3);
      const bestLap = player.stats.bestLap();
      if (this.hud.best) this.hud.best.textContent = bestLap ? bestLap.toFixed(3) : "-";
      if (this.state === "running") this.setStatus("Racing");

      if (this.mode === MODE.RACE && this.leaderboard) {
        const rows = this.racers.map((c, idx) => ({
          idx,
          progress: c.stats.progressFraction(),
          color: c.mesh.children[0]?.material?.color?.getHex(),
        }));
        rows.sort((a, b) => b.progress - a.progress);
        this.leaderboard.innerHTML = "";
        rows.forEach((row, i) => {
          const li = document.createElement("li");
          li.className = "d-flex align-items-center mb-1";
          const colorHex = row.color ?? PALETTE.cars.red;
          const swatch = document.createElement("span");
          swatch.className = "d-inline-block me-2 rounded";
          swatch.style.width = "12px";
          swatch.style.height = "12px";
          swatch.style.background = `#${colorHex.toString(16).padStart(6, "0")}`;
          const label = document.createElement("span");
          label.className = "flex-grow-1";
          label.textContent = `${i + 1}. ${row.idx === 0 ? "You" : "AI " + row.idx}`;
          const time = document.createElement("span");
          time.className = "text-muted";
          time.textContent = `${this.racers[row.idx].stats.currentLapDisplay()}/${this.racers[row.idx].stats.totalLaps}`;
          li.append(swatch, label, time);
          this.leaderboard.appendChild(li);
        });
      }
    }

    submitBestResult() {
      if (this.resultSubmitted) return;
      this.resultSubmitted = true;

      const player = this.racers[0];
      const lapValue = Number(player?.stats?.bestLap?.() ?? NaN);
      const hasLap = Number.isFinite(lapValue);

      const payload = {
        bestLapTime: hasLap ? lapValue : null,
        totalTime: this.circuitStartTime,
        trackKey: this.circuitKey,
        trackName: TRACKS[this.circuitKey]?.meta?.displayName || this.circuitKey,
      };

      if (!hasLap || !this.isAuthenticated) {
        this.renderBestResult(this.bestResult);
        return;
      }

      fetch("/F1Game/SaveBestResult", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
      })
        .then((res) => (res.ok ? res.json() : null))
        .then((data) => {
          if (!data) return;
          if (data.bestResult) this.bestResult = data.bestResult;
          this.renderBestResult(this.bestResult);
        })
        .catch(() => {
          this.renderBestResult(this.bestResult);
        });
    }

    renderBestResult(best) {
      const els = this.bestResultEls || {};
      const stats = els.stats || els.container?.querySelector?.("#best-result-stats");
      const message = els.message || els.container?.querySelector?.("#best-result-message");
      const lap = els.lap || els.container?.querySelector?.("#best-result-lap");
      const track = els.track || els.container?.querySelector?.("#best-result-track");
      const updated = els.updated || els.container?.querySelector?.("#best-result-updated");
      const lapValue = Number(best?.bestLapTime ?? NaN);
      const hasResult = Number.isFinite(lapValue);

      if (stats) stats.classList.toggle("d-none", !hasResult);

      if (message) {
        if (!this.isAuthenticated) {
          message.textContent = "Log in to save your best result.";
          message.classList.remove("d-none");
        } else if (!hasResult) {
          message.textContent = "Complete a race to set your best lap.";
          message.classList.remove("d-none");
        } else {
          message.classList.add("d-none");
        }
      }

      if (lap) lap.textContent = hasResult ? `${lapValue.toFixed(3)} s` : "-";
      if (track) track.textContent = hasResult ? best.trackName || best.trackKey || "Unknown" : "-";
      if (updated) {
        if (hasResult && best?.updatedAt) {
          const dt = new Date(best.updatedAt);
          updated.textContent = Number.isNaN(dt.getTime()) ? "-" : dt.toLocaleString();
        } else {
          updated.textContent = "-";
        }
      }
    }

    renderCountdown(text, hide = false) {
      if (!this.countdownEl) return;
      if (hide || !text) {
        this.countdownEl.classList.add("d-none");
        this.countdownEl.classList.remove("is-go");
        return;
      }
      this.countdownEl.textContent = text;
      this.countdownEl.classList.remove("d-none");
      if (text.toLowerCase() === "go") this.countdownEl.classList.add("is-go");
      else this.countdownEl.classList.remove("is-go");
    }

    setStatus(text) {
      if (this.hud.status) this.hud.status.textContent = text;
    }
  }

  window.F1Game3D = {
    init: (config) => new Game(config),
  };
})();
