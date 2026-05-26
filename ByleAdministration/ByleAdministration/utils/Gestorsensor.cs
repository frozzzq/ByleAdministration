using System;
using System.Windows;
using DPFP;
using DPFP.Capture;

namespace ByleAdministration.Utilidades
{
    public enum EstadoSensor
    {
        Desconectado,
        Conectado,
        Listo,
        Capturando,
        Error
    }

    /// <summary>
    /// Singleton que monitorea el estado del sensor globalmente.
    /// REGLA: solo métodos públicos implementan la interfaz — NO agregar
    /// implementaciones explícitas "void DPFP.Capture.EventHandler.OnX(){}"
    /// porque en C# las explícitas tienen prioridad y silencian la lógica.
    /// </summary>
    public class GestorSensor : DPFP.Capture.EventHandler
    {
        // ── Singleton ───────────────────────────────────────────────
        private static readonly Lazy<GestorSensor> _instancia =
            new Lazy<GestorSensor>(() => new GestorSensor());

        public static GestorSensor Instancia => _instancia.Value;

        // ── Estado ─────────────────────────────────────────────────
        public EstadoSensor Estado { get; private set; } = EstadoSensor.Desconectado;
        public string ModeloSensor { get; private set; } = "No detectado";
        public string SerialSensor { get; private set; } = "—";
        private bool _pausado = false;

        public event Action<EstadoSensor> EstadoCambiado;

        private Capture _capture;

        private GestorSensor() => Inicializar();

        private void Inicializar()
        {
            try
            {
                _capture = new Capture();
                _capture.EventHandler = this;
                _capture.StartCapture();
            }
            catch
            {
                ModeloSensor = "Error al inicializar";
                CambiarEstado(EstadoSensor.Error);
            }
        }

        // ── Ceder el sensor a LectorHuella durante enrolamiento ─────

        /// <summary>
        /// LectorHuella llama a esto antes de StartCapture para evitar
        /// que dos instancias de Capture compitan por el mismo sensor.
        /// </summary>
        public void PausarCaptura()
        {
            _pausado = true;
            try { _capture?.StopCapture(); } catch { /* ya detenida */ }
        }

        /// <summary>
        /// LectorHuella llama a esto cuando termina su sesión de captura.
        /// </summary>
        public void ReanudarCaptura()
        {
            _pausado = false;
            try { _capture?.StartCapture(); } catch { }
            // Mantener el último estado conocido; OnReaderConnect disparará si sigue conectado
        }

        private void CambiarEstado(EstadoSensor nuevoEstado)
        {
            if (_pausado) return;   // ignorar eventos mientras LectorHuella usa el sensor
            Estado = nuevoEstado;
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                EstadoCambiado?.Invoke(nuevoEstado)));
        }

        // ── DPFP.Capture.EventHandler ───────────────────────────────

        public void OnReaderConnect(object Capture, string ReaderSerialNumber)
        {
            ModeloSensor = "U.are.U 4500";
            SerialSensor = ReaderSerialNumber ?? "—";
            CambiarEstado(EstadoSensor.Listo);
        }

        public void OnReaderDisconnect(object Capture, string ReaderSerialNumber)
        {
            ModeloSensor = "No detectado";
            SerialSensor = "—";
            // La desconexión siempre se propaga, incluso en pausa
            Estado = EstadoSensor.Desconectado;
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
                EstadoCambiado?.Invoke(EstadoSensor.Desconectado)));
        }

        public void OnFingerTouch(object Capture, string ReaderSerialNumber)
            => CambiarEstado(EstadoSensor.Capturando);

        public void OnFingerGone(object Capture, string ReaderSerialNumber)
        {
            if (Estado == EstadoSensor.Capturando)
                CambiarEstado(EstadoSensor.Listo);
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, Sample Sample)
            => CambiarEstado(EstadoSensor.Listo);

        public void OnStopped(object Capture, string ReaderSerialNumber)
            => CambiarEstado(EstadoSensor.Desconectado);

        public void OnSampleQuality(object Capture, string ReaderSerialNumber,
                                    CaptureFeedback CaptureFeedback)
        { }
    }
}