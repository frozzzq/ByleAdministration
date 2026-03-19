using ByleAdministration.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ByleAdministration.Soportes;
using ByleAdministration.Modelos;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace ByleAdministration.Repositorios
{
    public class RepositorioEmpleado
    {
        public bool Insertar(Empleados empleados) 
        {
            using (var conexion = SoporteDatabase.ObtenerConexion()) 
            {
            conexion.Open();







            }
            return false;
        }
    }
}
