using System;

namespace ByleAdministration.Modelos
{
    public class PreRegistro
    {
        public int    IdPre              { get; set; }
        public string NombreCompleto     { get; set; }
        public int?   Edad               { get; set; }
        public string Ciudad             { get; set; }
        public string Correo             { get; set; }
        public long?  Telefono           { get; set; }
        public long?  TelefonoEmergencia { get; set; }
        public int?   IdMembresia        { get; set; }
        public string NombreMembresia    { get; set; }
        public DateTime FechaRegistro   { get; set; }
        public DateTime ExpiraEn        { get; set; }
        public string Estado             { get; set; }
    }
}
