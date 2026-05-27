using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByleAdministration.Modelos
{

    public class Membresia
    {
        public int IdMembresia { get; set; }
        public string NombreMembresia { get; set; }
        public decimal Precio { get; set; }
        public int DuracionDias { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; } // activa | inactiva | temporada | oferta
        public int    NumClientes { get; set; } // no persisted, loaded with ObtenerTodasConConteo

        public override string ToString() => NombreMembresia;
    }
}