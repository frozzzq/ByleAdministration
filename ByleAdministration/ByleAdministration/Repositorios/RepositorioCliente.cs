using ByleAdministration.Modelos;
using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace ByleAdministration.Repositorios
{
    public class RepositorioCliente
    {
        public List<Cliente> ObtenerTodos()
        {
            var lista = new List<Cliente>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT u.id_usuario, u.nombre_completo, u.edad, u.ciudad,
                                      u.telefono, u.telefono_emergencia, u.correo,
                                      u.fecha_inscripcion, u.fecha_renovacion,
                                      u.id_membresia, u.estado,
                                      m.nombre_membresia
                               FROM usuarios u
                               LEFT JOIN membresias m ON u.id_membresia = m.id_membresia
                               ORDER BY u.nombre_completo";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(MapearCliente(reader));
                    }
                }
            }
            return lista;
        }

        public List<Cliente> BuscarPorNombreOCorreo(string termino)
        {
            var lista = new List<Cliente>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT u.id_usuario, u.nombre_completo, u.edad, u.ciudad,
                                      u.telefono, u.telefono_emergencia, u.correo,
                                      u.fecha_inscripcion, u.fecha_renovacion,
                                      u.id_membresia, u.estado,
                                      m.nombre_membresia
                               FROM usuarios u
                               LEFT JOIN membresias m ON u.id_membresia = m.id_membresia
                               WHERE u.nombre_completo LIKE @termino
                                  OR u.correo LIKE @termino
                                  OR u.telefono LIKE @termino
                               ORDER BY u.nombre_completo";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@termino", "%" + termino + "%");
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearCliente(reader));
                        }
                    }
                }
            }
            return lista;
        }

        public List<Cliente> ObtenerPorEstado(string estado)
        {
            var lista = new List<Cliente>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = @"SELECT u.id_usuario, u.nombre_completo, u.edad, u.ciudad,
                                      u.telefono, u.telefono_emergencia, u.correo,
                                      u.fecha_inscripcion, u.fecha_renovacion,
                                      u.id_membresia, u.estado,
                                      m.nombre_membresia
                               FROM usuarios u
                               LEFT JOIN membresias m ON u.id_membresia = m.id_membresia
                               WHERE u.estado = @estado
                               ORDER BY u.nombre_completo";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@estado", estado);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(MapearCliente(reader));
                        }
                    }
                }
            }
            return lista;
        }

        private Cliente MapearCliente(MySqlDataReader reader)
        {
            return new Cliente
            {
                IdUsuario = reader.GetInt32("id_usuario"),
                NombreCompleto = reader["nombre_completo"].ToString(),
                Edad = reader.GetInt32("edad"),
                Ciudad = reader["ciudad"].ToString(),
                Telefono = reader.GetInt64("telefono"),
                TelefonoEmergencia = reader.GetInt64("telefono_emergencia"),
                Correo = reader["correo"].ToString(),
                FechaInscripcion = reader.GetDateTime("fecha_inscripcion"),
                FechaRenovacion = reader.IsDBNull(reader.GetOrdinal("fecha_renovacion"))
                    ? (DateTime?)null
                    : reader.GetDateTime("fecha_renovacion"),
                IdMembresia = reader.GetInt32("id_membresia"),
                Estado = reader["estado"].ToString(),
                NombreMembresia = reader.IsDBNull(reader.GetOrdinal("nombre_membresia"))
                    ? "Sin membresía"
                    : reader["nombre_membresia"].ToString()
            };
        }
    }
}