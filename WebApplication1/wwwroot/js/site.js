(() => {
  const host = document.querySelector('.particles');
  if (!host) return;

  const prefersReducedMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  if (prefersReducedMotion) return;

  const canvas = document.createElement('canvas');
  canvas.className = 'particles-canvas';
  host.appendChild(canvas);
  const ctx = canvas.getContext('2d');
  if (!ctx) return;

  let w = 0;
  let h = 0;
  let dpr = Math.min(window.devicePixelRatio || 1, 2);

  const maxParticles = 60;
  const particles = [];

  function resize() {
    const rect = host.getBoundingClientRect();
    w = Math.max(1, Math.floor(rect.width));
    h = Math.max(1, Math.floor(rect.height));
    dpr = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = Math.floor(w * dpr);
    canvas.height = Math.floor(h * dpr);
    canvas.style.width = `${w}px`;
    canvas.style.height = `${h}px`;
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
  }

  function rand(min, max) {
    return Math.random() * (max - min) + min;
  }

  function fillParticles() {
    particles.length = 0;
    const count = Math.min(maxParticles, Math.floor((w * h) / 22000));
    for (let i = 0; i < count; i++) {
      particles.push({
        x: rand(0, w),
        y: rand(0, h),
        r: rand(1.2, 3.6),
        vx: rand(-0.18, 0.18),
        vy: rand(0.05, 0.28),
        a: rand(0.10, 0.22),
        hue: Math.random() > 0.65 ? '107, 78, 46' : '31, 59, 44'
      });
    }
  }

  let raf = 0;
  function tick() {
    ctx.clearRect(0, 0, w, h);

    for (const p of particles) {
      p.x += p.vx;
      p.y += p.vy;
      if (p.y - p.r > h) p.y = -p.r;
      if (p.x - p.r > w) p.x = -p.r;
      if (p.x + p.r < 0) p.x = w + p.r;

      ctx.beginPath();
      ctx.fillStyle = `rgba(${p.hue}, ${p.a})`;
      ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
      ctx.fill();
    }

    raf = window.requestAnimationFrame(tick);
  }

  const ro = new ResizeObserver(() => {
    resize();
    fillParticles();
  });
  ro.observe(host);

  window.addEventListener('visibilitychange', () => {
    if (document.hidden) {
      window.cancelAnimationFrame(raf);
      raf = 0;
    } else if (!raf) {
      raf = window.requestAnimationFrame(tick);
    }
  });

  resize();
  fillParticles();
  raf = window.requestAnimationFrame(tick);
})();

(() => {
  const el = document.getElementById('page-transition');
  if (!el) return;

  const show = () => {
    el.classList.add('is-active');
  };

  const hide = () => {
    el.classList.remove('is-active');
  };

  window.addEventListener('pageshow', () => hide());
  window.addEventListener('pagehide', () => show());

  document.addEventListener('click', (e) => {
    const a = e.target instanceof Element ? e.target.closest('a') : null;
    if (!a) return;

    if (a.closest('[data-no-transition="true"]')) return;

    if (a.hasAttribute('download')) return;
    const href = a.getAttribute('href');
    if (!href || href.startsWith('#') || href.startsWith('javascript:')) return;

    const target = a.getAttribute('target');
    if (target && target !== '_self') return;
    if (e.defaultPrevented) return;
    if (e.button !== 0) return;
    if (e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

    const url = new URL(href, window.location.href);
    if (url.origin !== window.location.origin) return;
    if (url.href === window.location.href) return;

    show();
  }, { capture: true });

  document.addEventListener('submit', (e) => {
    const form = e.target;
    if (!(form instanceof HTMLFormElement)) return;
    if (e.defaultPrevented) return;
    if (form.matches('[data-no-transition="true"]')) return;
    show();
  }, { capture: true });
})();
