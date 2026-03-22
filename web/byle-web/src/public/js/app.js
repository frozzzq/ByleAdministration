/* ══════════════════════════════════════
   BYLE — Portal Web v2
   JavaScript principal
   ══════════════════════════════════════ */

// ── TEMA (claro/oscuro) ──
function toggleTheme() {
  const html = document.documentElement;
  const current = html.getAttribute('data-theme');
  const next = current === 'dark' ? 'light' : 'dark';

  html.setAttribute('data-theme', next === 'light' ? '' : 'dark');
  localStorage.setItem('byle-theme', next);

  // Actualizar ícono y meta color
  const btn = document.querySelector('.theme-toggle');
  if (btn) btn.textContent = next === 'dark' ? '🌙' : '☀️';

  const meta = document.querySelector('meta[name="theme-color"]');
  if (meta) meta.content = next === 'dark' ? '#0D0D1B' : '#F5F3EF';
}

// Cargar tema guardado
(function initTheme() {
  const saved = localStorage.getItem('byle-theme');
  if (saved === 'dark') {
    document.documentElement.setAttribute('data-theme', 'dark');
    const btn = document.querySelector('.theme-toggle');
    if (btn) btn.textContent = '🌙';
    const meta = document.querySelector('meta[name="theme-color"]');
    if (meta) meta.content = '#0D0D1B';
  }
})();

// ── MENÚ MÓVIL ──
function toggleMenu() {
  const menu = document.getElementById('mobileMenu');
  if (menu) menu.classList.toggle('open');
}

// Cerrar menú al hacer click en un link
document.querySelectorAll('.mobile-menu a').forEach(function(link) {
  link.addEventListener('click', function() {
    var menu = document.getElementById('mobileMenu');
    if (menu) menu.classList.remove('open');
  });
});

// ── TABS DE CATEGORÍAS (competencias) ──
function showCategory(btn, id) {
  // Ocultar todas las categorías
  var cats = document.querySelectorAll('[id^="cat-"]');
  cats.forEach(function(c) { c.classList.add('hidden'); });

  // Mostrar la seleccionada
  var target = document.getElementById(id);
  if (target) target.classList.remove('hidden');

  // Actualizar tabs activas
  var tabs = btn.parentElement.querySelectorAll('.ranking-tab');
  tabs.forEach(function(t) { t.classList.remove('active'); });
  btn.classList.add('active');
}

// ── FILTRO DE CATEGORÍAS (tienda) ──
document.querySelectorAll('.ranking-tabs .ranking-tab').forEach(function(tab) {
  // Solo para tabs que NO son links (botones de filtro)
  if (tab.tagName === 'BUTTON' && !tab.hasAttribute('onclick')) {
    tab.addEventListener('click', function() {
      var siblings = this.parentElement.querySelectorAll('.ranking-tab');
      siblings.forEach(function(s) { s.classList.remove('active'); });
      this.classList.add('active');
    });
  }
});

// ── COUNTDOWN QR ──
(function initQRTimer() {
  var timerEl = document.getElementById('qrTimer');
  if (!timerEl) return;

  var seconds = 298;

  setInterval(function() {
    seconds--;
    if (seconds <= 0) seconds = 300;
    var min = Math.floor(seconds / 60);
    var sec = seconds % 60;
    timerEl.innerHTML = 'Se actualiza en: <strong>' + min + ':' + String(sec).padStart(2, '0') + '</strong>';
  }, 1000);
})();

// ── MARCAR LINK ACTIVO EN NAVBAR (desktop) ──
(function highlightActiveNav() {
  var path = window.location.pathname;
  var links = document.querySelectorAll('.navbar-links a');
  links.forEach(function(link) {
    var href = link.getAttribute('href');
    // Coincidencia exacta o que empiece con el href
    if (href === path || (href !== '/' && path.indexOf(href) === 0)) {
      link.classList.add('active');
    }
  });

  // También para el menú móvil
  var mobileLinks = document.querySelectorAll('.mobile-menu a');
  mobileLinks.forEach(function(link) {
    var href = link.getAttribute('href');
    if (href === path || (href !== '/' && path.indexOf(href) === 0)) {
      link.classList.add('active');
    }
  });
})();
