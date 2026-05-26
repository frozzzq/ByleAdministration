using System;
using System.Collections.Generic;
using ByleAdministration.Modelos;
using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;

namespace ByleAdministration.Repositorios
{
    public class RepositorioAcceso
    {
        // ─────────────────────────────────────────────────────────
        // ESCRITURA
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// Inserta una nueva fila con fecha_hora_entrada = NOW().
        /// Devuelve el id_acceso generado.
        /// </summary>
        public int RegistrarEntrada(int idUsuario, string metodo = "huella")
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO acceso (id_usuario, fecha_hora_entrada, metodo_verificacion)
                    VALUES (@id, NOW(), @metodo)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    cmd.Parameters.AddWithValue("@metodo", metodo);
                    cmd.ExecuteNonQuery();
                    return (int)cmd.LastInsertedId;
                }
            }
        }

        /// <summary>
        /// Completa el registro abierto del usuario con fecha_hora_salida = NOW().
        /// </summary>
        public bool RegistrarSalida(int idUsuario)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    UPDATE acceso
                    SET    fecha_hora_salida = NOW()
                    WHERE  id_usuario = @id
                      AND  fecha_hora_salida IS NULL
                    ORDER  BY fecha_hora_entrada DESC
                    LIMIT  1";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ─────────────────────────────────────────────────────────
        // LECTURA
        // ─────────────────────────────────────────────────────────

        /// <summary>
        /// true si el usuario tiene una entrada hoy sin salida registrada.
        /// </summary>
        public bool EstaAdentro(int idUsuario)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT COUNT(*) FROM acceso
                    WHERE  id_usuario = @id
                      AND  DATE(fecha_hora_entrada) = CURDATE()
                      AND  fecha_hora_salida IS NULL";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        /// <summary>
        /// Personas que entraron hoy y aún no han salido.
        /// </summary>
        public int ContarPersonasDentro()
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT COUNT(DISTINCT id_usuario) FROM acceso
                    WHERE  DATE(fecha_hora_entrada) = CURDATE()
                      AND  fecha_hora_salida IS NULL";

                using (var cmd = new MySqlCommand(sql, conn))
                    return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int ContarEntradasHoy()
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql =
                    "SELECT COUNT(*) FROM acceso WHERE DATE(fecha_hora_entrada) = CURDATE()";
                using (var cmd = new MySqlCommand(sql, conn))
                    return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int ContarSalidasHoy()
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT COUNT(*) FROM acceso
                    WHERE  fecha_hora_salida IS NOT NULL
                      AND  DATE(fecha_hora_salida) = CURDATE()";
                using (var cmd = new MySqlCommand(sql, conn))
                    return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        /// <summary>
        /// Bitácora del día: entradas y salidas como eventos separados, más recientes primero.
        /// </summary>
        public List<RegistroAcceso> ObtenerBitacoraHoy(int limite = 50)
        {
            var lista = new List<RegistroAcceso>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();

                // UNION: entrada + salida como filas independientes ordenadas por hora
                string sql = $@"
                    (SELECT a.id_acceso, a.id_usuario,
                            u.nombre_completo, m.nombre_membresia,
                            a.fecha_hora_entrada, a.fecha_hora_salida,
                            a.metodo_verificacion,
                            a.fecha_hora_entrada AS hora_evento,
                            1 AS es_entrada
                     FROM   acceso a
                     JOIN   usuarios u  ON a.id_usuario  = u.id_usuario
                     LEFT JOIN membresias m ON u.id_membresia = m.id_membresia
                     WHERE  DATE(a.fecha_hora_entrada) = CURDATE())

                    UNION ALL

                    (SELECT a.id_acceso, a.id_usuario,
                            u.nombre_completo, m.nombre_membresia,
                            a.fecha_hora_entrada, a.fecha_hora_salida,
                            a.metodo_verificacion,
                            a.fecha_hora_salida AS hora_evento,
                            0 AS es_entrada
                     FROM   acceso a
                     JOIN   usuarios u  ON a.id_usuario  = u.id_usuario
                     LEFT JOIN membresias m ON u.id_membresia = m.id_membresia
                     WHERE  a.fecha_hora_salida IS NOT NULL
                       AND  DATE(a.fecha_hora_salida) = CURDATE())

                    ORDER  BY hora_evento DESC
                    LIMIT  {limite}";

                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new RegistroAcceso
                        {
                            IdAcceso = r.GetInt32("id_acceso"),
                            IdUsuario = r.GetInt32("id_usuario"),
                            NombreCompleto = r["nombre_completo"].ToString(),
                            NombreMembresia = r["nombre_membresia"] == DBNull.Value
                                                    ? "—" : r["nombre_membresia"].ToString(),
                            FechaHoraEntrada = r.GetDateTime("fecha_hora_entrada"),
                            FechaHoraSalida = r["fecha_hora_salida"] == DBNull.Value
                                                    ? (DateTime?)null
                                                    : r.GetDateTime("fecha_hora_salida"),
                            MetodoVerificacion = r["metodo_verificacion"].ToString(),
                            HoraEvento = r.GetDateTime("hora_evento"),
                            EsEntrada = r.GetInt32("es_entrada") == 1
                        });
                    }
                }
            }
            return lista;
        }
    }
}