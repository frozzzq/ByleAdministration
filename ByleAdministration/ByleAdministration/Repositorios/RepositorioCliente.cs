using ByleAdministration.Modelos;
using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace ByleAdministration.Repositorios
{
    public class RepositorioCliente
    {
        // ── SQL base con JOIN a membresias ──────────────────────────
        // Todas las consultas de lectura usan este SELECT para que
        // NombreMembresia llegue lleno sin consultas adicionales.
        private const string SELECT_BASE = @"
            SELECT  u.*,
                    m.nombre_membresia
            FROM    usuarios u
            LEFT JOIN membresias m ON u.id_membresia = m.id_membresia";

        // ─────────────────────────────────────────────────────────────
        // ESCRITURA
        // ─────────────────────────────────────────────────────────────

        public int Insertar(Cliente c)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    INSERT INTO usuarios
                        (nombre_completo, edad, ciudad, telefono, telefono_emergencia,
                         correo, fecha_inscripcion, fecha_renovacion, id_membresia, estado)
                    VALUES
                        (@nombre, @edad, @ciudad, @tel, @telEm,
                         @correo, @inscripcion, @renovacion, @idMem, @estado)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", c.NombreCompleto);
                    cmd.Parameters.AddWithValue("@edad", c.Edad);
                    cmd.Parameters.AddWithValue("@ciudad", c.Ciudad);
                    cmd.Parameters.AddWithValue("@tel", c.Telefono);
                    cmd.Parameters.AddWithValue("@telEm", c.TelefonoEmergencia);
                    cmd.Parameters.AddWithValue("@correo", c.Correo);
                    cmd.Parameters.AddWithValue("@inscripcion", c.FechaInscripcion);
                    cmd.Parameters.AddWithValue("@renovacion", (object)c.FechaRenovacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@idMem", c.IdMembresia);
                    cmd.Parameters.AddWithValue("@estado", c.Estado ?? "activo");
                    cmd.ExecuteNonQuery();
                    return (int)cmd.LastInsertedId;
                }
            }
        }

        public bool Actualizar(Cliente c)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    UPDATE usuarios SET
                        nombre_completo     = @nombre,
                        edad                = @edad,
                        ciudad              = @ciudad,
                        telefono            = @tel,
                        telefono_emergencia = @telEm,
                        correo              = @correo,
                        id_membresia        = @idMem,
                        estado              = @estado,
                        fecha_inscripcion   = @inscripcion,
                        fecha_renovacion    = @renovacion
                    WHERE id_usuario = @id";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@nombre", c.NombreCompleto);
                    cmd.Parameters.AddWithValue("@edad", c.Edad);
                    cmd.Parameters.AddWithValue("@ciudad", c.Ciudad);
                    cmd.Parameters.AddWithValue("@tel", c.Telefono);
                    cmd.Parameters.AddWithValue("@telEm", c.TelefonoEmergencia);
                    cmd.Parameters.AddWithValue("@correo", c.Correo);
                    cmd.Parameters.AddWithValue("@idMem", c.IdMembresia);
                    cmd.Parameters.AddWithValue("@estado", c.Estado ?? "activo");
                    cmd.Parameters.AddWithValue("@inscripcion", c.FechaInscripcion);
                    cmd.Parameters.AddWithValue("@renovacion", (object)c.FechaRenovacion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", c.IdUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool CambiarEstado(int idUsuario, string estado)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = "UPDATE usuarios SET estado = @estado WHERE id_usuario = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@estado", estado);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // LECTURA  — todas usan SELECT_BASE (incluye JOIN a membresias)
        // ─────────────────────────────────────────────────────────────

        public Cliente ObtenerPorId(int id)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = SELECT_BASE + " WHERE u.id_usuario = @id LIMIT 1";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                        return r.Read() ? Mapear(r) : null;
                }
            }
        }

        public List<Cliente> ObtenerTodos()
        {
            var lista = new List<Cliente>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = SELECT_BASE + " ORDER BY u.nombre_completo";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(Mapear(r));
            }
            return lista;
        }

        public List<Cliente> ObtenerActivos()
        {
            var lista = new List<Cliente>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = SELECT_BASE + " WHERE u.estado = 'activo' ORDER BY u.nombre_completo";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) lista.Add(Mapear(r));
            }
            return lista;
        }

        public List<Cliente> BuscarPorNombre(string termino)
        {
            var lista = new List<Cliente>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = SELECT_BASE +
                    " WHERE u.nombre_completo LIKE @termino ORDER BY u.nombre_completo";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@termino", $"%{termino}%");
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) lista.Add(Mapear(r));
                }
            }
            return lista;
        }

        public List<Cliente> ObtenerPorVencer(int dias = 7)
        {
            var lista = new List<Cliente>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = SELECT_BASE + @"
                    WHERE u.estado = 'activo'
                      AND u.fecha_renovacion BETWEEN NOW()
                          AND DATE_ADD(NOW(), INTERVAL @dias DAY)
                    ORDER BY u.fecha_renovacion";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@dias", dias);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) lista.Add(Mapear(r));
                }
            }
            return lista;
        }

        // ─────────────────────────────────────────────────────────────
        // MAPEO
        // ─────────────────────────────────────────────────────────────

        private static Cliente Mapear(MySqlDataReader r)
        {
            return new Cliente
            {
                IdUsuario = r.GetInt32("id_usuario"),
                NombreCompleto = r["nombre_completo"].ToString(),
                Edad = r.GetInt32("edad"),
                Ciudad = r["ciudad"].ToString(),
                Telefono = Convert.ToInt64(r["telefono"]),
                TelefonoEmergencia = Convert.ToInt64(r["telefono_emergencia"]),
                Correo = r["correo"].ToString(),
                FechaInscripcion = r.GetDateTime("fecha_inscripcion"),
                FechaRenovacion = r["fecha_renovacion"] == DBNull.Value
                                        ? (DateTime?)null
                                        : r.GetDateTime("fecha_renovacion"),
                IdMembresia = r.GetInt32("id_membresia"),
                Estado = r["estado"].ToString(),

                // ← Viene del JOIN con membresias
                NombreMembresia = r["nombre_membresia"] == DBNull.Value
                                        ? "—"
                                        : r["nombre_membresia"].ToString()
            };
        }
    }
}