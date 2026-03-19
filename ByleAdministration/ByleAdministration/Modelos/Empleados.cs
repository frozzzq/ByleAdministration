using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByleAdministration.Modelos
{
    public class Empleados
    {
        public string NombreCompleto { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string ContraseñaHash { get; set; }
        public string Rfc { get; set; }
        public int IdRol { get; set; }
    }
}
