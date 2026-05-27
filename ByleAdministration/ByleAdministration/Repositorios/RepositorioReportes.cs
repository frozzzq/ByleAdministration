using ByleAdministration.Soportes;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace ByleAdministration.Repositorios
{
    // ── Modelos de reporte ────────────────────────────────────────

    public class ReporteMetricas
    {
        public decimal IngresosTotales   { get; set; }
        public int     VentasRegistradas { get; set; }
        public int     ClientesNuevos    { get; set; }
        public int     Renovaciones      { get; set; }
        // Período anterior (para calcular variación)
        public decimal IngresosPrev      { get; set; }
        public int     VentasPrev        { get; set; }
        public int     ClientesPrev      { get; set; }
        public int     RenovacionesPrev  { get; set; }
    }

    public class VentaDetalle
    {
        public string   NombreCliente { get; set; }
        public string   Concepto      { get; set; }
        public DateTime Fecha         { get; set; }
        public decimal  Monto         { get; set; }
        public string   TipoVenta     { get; set; }
    }

    public class TopMembresia
    {
        public string  Nombre   { get; set; }
        public int     Cantidad { get; set; }
        public decimal Total    { get; set; }
    }

    public class TopProducto
    {
        public string  Nombre   { get; set; }
        public int     Cantidad { get; set; }
        public decimal Total    { get; set; }
    }

    // ── Repositorio ──────────────────────────────────────────────

    public class RepositorioReportes
    {
        public ReporteMetricas ObtenerMetricas(DateTime desde, DateTime hasta)
        {
            var m      = new ReporteMetricas();
            var span   = hasta - desde;
            var pDesde = desde - span;
            var pHasta = desde;

            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();

                // Ingresos + ventas del período
                Leer1(conn,
                    @"SELECT COALESCE(SUM(monto_total),0), COUNT(*)
                      FROM ventas WHERE fecha >= @d AND fecha <= @h",
                    desde, hasta,
                    r => { m.IngresosTotales = r.GetDecimal(0); m.VentasRegistradas = r.GetInt32(1); });

                // Ingresos + ventas período anterior
                Leer1(conn,
                    @"SELECT COALESCE(SUM(monto_total),0), COUNT(*)
                      FROM ventas WHERE fecha >= @d AND fecha <= @h",
                    pDesde, pHasta,
                    r => { m.IngresosPrev = r.GetDecimal(0); m.VentasPrev = r.GetInt32(1); });

                // Clientes nuevos (por fecha de inscripción)
                m.ClientesNuevos = Contar(conn,
                    "SELECT COUNT(*) FROM usuarios WHERE fecha_inscripcion >= @d AND fecha_inscripcion <= @h",
                    desde, hasta);
                m.ClientesPrev = Contar(conn,
                    "SELECT COUNT(*) FROM usuarios WHERE fecha_inscripcion >= @d AND fecha_inscripcion <= @h",
                    pDesde, pHasta);

                // Renovaciones (fecha_renovacion en el período)
                m.Renovaciones = Contar(conn,
                    "SELECT COUNT(*) FROM usuarios WHERE fecha_renovacion >= @d AND fecha_renovacion <= @h",
                    desde, hasta);
                m.RenovacionesPrev = Contar(conn,
                    "SELECT COUNT(*) FROM usuarios WHERE fecha_renovacion >= @d AND fecha_renovacion <= @h",
                    pDesde, pHasta);
            }

            return m;
        }

        public List<VentaDetalle> ObtenerUltimasVentas(DateTime desde, DateTime hasta, int limite = 25)
        {
            var lista = new List<VentaDetalle>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT u.nombre_completo, v.fecha, v.monto_total, v.tipo_venta,
                           COALESCE(m.nombre_membresia, '') AS nombre_membresia
                    FROM   ventas v
                    JOIN   usuarios   u ON v.id_usuario   = u.id_usuario
                    LEFT JOIN membresias m ON u.id_membresia = m.id_membresia
                    WHERE  v.fecha >= @d AND v.fecha <= @h
                    ORDER  BY v.fecha DESC
                    LIMIT  @lim";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@d",   desde);
                    cmd.Parameters.AddWithValue("@h",   hasta);
                    cmd.Parameters.AddWithValue("@lim", limite);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            string tipo   = r.GetString("tipo_venta");
                            string nomMem = r.GetString("nombre_membresia");
                            lista.Add(new VentaDetalle
                            {
                                NombreCliente = r.GetString("nombre_completo"),
                                Concepto = tipo == "membresia" && !string.IsNullOrEmpty(nomMem)
                                           ? $"Membresía {nomMem}"
                                           : tipo == "clase" ? "Clase grupal" : "Producto",
                                Fecha     = r.GetDateTime("fecha"),
                                Monto     = r.GetDecimal("monto_total"),
                                TipoVenta = tipo,
                            });
                        }
                }
            }
            return lista;
        }

        public List<TopMembresia> ObtenerTopMembresias(DateTime desde, DateTime hasta)
        {
            var lista = new List<TopMembresia>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT m.nombre_membresia,
                           COUNT(*)           AS cantidad,
                           SUM(v.monto_total) AS total
                    FROM   ventas v
                    JOIN   usuarios   u ON v.id_usuario   = u.id_usuario
                    JOIN   membresias m ON u.id_membresia = m.id_membresia
                    WHERE  v.tipo_venta = 'membresia'
                      AND  v.fecha >= @d AND v.fecha <= @h
                    GROUP  BY m.id_membresia, m.nombre_membresia
                    ORDER  BY cantidad DESC
                    LIMIT  5";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@d", desde);
                    cmd.Parameters.AddWithValue("@h", hasta);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            lista.Add(new TopMembresia
                            {
                                Nombre   = r.GetString("nombre_membresia"),
                                Cantidad = r.GetInt32("cantidad"),
                                Total    = r.GetDecimal("total"),
                            });
                }
            }
            return lista;
        }

        public List<TopProducto> ObtenerTopProductos(DateTime desde, DateTime hasta)
        {
            var lista = new List<TopProducto>();
            using (var conn = SoporteDatabase.ObtenerConexion())
            {
                conn.Open();
                const string sql = @"
                    SELECT p.nombre_producto,
                           SUM(o.cantidad)    AS cantidad,
                           SUM(o.monto_total) AS total
                    FROM   orden_web o
                    JOIN   productos p ON o.id_producto = p.id_producto
                    WHERE  o.fecha >= @d AND o.fecha <= @h
                    GROUP  BY p.id_producto, p.nombre_producto
                    ORDER  BY cantidad DESC
                    LIMIT  5";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@d", desde);
                    cmd.Parameters.AddWithValue("@h", hasta);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                            lista.Add(new TopProducto
                            {
                                Nombre   = r.GetString("nombre_producto"),
                                Cantidad = r.GetInt32("cantidad"),
                                Total    = r.GetDecimal("total"),
                            });
                }
            }
            return lista;
        }

        // ── Helpers ──────────────────────────────────────────────

        private static void Leer1(MySqlConnection conn, string sql,
                                   DateTime d, DateTime h, Action<MySqlDataReader> cb)
        {
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@d", d);
                cmd.Parameters.AddWithValue("@h", h);
                using (var r = cmd.ExecuteReader())
                    if (r.Read()) cb(r);
            }
        }

        private static int Contar(MySqlConnection conn, string sql, DateTime d, DateTime h)
        {
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@d", d);
                cmd.Parameters.AddWithValue("@h", h);
                using (var r = cmd.ExecuteReader())
                    return r.Read() ? r.GetInt32(0) : 0;
            }
        }
    }
}
