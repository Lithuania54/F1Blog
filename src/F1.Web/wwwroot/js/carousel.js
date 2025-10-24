// Minimal accessible carousel for F1.Web
(() => {
  function Carousel(root){
    this.root = root;
    this.slidesEl = root.querySelector('.carousel-slides');
    this.nextBtn = root.querySelector('.carousel-next');
    this.prevBtn = root.querySelector('.carousel-prev');
    this.dotsEl = root.querySelector('.carousel-dots');
    this.interval = Number(root.dataset.interval) || 4000;
    this.images = window.F1CarouselImages || [];
    this.index = 0;
    this.timer = null;
    this.paused = false;
    this.init();
  }
  Carousel.prototype.init = function(){
    if(!this.images.length) return;
    this.slidesEl.innerHTML = '';
    this.dotsEl.innerHTML = '';
    this.images.forEach((src,i)=>{
      const img = document.createElement('img');
      img.src = src; img.loading='lazy'; img.alt = `F1 car #${i+1}`; img.className='w-full h-60 object-cover';
      const slide = document.createElement('div'); slide.className='carousel-slide'; if(i!==0) slide.style.display='none'; slide.appendChild(img);
      this.slidesEl.appendChild(slide);
      const dot = document.createElement('button'); dot.className='w-3 h-3 rounded-full bg-white/40'; dot.setAttribute('aria-label',`Go to slide ${i+1}`);
      dot.addEventListener('click', ()=>this.go(i));
      this.dotsEl.appendChild(dot);
    });
    this.update();
    this.nextBtn.addEventListener('click', ()=>this.next());
    this.prevBtn.addEventListener('click', ()=>this.prev());
    this.root.addEventListener('mouseenter', ()=>this.pause());
    this.root.addEventListener('focusin', ()=>this.pause());
    this.root.addEventListener('mouseleave', ()=>this.play());
    this.root.addEventListener('focusout', ()=>this.play());
    this.root.addEventListener('keydown', (e)=>{ if(e.key==='ArrowRight') this.next(); if(e.key==='ArrowLeft') this.prev(); });
    this.play();
  }
  Carousel.prototype.play = function(){ if(this.paused) return; this.timer = setInterval(()=>this.next(), this.interval); }
  Carousel.prototype.pause = function(){ clearInterval(this.timer); this.timer=null; }
  Carousel.prototype.next = function(){ this.go((this.index+1)%this.images.length); }
  Carousel.prototype.prev = function(){ this.go((this.index-1+this.images.length)%this.images.length); }
  Carousel.prototype.go = function(i){
    const slides = this.slidesEl.children; if(slides[this.index]) slides[this.index].style.display='none';
    this.index = i; if(slides[this.index]) slides[this.index].style.display='block';
    Array.from(this.dotsEl.children).forEach((d,idx)=> d.style.opacity = idx===i ? '1' : '0.5');
  }
  Carousel.prototype.update = function(){ this.go(this.index); }

  document.addEventListener('DOMContentLoaded', ()=>{
    try{
      const c = document.getElementById('f1Carousel');
      if(!c) return;
      // images should be provided by server: window.F1CarouselImages = [...]
      if(!window.F1CarouselImages){
        // try to build from data attribute list (provided inline)
        const imgs = [];
        // server should inject a global; if not, attempt to read from data-images attr
        const data = c.dataset.images;
        if(data){ try{ JSON.parse(data).forEach(u=>imgs.push(u)); }catch(e){} }
        window.F1CarouselImages = imgs;
      }
      new Carousel(c);
    }catch(e){ console.error(e); }
  });
})();
