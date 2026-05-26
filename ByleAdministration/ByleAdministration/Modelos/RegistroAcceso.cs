using System;

namespace ByleAdministration.Modelos
{
    /// <summary>
    /// Representa un evento de entrada o salida en la bitácora del día.
    /// </summary>
    public class RegistroAcceso
    {
        public int IdAcceso { get; set; }
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string NombreMembresia { get; set; }
        public DateTime FechaHoraEntrada { get; set; }
        public DateTime? FechaHoraSalida { get; set; }
        public string MetodoVerificacion { get; set; }

        // ── Propiedades calculadas ──────────────────────────────
        public string Iniciales
        {
            get
            {
                if (string.IsNullOrWhiteSpace(NombreCompleto)) return "??";
                var partes = NombreCompleto.Trim().Split(' ');
                return partes.Length >= 2
                    ? $"{partes[0][0]}{partes[1][0]}".ToUpper()
                    : partes[0].Substring(0, Math.Min(2, partes[0].Length)).ToUpper();
            }
        }

        /// <summary>Hora del evento más reciente (entrada o salida).</summary>
        public DateTime HoraEvento { get; set; }

        /// <summary>true = entrada, false = salida.</summary>
        public bool EsEntrada { get; set; }

        public string HoraFormateada => HoraEvento.ToString("HH:mm");
    }
}