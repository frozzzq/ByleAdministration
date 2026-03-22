/* ══════════════════════════════════════
   BYLE — Portal de Clientes
   Interacciones básicas
   ══════════════════════════════════════ */

// ── Countdown del QR (si estamos en la página de acceso) ──
(function initQRTimer() {
  const timerEl = document.getElementById('qrTimer');
  if (!timerEl) return;

  let seconds = 298; // 4:58

  setInterval(() => {
    seconds--;
    if (seconds <= 0) {
      seconds = 300; // reiniciar a 5:00
    }
    const min = Math.floor(seconds / 60);
    const sec = seconds % 60;
    timerEl.innerHTML = `Se actualiza en: <strong>${min}:${sec.toString().padStart(2, '0')}</strong>`;
  }, 1000);
})();

// ── Botones de filtro (selector de días, etc.) ──
document.querySelectorAll('.flex.gap-sm .btn').forEach(btn => {
  btn.addEventListener('click', function () {
    // Quitar active de todos los hermanos
    this.parentElement.querySelectorAll('.btn').forEach(b => {
      b.classList.remove('btn-primary');
      b.classList.add('btn-ghost');
    });
    // Activar el clickeado
    this.classList.remove('btn-ghost');
    this.classList.add('btn-primary');
  });
});
