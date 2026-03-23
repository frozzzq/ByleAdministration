using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByleAdministration.Modelos
{
    public class Cliente
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public int Edad { get; set; }
        public string Ciudad { get; set; }
        public long Telefono { get; set; }
        public long TelefonoEmergencia { get; set; }
        public string Correo { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public DateTime? FechaRenovacion { get; set; }
        public int IdMembresia { get; set; }
        public string NombreMembresia { get; set; } // viene del JOIN
        public string Estado { get; set; }

        // Helpers para la vista
        public string Iniciales
        {
            get
            {
                if (string.IsNullOrEmpty(NombreCompleto)) return "??";
                var partes = NombreCompleto.Trim().Split(' ');
                if (partes.Length >= 2)
                    return (partes[0][0].ToString() + partes[1][0].ToString()).ToUpper();
                return partes[0].Substring(0, Math.Min(2, partes[0].Length)).ToUpper();
            }
        }

        public bool EstaActivo => Estado?.ToLower() == "activo";

        public string TelefonoFormateado
        {
            get
            {
                string t = Telefono.ToString();
                if (t.Length == 10)
                    return $"{t.Substring(0, 3)} {t.Substring(3, 3)} {t.Substring(6, 4)}";
                return t;
            }
        }
    }
}
