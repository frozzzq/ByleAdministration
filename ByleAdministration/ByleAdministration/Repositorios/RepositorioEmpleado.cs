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

                string sql = @"INSERT INTO empleados 
               (nombre_completo, correo, telefono, contraseña, RFC, id_rol) 
               VALUES 
               (@nombre, @correo, @telefono, @contrasena, @rfc, @IdRol)";

                using (var cmd = new MySqlCommand(sql, conexion)) 
                {
                    cmd.Parameters.AddWithValue("@nombre", empleados.NombreCompleto);
                    cmd.Parameters.AddWithValue("@correo", empleados.Correo);
                    cmd.Parameters.AddWithValue("@telefono", empleados.Telefono);
                    cmd.Parameters.AddWithValue("@contrasena", empleados.ContraseñaHash);
                    cmd.Parameters.AddWithValue("@rfc", empleados.Rfc);
                    cmd.Parameters.AddWithValue("@IdRol", empleados.IdRol);
                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }
        public Empleados ObtenerPorCorreo(string correo)
        {
            using (var conexion = SoporteDatabase.ObtenerConexion())
            {
                conexion.Open();
                string sql = "SELECT * FROM empleados WHERE correo = @correo";
                using (var cmd = new MySqlCommand(sql, conexion))
                {
                    cmd.Parameters.AddWithValue("@correo", correo);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Empleados
                            {
                                NombreCompleto = reader["nombre_completo"].ToString(),
                                Correo = reader["correo"].ToString(),
                                ContraseñaHash = reader["contraseña"].ToString(),  // ← faltaba esto
                                Telefono = reader["telefono"].ToString(),
                                Rfc = reader["RFC"].ToString(),
                                IdRol = Convert.ToInt32(reader["id_rol"])
                            };
                        }
                        return null;
                    }
                }
            }
        }
    }
}
