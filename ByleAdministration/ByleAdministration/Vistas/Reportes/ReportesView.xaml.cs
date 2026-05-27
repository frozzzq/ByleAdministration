using ByleAdministration.Repositorios;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ByleAdministration.Vistas.Reportes
{
    public partial class ReportesView : UserControl
    {
        private readonly RepositorioReportes _repo = new RepositorioReportes();
        private string   _periodo = "hoy";
        private DateTime _desde;
        private DateTime _hasta;

        // ── Pinceles estáticos ───────────────────────────────────
        private static readonly SolidColorBrush _brAccent =
            new SolidColorBrush(Color.FromRgb(0xF2, 0x64, 0x19));
        private static readonly SolidColorBrush _brVerde =
            new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99));
        private static readonly SolidColorBrush _brRojo =
            new SolidColorBrush(Color.FromRgb(0xF8, 0x71, 0x71));
        private static readonly SolidColorBrush _brGris =
            new SolidColorBrush(Color.FromArgb(0x88, 0x88, 0x88, 0x88));

        public ReportesView()
        {
            InitializeComponent();
            Loaded += (s, e) => AplicarPeriodo("hoy");
        }

        // ════════════════════════════════════════════════════════
        //  PERÍODO
        // ════════════════════════════════════════════════════════

        private void BtnPeriodo_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b) AplicarPeriodo(b.Tag?.ToString() ?? "hoy");
        }

        private void BtnAplicar_Click(object sender, RoutedEventArgs e)
        {
            if (!DpDesde.SelectedDate.HasValue || !DpHasta.SelectedDate.HasValue)
            {
                MessageBox.Show("Selecciona ambas fechas antes de aplicar.", "Fechas requeridas",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _desde = DpDesde.SelectedDate.Value.Date;
            _hasta = DpHasta.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);
            CargarDatos();
        }

        private void AplicarPeriodo(string periodo)
        {
            _periodo = periodo;
            ActualizarBotonesPeriodo();

            bool esCustom = periodo == "personalizado";
            DpDesde.IsEnabled = esCustom;
            DpHasta.IsEnabled = esCustom;

            if (!esCustom)
            {
                var hoy = DateTime.Today;
                switch (periodo)
                {
                    case "hoy":
                        _desde = hoy;
                        _hasta = hoy.AddDays(1).AddSeconds(-1);
                        break;
                    case "semana":
                        int offset = ((int)hoy.DayOfWeek + 6) % 7; // días desde el lunes
                        _desde = hoy.AddDays(-offset);
                        _hasta = _desde.AddDays(7).AddSeconds(-1);
                        break;
                    case "mes":
                        _desde = new DateTime(hoy.Year, hoy.Month, 1);
                        _hasta = _desde.AddMonths(1).AddSeconds(-1);
                        break;
                }
                DpDesde.SelectedDate = _desde.Date;
                DpHasta.SelectedDate = _hasta.Date;
                CargarDatos();
            }
            else
            {
                // Inicializar el picker de personalizado con el rango actual
                DpDesde.SelectedDate = _desde.Date;
                DpHasta.SelectedDate = _hasta.Date;
            }
        }

        private void ActualizarBotonesPeriodo()
        {
            var activoBg = Application.Current.TryFindResource("BrushAccentMuted") as Brush
                           ?? new SolidColorBrush(Color.FromArgb(0x22, 0xF2, 0x64, 0x19));
            var inactivoBg = Application.Current.TryFindResource("BrushBgPrimary") as Brush
                             ?? new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x1A));
            var borde = Application.Current.TryFindResource("BrushBorder") as Brush
                        ?? new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
            var accentFg = Application.Current.TryFindResource("BrushAccent") as Brush ?? _brAccent;
            var mutedFg  = Application.Current.TryFindResource("BrushTextSecondary") as Brush ?? _brGris;

            var btns = new (string tag, Border btn)[]
            {
                ("hoy",          BtnHoy),
                ("semana",       BtnSemana),
                ("mes",          BtnMes),
                ("personalizado",BtnPersonalizado),
            };

            foreach (var (tag, btn) in btns)
            {
                bool activo = tag == _periodo;
                btn.Background      = activo ? activoBg : inactivoBg;
                btn.BorderBrush     = activo ? Brushes.Transparent : borde;
                btn.BorderThickness = activo ? new Thickness(0) : new Thickness(1);
                if (btn.Child is TextBlock tb)
                    tb.Foreground = activo ? accentFg : mutedFg;
            }
        }

        // ════════════════════════════════════════════════════════
        //  CARGA DE DATOS
        // ════════════════════════════════════════════════════════

        private void CargarDatos()
        {
            try
            {
                var metricas = _repo.ObtenerMetricas(_desde, _hasta);
                ActualizarMetricas(metricas);

                var ventas = _repo.ObtenerUltimasVentas(_desde, _hasta);
                ActualizarTransacciones(ventas);

                var topMem = _repo.ObtenerTopMembresias(_desde, _hasta);
                ActualizarTopMembresias(topMem);

                var topProd = _repo.ObtenerTopProductos(_desde, _hasta);
                ActualizarTopProductos(topProd);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando reportes:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ── Métricas ─────────────────────────────────────────────

        private void ActualizarMetricas(ReporteMetricas m)
        {
            TxtIngresos.Text    = $"${m.IngresosTotales:N0}";
            TxtVentasNum.Text   = m.VentasRegistradas.ToString("N0");
            TxtClientesNum.Text = m.ClientesNuevos.ToString();
            TxtRenovNum.Text    = m.Renovaciones.ToString();

            SetVariacion(TxtIngresosVar, m.IngresosTotales,          m.IngresosPrev);
            SetVariacion(TxtVentasVar,   (decimal)m.VentasRegistradas, (decimal)m.VentasPrev);
            SetVariacion(TxtClientesVar, (decimal)m.ClientesNuevos,    (decimal)m.ClientesPrev);
            SetVariacion(TxtRenovVar,    (decimal)m.Renovaciones,      (decimal)m.RenovacionesPrev);
        }

        private static void SetVariacion(TextBlock tb, decimal actual, decimal anterior)
        {
            if (anterior == 0 && actual == 0)
            {
                tb.Text = "Sin datos en el período";
                tb.Foreground = new SolidColorBrush(Color.FromArgb(0x88, 0x88, 0x88, 0x88));
                return;
            }
            if (anterior == 0)
            {
                tb.Text = "Nuevo en este período";
                tb.Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99));
                return;
            }
            decimal pct    = Math.Round((actual - anterior) / anterior * 100m, 1);
            string  flecha = pct >= 0 ? "↑" : "↓";
            tb.Text      = $"{flecha} {Math.Abs(pct)}% vs período anterior";
            tb.Foreground = pct >= 0
                ? new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99))
                : new SolidColorBrush(Color.FromRgb(0xF8, 0x71, 0x71));
        }

        // ── Transacciones ─────────────────────────────────────────

        private void ActualizarTransacciones(List<VentaDetalle> ventas)
        {
            ListaTransacciones.Children.Clear();
            if (ventas.Count == 0)
            {
                ListaTransacciones.Children.Add(Vacio("No hay transacciones en este período"));
                return;
            }
            foreach (var v in ventas)
                ListaTransacciones.Children.Add(CrearFilaVenta(v));
        }

        private UIElement CrearFilaVenta(VentaDetalle v)
        {
            var g = new Grid { Height = 46, Margin = new Thickness(0, 0, 0, 4) };
            g.ColumnDefinitions.Add(new ColumnDefinition());
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });

            var sp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            sp.Children.Add(new TextBlock
            {
                Text       = v.NombreCliente,
                FontSize   = 12.5,
                FontWeight = FontWeights.Medium,
                Foreground = R("BrushTextPrimary"),
            });
            sp.Children.Add(new TextBlock
            {
                Text       = v.Concepto,
                FontSize   = 11,
                Foreground = R("BrushTextSecondary"),
            });
            Grid.SetColumn(sp, 0);
            g.Children.Add(sp);

            var fecha = new TextBlock
            {
                Text                = v.Fecha.ToString("dd/MM/yy"),
                FontSize            = 11.5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center,
                Foreground          = R("BrushTextSecondary"),
            };
            Grid.SetColumn(fecha, 1);
            g.Children.Add(fecha);

            var monto = new TextBlock
            {
                Text                = $"${v.Monto:N0}",
                FontSize            = 13,
                FontWeight          = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment   = VerticalAlignment.Center,
                Foreground          = _brAccent,
            };
            Grid.SetColumn(monto, 2);
            g.Children.Add(monto);

            return g;
        }

        // ── Top membresías ────────────────────────────────────────

        private void ActualizarTopMembresias(List<TopMembresia> top)
        {
            ListaTopMembresias.Children.Clear();
            if (top.Count == 0)
            {
                ListaTopMembresias.Children.Add(Vacio("Sin datos en este período"));
                return;
            }

            int maxCant = top[0].Cantidad;

            foreach (var m in top)
            {
                var sp = new StackPanel { Margin = new Thickness(0, 0, 0, 14) };

                var dp = new DockPanel { Margin = new Thickness(0, 0, 0, 5) };
                var info = new TextBlock
                {
                    Text       = $"{m.Cantidad} ventas · ${m.Total:N0}",
                    FontSize   = 11.5,
                    Foreground = R("BrushTextSecondary"),
                };
                DockPanel.SetDock(info, Dock.Right);
                dp.Children.Add(info);
                dp.Children.Add(new TextBlock
                {
                    Text       = m.Nombre,
                    FontSize   = 12.5,
                    FontWeight = FontWeights.Medium,
                    Foreground = R("BrushTextPrimary"),
                });

                double ancho = maxCant > 0 ? (double)m.Cantidad / maxCant * 280 : 4;
                var outer = new Border
                {
                    Height       = 8,
                    CornerRadius = new CornerRadius(4),
                    Background   = R("BrushBgPrimary"),
                };
                outer.Child = new Border
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width               = ancho,
                    Height              = 8,
                    CornerRadius        = new CornerRadius(4),
                    Background          = GradAccent(),
                };

                sp.Children.Add(dp);
                sp.Children.Add(outer);
                ListaTopMembresias.Children.Add(sp);
            }
        }

        // ── Top productos ─────────────────────────────────────────

        private void ActualizarTopProductos(List<TopProducto> top)
        {
            ListaTopProductos.Children.Clear();
            if (top.Count == 0)
            {
                ListaTopProductos.Children.Add(Vacio("Sin datos en este período"));
                return;
            }

            foreach (var p in top)
            {
                var g = new Grid { Margin = new Thickness(0, 0, 0, 8) };
                g.ColumnDefinitions.Add(new ColumnDefinition());
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(55) });
                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(65) });

                g.Children.Add(new TextBlock
                {
                    Text              = p.Nombre,
                    FontSize          = 12,
                    Foreground        = R("BrushTextPrimary"),
                    VerticalAlignment = VerticalAlignment.Center,
                });

                var uds = new TextBlock
                {
                    Text                = $"{p.Cantidad} uds",
                    FontSize            = 11,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Foreground          = R("BrushTextSecondary"),
                };
                Grid.SetColumn(uds, 1);
                g.Children.Add(uds);

                var total = new TextBlock
                {
                    Text                = $"${p.Total:N0}",
                    FontSize            = 12,
                    FontWeight          = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment   = VerticalAlignment.Center,
                    Foreground          = _brAccent,
                };
                Grid.SetColumn(total, 2);
                g.Children.Add(total);

                ListaTopProductos.Children.Add(g);
            }
        }

        // ── Helpers ──────────────────────────────────────────────

        private static Brush R(string key)
            => Application.Current.TryFindResource(key) as Brush ?? Brushes.Gray;

        private static LinearGradientBrush GradAccent()
        {
            var gb = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint   = new Point(1, 0),
            };
            gb.GradientStops.Add(new GradientStop(Color.FromRgb(0xF2, 0x64, 0x19), 0));
            gb.GradientStops.Add(new GradientStop(Color.FromRgb(0xD9, 0x4F, 0x00), 1));
            return gb;
        }

        private static UIElement Vacio(string msg) => new TextBlock
        {
            Text                = msg,
            FontSize            = 12,
            Foreground          = new SolidColorBrush(Color.FromArgb(0x88, 0x88, 0x88, 0x88)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin              = new Thickness(0, 16, 0, 8),
        };
    }
}
