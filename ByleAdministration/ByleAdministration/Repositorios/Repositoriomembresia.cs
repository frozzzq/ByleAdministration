using System;
using System.Collections.Generic;
using ByleAdministration.Modelos;
using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;

namespace ByleAdministration.Repositorios
{
    /// <summary>
    /// Acceso directo a la tabla <c>membresias</c>.
    /// </summary>
    public class RepositorioMembresia
    {
        /// <summary>Devuelve los planes activos y en oferta, ordenados por precio.</summary>
        public List<Membresia> ObtenerActivas()
        {
            var lista = new List<Membresia>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT id_membresia, nombre_membresia, precio, duracion_dias, descripcion, estado
                    FROM   membresias
                    WHERE  estado IN ('activa', 'oferta')
                    ORDER  BY precio";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(Mapear(r));
            }
            return lista;
        }

        /// <summary>Devuelve todas las membresías independientemente del estado.</summary>
        public List<Membresia> ObtenerTodas()
        {
            var lista = new List<Membresia>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT id_membresia, nombre_membresia, precio, duracion_dias, descripcion, estado
                    FROM   membresias
                    ORDER  BY precio";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(Mapear(r));
            }
            return lista;
        }

        /// <summary>Devuelve todas las membresías con el conteo de clientes activos por plan.</summary>
        public List<Membresia> ObtenerTodasConConteo()
        {
            var lista = new List<Membresia>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT m.id_membresia, m.nombre_membresia, m.precio, m.duracion_dias, m.descripcion, m.estado,
                           COUNT(u.id_usuario) AS num_clientes
                    FROM   membresias m
                    LEFT   JOIN usuarios u ON u.id_membresia = m.id_membresia
                    GROUP  BY m.id_membresia, m.nombre_membresia, m.precio, m.duracion_dias, m.descripcion, m.estado
                    ORDER  BY m.precio";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var m = Mapear(r);
                        m.NumClientes = Convert.ToInt32(r["num_clientes"]);
                        lista.Add(m);
                    }
                }
            }
            return lista;
        }

        public int Insertar(Membresia m)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO membresias (nombre_membresia, precio, duracion_dias, descripcion, estado)
                    VALUES (@nombre, @precio, @dias, @desc, @estado);
                    SELECT LAST_INSERT_ID();";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", m.NombreMembresia);
                    cmd.Parameters.AddWithValue("@precio", m.Precio);
                    cmd.Parameters.AddWithValue("@dias",   m.DuracionDias);
                    cmd.Parameters.AddWithValue("@desc",   (object)m.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@estado", m.Estado);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void Actualizar(Membresia m)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    UPDATE membresias
                    SET nombre_membresia=@nombre, precio=@precio, duracion_dias=@dias,
                        descripcion=@desc, estado=@estado
                    WHERE id_membresia=@id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", m.NombreMembresia);
                    cmd.Parameters.AddWithValue("@precio", m.Precio);
                    cmd.Parameters.AddWithValue("@dias",   m.DuracionDias);
                    cmd.Parameters.AddWithValue("@desc",   (object)m.Descripcion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@estado", m.Estado);
                    cmd.Parameters.AddWithValue("@id",     m.IdMembresia);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CambiarEstado(int id, string estado)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = "UPDATE membresias SET estado=@estado WHERE id_membresia=@id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@estado", estado);
                    cmd.Parameters.AddWithValue("@id",     id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Membresia ObtenerPorId(int id)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = "SELECT * FROM membresias WHERE id_membresia = @id LIMIT 1";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? Mapear(r) : null;
                }
            }
        }

        private static Membresia Mapear(MySqlDataReader r)
        {
            return new Membresia
            {
                IdMembresia = r.GetInt32("id_membresia"),
                NombreMembresia = r.GetString("nombre_membresia"),
                Precio = r.GetDecimal("precio"),
                DuracionDias = r.GetInt32("duracion_dias"),
                Descripcion = r["descripcion"] == DBNull.Value ? null : r["descripcion"].ToString(),
                Estado = r.GetString("estado")
            };
        }
    }
}