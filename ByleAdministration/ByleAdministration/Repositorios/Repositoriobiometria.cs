using System;
using System.Collections.Generic;
using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;

namespace ByleAdministration.Repositorios
{
    public class RepositorioBiometria
    {
        public bool Guardar(int idUsuario, byte[] templateBytes)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                bool existe = ExisteEnrolamiento(idUsuario, conn);

                string sql = existe
                    ? "UPDATE biometria SET huella_digital = @huella, fecha_registro = NOW() WHERE id_usuario = @id"
                    : "INSERT INTO biometria (id_usuario, huella_digital, fecha_registro) VALUES (@id, @huella, NOW())";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    var p = cmd.Parameters.AddWithValue("@huella", templateBytes);
                    p.MySqlDbType = MySql.Data.MySqlClient.MySqlDbType.MediumBlob;
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Eliminar(int idUsuario)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = "DELETE FROM biometria WHERE id_usuario = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public byte[] ObtenerPorUsuario(int idUsuario)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = "SELECT huella_digital FROM biometria WHERE id_usuario = @id LIMIT 1";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read() || reader.IsDBNull(0)) return null;
                        return (byte[])reader["huella_digital"];
                    }
                }
            }
        }

        public List<(int IdUsuario, byte[] Template)> ObtenerTodos()
        {
            var lista = new List<(int, byte[])>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = "SELECT id_usuario, huella_digital FROM biometria";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32("id_usuario");
                        byte[] bytes = (byte[])reader["huella_digital"];
                        lista.Add((id, bytes));
                    }
                }
            }
            return lista;
        }

        public bool ExisteEnrolamiento(int idUsuario)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                return ExisteEnrolamiento(idUsuario, conn);
            }
        }

        private bool ExisteEnrolamiento(int idUsuario, MySqlConnection conn)
        {
            const string sql = "SELECT COUNT(*) FROM biometria WHERE id_usuario = @id";
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", idUsuario);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}