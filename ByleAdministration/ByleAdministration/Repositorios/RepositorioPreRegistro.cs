using ByleAdministration.Modelos;
using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace ByleAdministration.Repositorios
{
    public class RepositorioPreRegistro
    {
        public List<PreRegistro> ObtenerPendientes()
        {
            var lista = new List<PreRegistro>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT pr.*, m.nombre_membresia
                    FROM   pre_registros pr
                    LEFT JOIN membresias m ON pr.id_membresia = m.id_membresia
                    WHERE  pr.estado = 'pendiente' AND pr.expira_en > NOW()
                    ORDER BY pr.fecha_registro ASC";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        lista.Add(Mapear(r));
            }
            return lista;
        }

        public void MarcarAtendido(int idPre)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "UPDATE pre_registros SET estado = 'atendido' WHERE id_pre = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idPre);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ExpirarVencidos()
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    @"UPDATE pre_registros SET estado = 'expirado'
                      WHERE  estado = 'pendiente' AND expira_en < NOW()", conn))
                    cmd.ExecuteNonQuery();
            }
        }

        private static PreRegistro Mapear(MySqlDataReader r)
        {
            int ordNomMem = r.GetOrdinal("nombre_membresia");
            int ordEdad   = r.GetOrdinal("edad");
            int ordCiud   = r.GetOrdinal("ciudad");
            int ordCorr   = r.GetOrdinal("correo");
            int ordTel    = r.GetOrdinal("telefono");
            int ordTelE   = r.GetOrdinal("telefono_emergencia");
            int ordMem    = r.GetOrdinal("id_membresia");

            return new PreRegistro
            {
                IdPre              = r.GetInt32("id_pre"),
                NombreCompleto     = r.GetString("nombre_completo"),
                Edad               = r.IsDBNull(ordEdad)  ? (int?)null  : r.GetInt32(ordEdad),
                Ciudad             = r.IsDBNull(ordCiud)  ? null        : r.GetString(ordCiud),
                Correo             = r.IsDBNull(ordCorr)  ? null        : r.GetString(ordCorr),
                Telefono           = r.IsDBNull(ordTel)   ? (long?)null : r.GetInt64(ordTel),
                TelefonoEmergencia = r.IsDBNull(ordTelE)  ? (long?)null : r.GetInt64(ordTelE),
                IdMembresia        = r.IsDBNull(ordMem)   ? (int?)null  : r.GetInt32(ordMem),
                NombreMembresia    = r.IsDBNull(ordNomMem)? null        : r.GetString(ordNomMem),
                FechaRegistro      = r.GetDateTime("fecha_registro"),
                ExpiraEn           = r.GetDateTime("expira_en"),
                Estado             = r.GetString("estado"),
            };
        }
    }
}
