/* carousel-init.js
   Initializes Swiper carousels for the homepage (cars) and tracks.
   Uses server-provided data arrays rendered into the DOM.
   No eval/new Function is used. Scripts loaded with defer.
*/
document.addEventListener('DOMContentLoaded', function(){
  try{
    // Homepage carousel (id: f1Swiper, data-images JSON in data-images attr)
    const el = document.getElementById('f1Swiper');
    if(el){
      const data = el.dataset.images;
      let imgs = [];
      try{ imgs = data ? JSON.parse(data) : []; } catch(e){ console.warn('Failed to parse f1 images', e); }
      // Initialize even if slides are pre-rendered without data-images
      const swiper = new Swiper('#f1Swiper', {
        loop: imgs.length > 1,
        autoplay: { delay: 4000, disableOnInteraction: false },
        keyboard: { enabled: true },
        pagination: { el: '.swiper-pagination', clickable: true },
        navigation: { nextEl: '.swiper-button-next', prevEl: '.swiper-button-prev' },
        effect: 'slide'
      });
      // pause on hover/focus
      el.addEventListener('mouseenter', ()=>swiper.autoplay.stop());
      el.addEventListener('mouseleave', ()=>swiper.autoplay.start());
      el.addEventListener('focusin', ()=>swiper.autoplay.stop());
      el.addEventListener('focusout', ()=>swiper.autoplay.start());
    }

    // Tracks carousel (id: tracksSwiper, data-tracks attr)
    const t = document.getElementById('tracksSwiper');
    if(t){
      const data = t.dataset.tracks;
      let items = [];
      try{ items = data ? JSON.parse(data) : []; } catch(e){ console.warn('Failed to parse track images', e); }
      if(items.length){
        const swiper = new Swiper('#tracksSwiper', {
          loop: true,
          slidesPerView: 1,
          autoplay: { delay: 4000, disableOnInteraction: false },
          keyboard: { enabled: true },
          pagination: { el: '.swiper-pagination', clickable: true },
          navigation: { nextEl: '.swiper-button-next', prevEl: '.swiper-button-prev' },
        });
        t.addEventListener('mouseenter', ()=>swiper.autoplay.stop());
        t.addEventListener('mouseleave', ()=>swiper.autoplay.start());
      }
    }
  }catch(e){ console.error(e); }
});
