using System;
using System.IO;
using ByleAdministration.Repositorios;
using DPFP;
using DPFP.Processing;
using DPFP.Verification;
using DPFP.Capture;

namespace ByleAdministration.Servicios
{
    public class ServicioBiometria
    {
        private readonly RepositorioBiometria _repoBiometria = new RepositorioBiometria();
        private readonly RepositorioCliente _repoCliente = new RepositorioCliente();

        // ── Registro ───────────────────────────────────────────────

        public bool RegistrarHuella(int idUsuario, byte[] templateBytes)
        {
            try { return _repoBiometria.Guardar(idUsuario, templateBytes); }
            catch { return false; }
        }

        public bool TieneHuella(int idUsuario)
            => _repoBiometria.ExisteEnrolamiento(idUsuario);

        // ── Identificación 1:N ─────────────────────────────────────

        /// <summary>
        /// Compara la muestra contra todos los templates en la BD.
        /// Devuelve el IdUsuario del primer match, o null si no hay coincidencia.
        /// </summary>
        public int? Identificar(Sample muestra)
        {
            try
            {
                // 1. Extraer features de la muestra capturada
                var extractor = new FeatureExtraction();
                var feedback = CaptureFeedback.None;
                var features = new FeatureSet();

                extractor.CreateFeatureSet(muestra, DataPurpose.Verification,
                                           ref feedback, ref features);

                if (feedback != CaptureFeedback.Good) return null;

                // 2. Comparar contra todos los templates
                var templates = _repoBiometria.ObtenerTodos();
                if (templates.Count == 0) return null;

                var verificacion = new Verification();

                // Acceso explícito al item para evitar CS8130 en desconstrucción
                for (int i = 0; i < templates.Count; i++)
                {
                    int idUsuario = templates[i].IdUsuario;
                    byte[] templateBytes = templates[i].Template;

                    try
                    {
                        var template = DeserializarTemplate(templateBytes);
                        Verification.Result resultado = null;
                        verificacion.Verify(features, template, ref resultado);

                        if (resultado != null && resultado.Verified)
                            return idUsuario;
                    }
                    catch { /* template corrupto, continuar */ }
                }

                return null;
            }
            catch { return null; }
        }

        // ── Verificación 1:1 ───────────────────────────────────────

        public bool Verificar(Sample muestra, int idUsuario)
        {
            try
            {
                var templateBytes = _repoBiometria.ObtenerPorUsuario(idUsuario);
                if (templateBytes == null) return false;

                var extractor = new FeatureExtraction();
                var feedback = CaptureFeedback.None;
                var features = new FeatureSet();
                extractor.CreateFeatureSet(muestra, DataPurpose.Verification,
                                           ref feedback, ref features);

                if (feedback != CaptureFeedback.Good) return false;

                var template = DeserializarTemplate(templateBytes);
                var verificacion = new Verification();
                Verification.Result resultado = null;
                verificacion.Verify(features, template, ref resultado);

                return resultado != null && resultado.Verified;
            }
            catch { return false; }
        }

        // ── Helper ─────────────────────────────────────────────────

        private static Template DeserializarTemplate(byte[] bytes)
        {
            var t = new Template();
            using (var ms = new MemoryStream(bytes))
                t.DeSerialize(ms);
            return t;
        }
    }
}