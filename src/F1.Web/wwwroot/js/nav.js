document.addEventListener('DOMContentLoaded', function(){
  const btn = document.getElementById('mobileMenuButton');
  const menu = document.getElementById('mobileMenu');
  if(btn && menu){
    btn.addEventListener('click', ()=>{
      const expanded = btn.getAttribute('aria-expanded') === 'true';
      btn.setAttribute('aria-expanded', (!expanded).toString());
      menu.classList.toggle('hidden');
    });
  }

  // Dark toggle
  const dt = document.getElementById('darkToggle');
  if(dt){
    dt.addEventListener('click', ()=>{
      const key='f1:dark';
      const isDark = document.documentElement.classList.toggle('dark');
      localStorage.setItem(key, isDark.toString());
    });
  }
});
