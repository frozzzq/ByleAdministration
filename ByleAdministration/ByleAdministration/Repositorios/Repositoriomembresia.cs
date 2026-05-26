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