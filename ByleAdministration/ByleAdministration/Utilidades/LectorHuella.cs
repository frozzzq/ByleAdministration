using System;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using DPFPVerification = DPFP.Verification.Verification;

namespace ByleAdministration.Utilidades
{
    /// <summary>
    /// Encapsula toda la comunicación con el sensor DigitalPersona 4500.
    /// El resto del sistema NO sabe que existe DPFP — solo habla con esta clase.
    /// </summary>
    public class LectorHuella : DPFP.Capture.EventHandler, IDisposable
    {
        // ══════════════════════════════════════════════════════════
        // MODO DE OPERACIÓN
        // El lector puede estar en uno de dos estados:
        //   Enrolamiento → registrando huella nueva (necesita 4 capturas)
        //   Verificacion → comparando contra una huella ya guardada
        // ══════════════════════════════════════════════════════════
        public enum Modo { Enrolamiento, Verificacion }

        // ══════════════════════════════════════════════════════════
        // CAMPOS PRIVADOS (encapsulados — nadie los toca desde fuera)
        // ══════════════════════════════════════════════════════════
        private Capture _capturador;          // objeto del SDK que escucha al sensor
        private Enrollment _enrolador;        // acumula las 4 muestras del enrolamiento
        private Modo _modo;

        // ══════════════════════════════════════════════════════════
        // EVENTOS PÚBLICOS
        // La ViewModel se suscribe a estos para reaccionar en la UI.
        // Usamos eventos propios (no los de DPFP) para no exponer el SDK.
        // ══════════════════════════════════════════════════════════

        /// <summary>Dispara cuando el sensor se conecta o desconecta.</summary>
        public event Action<bool> SensorConectado;

        /// <summary>Dispara cuando el usuario toca o levanta el dedo.</summary>
        public event Action<string> EstadoCambiado;

        /// <summary>
        /// Dispara durante el enrolamiento: capturasFaltantes baja de 4 a 0.
        /// Cuando llega a 0 y hay template, ya terminó.
        /// </summary>
        public event Action<int> ProgresoEnrolamiento;

        /// <summary>Dispara cuando el enrolamiento termina con éxito.</summary>
        public event Action<byte[]> EnrolamientoCompleto;

        /// <summary>Dispara cuando la verificación termina (exitosa o fallida).</summary>
        public event Action<bool, int> VerificacionCompleta;

        // Para verificación necesitamos guardar el template contra el que comparamos
        private Template _templateParaVerificar;
        private FeatureSet _ultimoFeatureSet;

        // ══════════════════════════════════════════════════════════
        // MÉTODOS PÚBLICOS
        // ══════════════════════════════════════════════════════════

        /// <summary>
        /// Inicia el proceso de registro de huella (4 capturas).
        /// </summary>
        public void IniciarEnrolamiento()
        {
            _modo = Modo.Enrolamiento;
            _enrolador = new Enrollment();
            ProgresoEnrolamiento?.Invoke((int)_enrolador.FeaturesNeeded);  // ← cast
            IniciarCaptura();
        }

        /// <summary>
        /// Inicia el proceso de verificación contra una huella previamente guardada.
        /// </summary>
        /// <param name="templateGuardado">Los bytes leídos de biometria.huella_digital</param>
        public void IniciarVerificacion(byte[] templateGuardado)
        {
            if (templateGuardado == null || templateGuardado.Length == 0)
                throw new ArgumentException("El template no puede estar vacío.");

            _modo = Modo.Verificacion;
            _templateParaVerificar = new Template();
            _templateParaVerificar.DeSerialize(templateGuardado);
            IniciarCaptura();
        }

        /// <summary>Detiene la captura actual.</summary>
        public void Detener()
        {
            if (_capturador != null)
            {
                try { _capturador.StopCapture(); } catch { /* ya estaba detenido */ }
            }
        }

        // ══════════════════════════════════════════════════════════
        // LÓGICA INTERNA
        // ══════════════════════════════════════════════════════════

        private void IniciarCaptura()
        {
            try
            {
                if (_capturador == null)
                {
                    _capturador = new Capture();
                    _capturador.EventHandler = this;   // ← yo me encargo de los eventos
                }
                _capturador.StartCapture();
                EstadoCambiado?.Invoke("Coloca el dedo en el sensor");
            }
            catch (Exception ex)
            {
                EstadoCambiado?.Invoke("Error: " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════
        // EVENTOS DEL SDK (ojo: se disparan en un hilo del driver,
        // NO en el hilo de la UI — por eso la VM usa Dispatcher.Invoke)
        // ══════════════════════════════════════════════════════════

        public void OnReaderConnect(object capture, string readerSerialNumber)
        {
            SensorConectado?.Invoke(true);
        }

        public void OnReaderDisconnect(object capture, string readerSerialNumber)
        {
            SensorConectado?.Invoke(false);
        }

        public void OnFingerTouch(object capture, string readerSerialNumber)
        {
            EstadoCambiado?.Invoke("Procesando huella...");
        }

        public void OnFingerGone(object capture, string readerSerialNumber)
        {
            EstadoCambiado?.Invoke("Dedo retirado");
        }

        public void OnSampleQuality(object capture, string readerSerialNumber, CaptureFeedback feedback)
        {
            if (feedback != CaptureFeedback.Good)
                EstadoCambiado?.Invoke("Calidad baja, intenta de nuevo");
        }

        public void OnComplete(object capture, string readerSerialNumber, Sample sample)
        {
            // Extraer las características de la muestra según el modo actual
            DataPurpose proposito = (_modo == Modo.Enrolamiento)
                ? DataPurpose.Enrollment
                : DataPurpose.Verification;

            FeatureSet caracteristicas = ExtraerCaracteristicas(sample, proposito);
            if (caracteristicas == null)
            {
                EstadoCambiado?.Invoke("No se pudieron leer las características, repite");
                return;
            }

            if (_modo == Modo.Enrolamiento)
                ProcesarEnrolamiento(caracteristicas);
            else
                ProcesarVerificacion(caracteristicas);
        }

        // ══════════════════════════════════════════════════════════
        // AUXILIARES PRIVADAS
        // ══════════════════════════════════════════════════════════

        private FeatureSet ExtraerCaracteristicas(Sample muestra, DataPurpose proposito)
        {
            var extractor = new FeatureExtraction();
            var feedback = CaptureFeedback.None;
            var caracteristicas = new FeatureSet();
            extractor.CreateFeatureSet(muestra, proposito, ref feedback, ref caracteristicas);
            return (feedback == CaptureFeedback.Good) ? caracteristicas : null;
        }

        private void ProcesarEnrolamiento(FeatureSet caracteristicas)
        {
            _enrolador.AddFeatures(caracteristicas);
            ProgresoEnrolamiento?.Invoke((int)_enrolador.FeaturesNeeded);  // ← cast

            switch (_enrolador.TemplateStatus)
            {
                case Enrollment.Status.Ready:
                    // Serializar el template a bytes usando MemoryStream
                    var memoria = new System.IO.MemoryStream();
                    _enrolador.Template.Serialize(memoria);
                    byte[] templateBytes = memoria.ToArray();

                    Detener();
                    EnrolamientoCompleto?.Invoke(templateBytes);
                    break;

                case Enrollment.Status.Failed:
                    _enrolador.Clear();
                    ProgresoEnrolamiento?.Invoke((int)_enrolador.FeaturesNeeded);  // ← cast
                    EstadoCambiado?.Invoke("Las huellas no coinciden, empezamos de nuevo");
                    break;
            }
        }

        private void ProcesarVerificacion(FeatureSet caracteristicas)
        {
            var verificador = new DPFPVerification();
            var resultado = new DPFPVerification.Result();
            verificador.Verify(caracteristicas, _templateParaVerificar, ref resultado);

            Detener();
            VerificacionCompleta?.Invoke(resultado.Verified, (int)resultado.FARAchieved);  // ← ya tenía cast, déjalo así
        }

        // ══════════════════════════════════════════════════════════
        // LIBERAR RECURSOS
        // ══════════════════════════════════════════════════════════

        public void Dispose()
        {
            Detener();
            _capturador = null;
            _enrolador = null;
            _templateParaVerificar = null;
        }
    }
}