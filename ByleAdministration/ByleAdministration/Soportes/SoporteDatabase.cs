using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace ByleAdministration.Soportes
{
    public static class SoporteDatabase
    {
        public static MySqlConnection ObtenerConexion()
        {
            string conn = ConfigurationManager.ConnectionStrings["ByleAdministration"].ConnectionString;
            return new MySqlConnection(conn);
        }
    }
}
