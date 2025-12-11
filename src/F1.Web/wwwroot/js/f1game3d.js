(() => {
  const { THREE } = window;
  const TRACKS = window.F1TrackData?.circuits || {};
  if (!THREE || !Object.keys(TRACKS).length) return;

  // Tuned driving constants for a responsive but controllable feel
  const PHYS = {
    engine_power: 28.0,
    braking_power: 46.0,
    max_speed_reverse: 12.0,
    friction_coefficient: 0.6,
    drag_coefficient: 0.32,
    lateral_grip: 6.5,
    max_steering_angle: 18.0,
    wheel_base: 2.8,
    velocity_eps: 0.05,
    steering_response: 7.0,
    ai_throttle: 0.82,
  };

  const KMH = 3.6;
  const MODE = { TIME: "time", RACE: "race" };
  const TRACK_WIDTH = 4.2;
  const KERB_EXTRA = 0.55;
  const GUARD_EXTRA = 1.6;

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
  const vec3FromArr = (arr) => new THREE.Vector3(arr[0], arr[1], arr[2]);

  class Stats {
    constructor(roadsCount, lapsCount, penalty) {
      this._roads_count = roadsCount;
      this._laps_count = lapsCount;
      this._penalty = penalty;
      this._road_idx = 0;
      this._next_road_idx = 1;
      this._lap_idx = 0;
      this._laps = new Array(1 + lapsCount).fill(0);
      this._penalties = new Array(1 + lapsCount).fill(0);
      this._road_avg = 0;
      this._road_avg_max = 0;
      this._road_avg_n = 0;
    }

    set_time_elapsed(roadIdx, timeElapsed) {
      if (this._lap_idx > this._laps_count || roadIdx === this._road_idx) return;

      if (roadIdx < this._next_road_idx) {
        if (roadIdx === 0) {
          for (let i = this._next_road_idx; i < this._roads_count; i += 1) {
            this._penalties[this._lap_idx] += this._penalty;
          }
        }
      } else if (roadIdx > this._next_road_idx) {
        for (let i = this._next_road_idx; i < roadIdx; i += 1) {
          this._penalties[this._lap_idx] += this._penalty;
        }
      }

      this._road_idx = roadIdx;
      this._next_road_idx = (roadIdx + 1) % this._roads_count;
      this._laps[this._lap_idx] = timeElapsed;
      this._road_avg_n += 1;
      this._road_avg = timeElapsed / this._road_avg_n;
      if (this._road_avg_max < this._road_avg) this._road_avg_max = this._road_avg;
      if (this._road_idx === 0) this._lap_idx += 1;
    }

    current_road_idx() {
      return this._road_idx;
    }

    lap_idx() {
      return this._lap_idx;
    }

    finished() {
      return this._lap_idx >= this._laps_count;
    }

    lap(idx) {
      const t = idx === 0 ? this._laps[idx] : this._laps[idx] - this._laps[idx - 1];
      return [t, this._penalties[idx], t + this._penalties[idx]];
    }

    total() {
      const t = [0, 0, 0];
      const n = this._lap_idx < this._laps_count ? this._lap_idx : this._laps_count;
      for (let i = 0; i < n; i += 1) {
        const ti = this.lap(i);
        t[0] += ti[0];
        t[1] += ti[1];
        t[2] += ti[2];
      }
      return t;
    }

    approx_total() {
      const t = [0, 0, 0];
      for (let i = 0; i < this._laps_count; i += 1) {
        if (this._laps[i] > 0.0) t[0] = this._laps[i];
        t[1] += this._penalties[i];
      }
      if (this._lap_idx < this._laps_count) {
        let n = this._roads_count - this._road_idx;
        t[0] += n * this._road_avg_max;
        n = this._laps_count - this._lap_idx - 1;
        if (n > 0) t[0] += n * this._roads_count * this._road_avg_max;
      }
      t[2] = t[0] + t[1];
      return t;
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
      this.steeringAngle = 0;
      this.maxSteeringRad = degToRad(PHYS.max_steering_angle);
      this.stats = null;
      this.lastRoadIdx = null;
      this.distance = 0;
      this._steerCommand = 0;
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
      const q = new THREE.Quaternion().setFromUnitVectors(new THREE.Vector3(0, 0, -1), forwardDir.clone().normalize());
      this.mesh.position.copy(position);
      this.mesh.position.y = Math.max(this.mesh.position.y, 0.22);
      this.mesh.quaternion.copy(q);
      this.velocity.set(0, 0, 0);
      this.acceleration.set(0, 0, 0);
      this.steeringAngle = 0;
      this.lastRoadIdx = null;
      this.distance = 0;
      this._steerCommand = 0;
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

    update(dt, input, desiredDir = null) {
      const forward = this.forward();
      const up = new THREE.Vector3(0, 1, 0);

      let steerInput = 0;
      let throttle = 0;
      let brake = 0;

      if (input) {
        steerInput = (input.left ? -1 : 0) + (input.right ? 1 : 0);
        throttle = input.up ? 1 : 0;
        brake = input.down ? 1 : 0;
      } else {
        if (desiredDir) {
          steerInput = clamp(desiredDir.clone().cross(forward).y * 2.0, -1, 1);
        }
        throttle = PHYS.ai_throttle;
      }

      const steerTarget = clamp(steerInput, -1, 1) * this.maxSteeringRad;
      const steerSmooth = 1 - Math.exp(-PHYS.steering_response * dt);
      this.steeringAngle = THREE.MathUtils.lerp(this.steeringAngle, steerTarget, steerSmooth);

      const forwardSpeed = this.velocity.dot(forward);
      let accelForward = throttle * PHYS.engine_power;
      if (brake > 0) {
        if (forwardSpeed > 1.2) {
          accelForward -= brake * PHYS.braking_power;
        } else {
          accelForward -= brake * (PHYS.engine_power * 0.65);
        }
      }

      const drag = this.velocity.clone().multiplyScalar(PHYS.drag_coefficient);
      const rolling = this.velocity.clone().multiplyScalar(PHYS.friction_coefficient);
      this.acceleration.copy(forward.clone().multiplyScalar(accelForward)).sub(drag).sub(rolling);

      this.applyGrip(forward, dt);
      this.velocity.add(this.acceleration.clone().multiplyScalar(dt));

      const currentForward = this.forward();
      const newForwardSpeed = this.velocity.dot(currentForward);
      if (newForwardSpeed < -PHYS.max_speed_reverse) {
        this.velocity.add(currentForward.multiplyScalar(-PHYS.max_speed_reverse - newForwardSpeed));
      }

      let speed = this.velocity.length();
      if (speed > 120) {
        this.velocity.setLength(120);
        speed = 120;
      }
      if (speed < PHYS.velocity_eps) {
        this.velocity.set(0, 0, 0);
      } else {
        const turnRate = Math.tan(this.steeringAngle) * speed / Math.max(0.1, PHYS.wheel_base);
        const yaw = turnRate * dt;
        if (Number.isFinite(yaw)) {
          const rot = new THREE.Quaternion().setFromAxisAngle(up, yaw);
          this.mesh.quaternion.multiply(rot);
          this.velocity.applyQuaternion(rot);
        }
      }

      this.mesh.position.add(this.velocity.clone().multiplyScalar(dt));
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
      this.samples = this.sampleCurve(Math.max(800, this.roadsCount * 40));
      this.length = this.samples.totalLength;
      this.roadLength = this.length / this.roadsCount;
      this.group = new THREE.Group();
      this.startPoint = this.curve.getPointAt(0);
      this.startTangent = this.curve.getTangentAt(0).normalize();
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
      const t = distance / this.length;
      return this.curve.getTangentAt(t % 1).normalize();
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
        this.scene.fog = new THREE.Fog(this.scene.background, 180, 420);
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

      const pts = [];
      const tangents = [];
      const normals2 = [];
      const samples = Math.max(600, this.roadsCount * 30);
      for (let i = 0; i <= samples; i += 1) {
        const t = i / samples;
        const p = this.curve.getPointAt(t);
        const tan = this.curve.getTangentAt(t);
        const dir2 = new THREE.Vector2(tan.x, tan.z).normalize();
        const normal2 = new THREE.Vector2(-dir2.y, dir2.x);
        pts.push(p);
        tangents.push(tan);
        normals2.push(normal2);
      }

      const width = TRACK_WIDTH * 0.5;
      const left = [];
      const right = [];
      const kerbLeft = [];
      const kerbRight = [];
      for (let i = 0; i < pts.length; i += 1) {
        const p = pts[i];
        const n2 = normals2[i];
        left.push(new THREE.Vector3(p.x + n2.x * width, p.y, p.z + n2.y * width));
        right.push(new THREE.Vector3(p.x - n2.x * width, p.y, p.z - n2.y * width));
        kerbLeft.push(new THREE.Vector3(p.x + n2.x * (width + KERB_EXTRA), p.y + 0.01, p.z + n2.y * (width + KERB_EXTRA)));
        kerbRight.push(new THREE.Vector3(p.x - n2.x * (width + KERB_EXTRA), p.y + 0.01, p.z - n2.y * (width + KERB_EXTRA)));
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
      const guard = (offset) => {
        const curve = new THREE.CatmullRomCurve3(
          pts.map((p, i) => {
            const n2 = normals2[i];
            return new THREE.Vector3(p.x + n2.x * offset, p.y + 0.3, p.z + n2.y * offset);
          }),
          true
        );
        const geo = new THREE.TubeGeometry(curve, pts.length, 0.12, 8, true);
        const mesh = new THREE.Mesh(geo, railMat);
        mesh.castShadow = true;
        return mesh;
      };
      group.add(guard(width + GUARD_EXTRA), guard(-width - GUARD_EXTRA));

      const margin = 60;
      const bounds = this.meta.bounds;
      const minX = bounds?.min?.[0] ?? -60;
      const maxX = bounds?.max?.[0] ?? 60;
      const minZ = bounds?.min?.[2] ?? -60;
      const maxZ = bounds?.max?.[2] ?? 60;
      const groundW = Math.max(Math.abs(minX), Math.abs(maxX)) * 2 + margin;
      const groundH = Math.max(Math.abs(minZ), Math.abs(maxZ)) * 2 + margin;

      const ground = new THREE.Mesh(
        new THREE.PlaneGeometry(groundW, groundH),
        new THREE.MeshStandardMaterial({ color: PALETTE.grass, roughness: 0.95 })
      );
      ground.rotation.x = -Math.PI / 2;
      ground.position.y = -0.02;
      ground.receiveShadow = true;
      group.add(ground);

      const sand = new THREE.Mesh(
        new THREE.PlaneGeometry(groundW * 1.1, groundH * 1.1),
        new THREE.MeshStandardMaterial({ color: PALETTE.sand, roughness: 1 })
      );
      sand.rotation.x = -Math.PI / 2;
      sand.position.y = -0.04;
      sand.receiveShadow = true;
      group.add(sand);

      const startPerp = new THREE.Vector2(-this.startTangent.z, this.startTangent.x).normalize();
      const stripe = new THREE.Mesh(
        new THREE.PlaneGeometry(TRACK_WIDTH, 0.4),
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
      const mast = new THREE.Mesh(new THREE.BoxGeometry(0.2, 2.5, 0.2), new THREE.MeshStandardMaterial({ color: 0x333333 }));
      mast.position.set(-TRACK_WIDTH * 0.8, 1.25, 0);
      lightsRig.add(mast);
      const bar = new THREE.Mesh(new THREE.BoxGeometry(3.0, 0.2, 0.2), new THREE.MeshStandardMaterial({ color: 0x444444 }));
      bar.position.set(0, 2.25, 0);
      lightsRig.add(bar);
      for (let i = 0; i < 5; i += 1) {
        const light = new THREE.Mesh(new THREE.SphereGeometry(0.12, 12, 8), new THREE.MeshStandardMaterial({ color: 0x550000, emissive: 0x220000 }));
        light.position.set(bar.position.x - 1.3 + i * 0.65, bar.position.y, bar.position.z);
        lightsRig.add(light);
      }
      lightsRig.rotation.y = stripe.rotation.y;
      group.add(lightsRig);

      const treeMatTrunk = new THREE.MeshStandardMaterial({ color: PALETTE.treeTrunk, roughness: 0.9 });
      const treeMatLeaf = new THREE.MeshStandardMaterial({ color: PALETTE.treeLeaves, roughness: 0.6 });
      const tree = () => {
        const t = new THREE.Group();
        const trunk = new THREE.Mesh(new THREE.CylinderGeometry(0.12, 0.15, 1.1, 6), treeMatTrunk);
        trunk.position.y = 0.55;
        const leaves = new THREE.Mesh(new THREE.ConeGeometry(0.7, 1.2, 8), treeMatLeaf);
        leaves.position.y = 1.7;
        t.add(trunk, leaves);
        t.castShadow = true;
        t.receiveShadow = true;
        return t;
      };
      for (let i = 0; i < pts.length; i += Math.floor(samples / 18)) {
        const n2 = normals2[i];
        const p = pts[i];
        const dist = TRACK_WIDTH * 2.5 + (i % 2 === 0 ? 6 : 10);
        const posA = new THREE.Vector3(p.x + n2.x * dist, 0, p.z + n2.y * dist);
        const posB = new THREE.Vector3(p.x - n2.x * dist, 0, p.z - n2.y * dist);
        const ta = tree();
        ta.position.copy(posA);
        const tb = tree();
        tb.position.copy(posB);
        group.add(ta, tb);
      }

      const pylonGeo = new THREE.ConeGeometry(0.18, 0.5, 6);
      const pylonMat = new THREE.MeshStandardMaterial({ color: PALETTE.kerbRed, roughness: 0.5, metalness: 0.1 });
      for (let i = Math.floor(samples / 16); i < pts.length; i += Math.floor(samples / 12)) {
        const n2 = normals2[i];
        const p = pts[i];
        const offset = TRACK_WIDTH * 1.8 + 2;
        const cone = new THREE.Mesh(pylonGeo, pylonMat);
        cone.position.set(p.x + n2.x * offset, 0.25, p.z + n2.y * offset);
        cone.castShadow = true;
        group.add(cone);
      }

      const wallMat = new THREE.MeshStandardMaterial({ color: 0xb8bcc4, roughness: 0.6, metalness: 0.08 });
      for (let i = 0; i < pts.length; i += Math.floor(samples / 14)) {
        const n2 = normals2[i];
        const p = pts[i];
        const wall = new THREE.Mesh(new THREE.BoxGeometry(0.35, 0.8, 2.6), wallMat);
        wall.position.set(p.x + n2.x * (width + GUARD_EXTRA + 0.6), 0.4, p.z + n2.y * (width + GUARD_EXTRA + 0.6));
        wall.castShadow = true;
        wall.receiveShadow = true;
        group.add(wall);
      }
    }
  }

  class Game {
    constructor(config) {
      this.canvas = config.canvas;
      this.modeInputs = config.modeInputs;
      this.circuitSelect = config.circuitSelect;
      this.carButtons = config.carButtons;
      this.startButton = config.startButton;
      this.restartButton = config.restartButton;
      this.hud = config.hud || {};
      this.leaderboard = config.leaderboard;

      this.mode = MODE.TIME;
      this.selectedCar = "red";
      this.circuitKey = Object.keys(TRACKS)[0];

      this.scene = new THREE.Scene();
      this.camera = new THREE.PerspectiveCamera(60, 16 / 9, 0.1, 600);
      this.camera.up.set(0, 1, 0);
      this.renderer = new THREE.WebGLRenderer({ canvas: this.canvas, antialias: true });
      this.renderer.shadowMap.enabled = true;
      this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
      this.clock = new THREE.Clock();

      this.input = { up: false, down: false, left: false, right: false };
      this.state = "idle";
      this.countdown = 0;
      this.racers = [];
      this.circuit = null;

      this.setupLights();
      this.bindUI();
      this.populateCircuits();
      this.loadCircuit(this.circuitKey);
      this.resetSession();
      this.resize();
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
        r.stats = new Stats(this.circuit.roadsCount, this.circuit.meta.lapsCount || 2, this.circuit.meta.penalty || 5);
        r.reset(startPos(idx), startForward);
      });
      this.state = "idle";
      this.countdown = 0;
      this.circuitStartTime = 0;
      this.setStatus("Ready");
      this.updateHUD();
    }

    start() {
      this.resetSession();
      this.countdown = 3.0;
      this.state = "countdown";
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
        if (this.countdown <= 0) {
          this.state = "running";
          this.circuitStartTime = 0;
          this.setStatus("Go!");
        }
      }

      if (this.state === "running") {
        this.circuitStartTime += dt;
        this.racers.forEach((r, idx) => {
          if (r.isPlayer) {
            r.update(dt, this.input, null);
          } else {
            const roadIdx = r.stats.current_road_idx();
            const dir = this.circuit.pathDirectionForRoad(roadIdx, r.mesh.position);
            r._steerCommand = dir;
            r.update(dt, null, dir);
          }

          const nearest = this.circuit.nearestSample(r.mesh.position);
          let distance = nearest.distance;
          if (Number.isFinite(r.distance)) {
            const diff = distance - (r.distance % this.circuit.length);
            if (diff < -this.circuit.length * 0.5) distance += this.circuit.length;
            else if (diff > this.circuit.length * 0.5) distance -= this.circuit.length;
          }
          r.distance = distance;
          const roadIdx = this.circuit.roadIndexAtDistance(distance);
          if (r.lastRoadIdx === null || roadIdx !== r.lastRoadIdx) {
            r.stats.set_time_elapsed(roadIdx, this.circuitStartTime);
            r.lastRoadIdx = roadIdx;
          }
        });

        if (this.racers[0].stats.finished()) {
          this.state = "finished";
          this.setStatus("Finished");
        }
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
      const lapIdx = Math.min(player.stats.lap_idx() + 1, this.circuit.meta.lapsCount || 2);
      if (this.hud.lap) this.hud.lap.textContent = `${lapIdx}/${this.circuit.meta.lapsCount || 2}`;
      const lapTime = player.stats._laps[player.stats.lap_idx()] || 0;
      if (this.hud.lapTime) this.hud.lapTime.textContent = lapTime.toFixed(3);
      let bestLap = null;
      for (let i = 0; i < player.stats.lap_idx(); i += 1) {
        const t = player.stats.lap(i)[2];
        bestLap = bestLap === null ? t : Math.min(bestLap, t);
      }
      if (this.hud.best) this.hud.best.textContent = bestLap ? bestLap.toFixed(3) : "-";
      if (this.state === "running") this.setStatus("Racing");

      if (this.mode === MODE.RACE && this.leaderboard) {
        const rows = this.racers.map((c, idx) => ({
          idx,
          progress: c.stats.lap_idx() + c.stats.current_road_idx() / (this.circuit.roadsCount || 1),
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
          time.textContent = `${Math.min(this.circuit.meta.lapsCount || 2, this.racers[row.idx].stats.lap_idx() + 1)}/${this.circuit.meta.lapsCount || 2}`;
          li.append(swatch, label, time);
          this.leaderboard.appendChild(li);
        });
      }
    }

    setStatus(text) {
      if (this.hud.status) this.hud.status.textContent = text;
    }
  }

  window.F1Game3D = {
    init: (config) => new Game(config),
  };
})();
