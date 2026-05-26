using System;
using System.IO;
using System.Windows;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;

namespace ByleAdministration.Utilidades
{
    public enum ModoLector { Enrolamiento, Verificacion }

    /// <summary>
    /// Sesión de captura biométrica.
    /// Crea una instancia por dialog. Al iniciar, pausa GestorSensor para
    /// evitar que dos Capture compitan por el mismo sensor físico.
    /// </summary>
    public class LectorHuella : DPFP.Capture.EventHandler, IDisposable
    {
        private Capture _capture;
        private Enrollment _enrolamiento;
        private readonly ModoLector _modo;
        private bool _disposed;

        // ── Eventos públicos ────────────────────────────────────────
        public event Action<int> ProgresoEnrolamiento; // muestras restantes
        public event Action<byte[]> EnrolamientoCompleto; // template serializado
        public event Action<DPFP.Sample> MuestraCaptured;      // para verificación
        public event Action<string> Error;
        public event Action<bool> FingerTouched;        // true=dedo, false=levantó

        public int MuestrasRestantes =>
            _enrolamiento == null ? 4 : (int)_enrolamiento.FeaturesNeeded;

        // ── Constructor ─────────────────────────────────────────────
        public LectorHuella(ModoLector modo)
        {
            _modo = modo;
            if (modo == ModoLector.Enrolamiento)
                _enrolamiento = new Enrollment();
        }

        // ── Control de sesión ───────────────────────────────────────

        public void Iniciar()
        {
            if (_disposed) return;
            try
            {
                // Pausar el monitor global para que solo esta sesión reciba eventos
                GestorSensor.Instancia.PausarCaptura();

                _capture = new Capture();
                _capture.EventHandler = this;
                _capture.StartCapture();
            }
            catch (Exception ex)
            {
                GestorSensor.Instancia.ReanudarCaptura();
                Despachar(() => Error?.Invoke($"No se pudo iniciar el sensor: {ex.Message}"));
            }
        }

        public void Detener()
        {
            try { _capture?.StopCapture(); } catch { }
            // Devolver el sensor al monitor global
            GestorSensor.Instancia.ReanudarCaptura();
        }

        public void ReiniciarEnrolamiento()
        {
            _enrolamiento = new Enrollment();
        }

        // ── DPFP.Capture.EventHandler ───────────────────────────────

        public void OnComplete(object Capture, string ReaderSerialNumber, Sample sample)
        {
            try
            {
                var extractor = new FeatureExtraction();
                CaptureFeedback feedback = CaptureFeedback.None;
                FeatureSet features = new FeatureSet();   // ← new, no null

                DataPurpose proposito = _modo == ModoLector.Enrolamiento
                    ? DataPurpose.Enrollment
                    : DataPurpose.Verification;

                extractor.CreateFeatureSet(sample, proposito, ref feedback, ref features);

                if (feedback != CaptureFeedback.Good)
                {
                    Despachar(() => Error?.Invoke(
                        "Calidad insuficiente. Coloca el dedo bien centrado y presiona con firmeza."));
                    return;
                }

                if (_modo == ModoLector.Enrolamiento)
                {
                    _enrolamiento.AddFeatures(features);
                    int restantes = (int)_enrolamiento.FeaturesNeeded;
                    Despachar(() => ProgresoEnrolamiento?.Invoke(restantes));

                    if (_enrolamiento.TemplateStatus == Enrollment.Status.Ready)
                    {
                        using (var ms = new MemoryStream())
                        {
                            _enrolamiento.Template.Serialize(ms);
                            byte[] bytes = ms.ToArray();
                            Detener();  // detiene captura y reanuda GestorSensor
                            Despachar(() => EnrolamientoCompleto?.Invoke(bytes));
                        }
                    }
                }
                else
                {
                    Despachar(() => MuestraCaptured?.Invoke(sample));
                }
            }
            catch (Exception ex)
            {
                Despachar(() => Error?.Invoke(ex.Message));
            }
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
            => Despachar(() => FingerTouched?.Invoke(true));

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
            => Despachar(() => FingerTouched?.Invoke(false));

        public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
            => Despachar(() => Error?.Invoke("El sensor fue desconectado durante la sesión."));

        public void OnStopped(object Capture, string ReaderSerialNumber) { }

        public void OnSampleQuality(object Capture, string ReaderSerialNumber,
                                    CaptureFeedback captureFeedback)
        {
            if (captureFeedback != CaptureFeedback.Good)
                Despachar(() => Error?.Invoke(
                    "Calidad insuficiente. Centra el dedo y presiona más fuerte."));
        }

        // ── Helpers ─────────────────────────────────────────────────

        private static void Despachar(Action accion)
        {
            var d = Application.Current?.Dispatcher;
            if (d == null) return;
            if (d.CheckAccess()) accion();
            else d.BeginInvoke(accion);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Detener();
            _capture = null;
        }
    }
}