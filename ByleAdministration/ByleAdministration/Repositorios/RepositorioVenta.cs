using System;
using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;

namespace ByleAdministration.Repositorios
{
    public class RepositorioVenta
    {
        public void Insertar(int idUsuario, int idEmpleado, decimal montoTotal,
                             string tipoVenta, int cantidad = 1)
        {
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                string sql = @"
                    INSERT INTO ventas (id_usuario, id_empleado, fecha, monto_total, cantidad, tipo_venta)
                    VALUES (@uid, @eid, NOW(), @monto, @cant, @tipo)";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid",   idUsuario);
                    cmd.Parameters.AddWithValue("@eid",   idEmpleado);
                    cmd.Parameters.AddWithValue("@monto", montoTotal);
                    cmd.Parameters.AddWithValue("@cant",  cantidad);
                    cmd.Parameters.AddWithValue("@tipo",  tipoVenta);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
