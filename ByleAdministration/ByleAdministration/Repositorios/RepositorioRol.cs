using ByleAdministration.Modelos;
using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace ByleAdministration.Repositorios
{
    public class RepositorioRol
    {
        public List<Roles> ObtenerTodos()
        {
            var lista = new List<Roles>();
            using (var conn = SoporteDatabase.ObtenerConexion())  // ← igual que tus otros repositorios
            {
                conn.Open();
                var cmd = new MySqlCommand("SELECT id_rol, nombre_rol FROM roles", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Roles
                        {
                            IdRol = reader.GetInt32("id_rol"),
                            NombreRol = reader.GetString("nombre_rol")
                        });
                    }
                }
            }
            return lista;
        }
    }
}