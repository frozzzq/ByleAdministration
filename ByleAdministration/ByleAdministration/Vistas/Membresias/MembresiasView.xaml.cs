using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using ByleAdministration.Vistas.Membresias.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ByleAdministration.Vistas.Membresias
{
    public partial class MembresiasView : UserControl
    {
        private readonly RepositorioMembresia _repo = new RepositorioMembresia();
        private List<Membresia> _todas = new List<Membresia>();
        private string _filtro = "todos";

        public MembresiasView()
        {
            InitializeComponent();
            Loaded += (s, e) => CargarMembresias();
        }

        // ════════════════════════════════════════════════════════
        //  CARGA Y FILTRADO
        // ════════════════════════════════════════════════════════

        private void CargarMembresias()
        {
            try
            {
                _todas = _repo.ObtenerTodasConConteo();
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando membresías:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AplicarFiltros()
        {
            string busqueda = TxtBuscar.Text.Trim().ToLower();

            IEnumerable<Membresia> filtradas = _todas;

            if (_filtro == "activos")
                filtradas = filtradas.Where(m => m.Estado != "inactiva");
            else if (_filtro == "inactivos")
                filtradas = filtradas.Where(m => m.Estado == "inactiva");

            if (!string.IsNullOrEmpty(busqueda))
                filtradas = filtradas.Where(m => m.NombreMembresia.ToLower().Contains(busqueda));

            PanelMembresias.Children.Clear();

            var lista = filtradas.ToList();
            if (lista.Count == 0)
            {
                PanelMembresias.Children.Add(new TextBlock
                {
                    Text       = "No se encontraron membresías.",
                    FontSize   = 13,
                    Foreground = new SolidColorBrush(Color.FromArgb(0x88, 0x88, 0x88, 0x88)),
                    Margin     = new Thickness(0, 20, 0, 0)
                });
                return;
            }

            foreach (var m in lista)
                PanelMembresias.Children.Add(CrearTarjeta(m));
        }

        // ════════════════════════════════════════════════════════
        //  CONSTRUCCIÓN DE TARJETA
        // ════════════════════════════════════════════════════════

        private UIElement CrearTarjeta(Membresia m)
        {
            bool esInactiva = m.Estado == "inactiva";

            var card = new Border
            {
                Width           = 230,
                Margin          = new Thickness(0, 0, 16, 16),
                CornerRadius    = new CornerRadius(14),
                Padding         = new Thickness(22, 20, 22, 20),
                Background      = R("BrushBgCard"),
                BorderThickness = new Thickness(1),
                BorderBrush     = R("BrushBorder"),
                Opacity         = esInactiva ? 0.6 : 1.0
            };

            var sp = new StackPanel();
            card.Child = sp;

            // ── Ícono (letra inicial del plan) ──
            Color iconColor = GetIconColor(m.Estado);
            Color iconBgColor = Color.FromArgb(0x22, iconColor.R, iconColor.G, iconColor.B);

            var iconBorder = new Border
            {
                Width               = 44,
                Height              = 44,
                CornerRadius        = new CornerRadius(12),
                Background          = new SolidColorBrush(iconBgColor),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin              = new Thickness(0, 0, 0, 14)
            };
            iconBorder.Child = new TextBlock
            {
                Text                = m.NombreMembresia.Length > 0
                                      ? m.NombreMembresia[0].ToString().ToUpper() : "M",
                FontSize            = 18,
                FontWeight          = FontWeights.Bold,
                Foreground          = new SolidColorBrush(iconColor),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center
            };
            sp.Children.Add(iconBorder);

            // ── Nombre ──
            sp.Children.Add(new TextBlock
            {
                Text         = m.NombreMembresia,
                FontSize     = 17,
                FontWeight   = FontWeights.Bold,
                Foreground   = R("BrushTextPrimary"),
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            // ── Precio ──
            var precioSp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin      = new Thickness(0, 4, 0, 12)
            };
            precioSp.Children.Add(new TextBlock
            {
                Text       = "$",
                FontSize   = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(iconColor),
                Margin     = new Thickness(0, 6, 2, 0)
            });
            precioSp.Children.Add(new TextBlock
            {
                Text       = m.Precio.ToString("N0"),
                FontSize   = 32,
                FontWeight = FontWeights.Black,
                Foreground = new SolidColorBrush(iconColor)
            });
            precioSp.Children.Add(new TextBlock
            {
                Text       = FormatDuracion(m.DuracionDias),
                FontSize   = 12,
                Foreground = R("BrushTextMuted"),
                Margin     = new Thickness(4, 18, 0, 0)
            });
            sp.Children.Add(precioSp);

            sp.Children.Add(Divider());

            // ── Descripción / duración ──
            string desc = !string.IsNullOrWhiteSpace(m.Descripcion)
                ? m.Descripcion
                : $"{m.DuracionDias} días de acceso";
            sp.Children.Add(new TextBlock
            {
                Text         = desc,
                FontSize     = 11.5,
                Foreground   = R("BrushTextSecondary"),
                TextWrapping = TextWrapping.Wrap,
                Margin       = new Thickness(0, 0, 0, 10),
                MaxHeight    = 52
            });

            sp.Children.Add(Divider());

            // ── Badge estado + clientes ──
            Color badgeFg = GetBadgeFg(m.Estado);
            Color badgeBg = Color.FromArgb(0x22, badgeFg.R, badgeFg.G, badgeFg.B);

            var dpEstado = new DockPanel { Margin = new Thickness(0, 0, 0, 12) };
            var badge = new Border
            {
                Background   = new SolidColorBrush(badgeBg),
                CornerRadius = new CornerRadius(6),
                Padding      = new Thickness(8, 4, 8, 4)
            };
            badge.Child = new TextBlock
            {
                Text       = $"● {m.Estado.ToUpper()}",
                FontSize   = 10,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(badgeFg)
            };
            DockPanel.SetDock(badge, Dock.Left);
            dpEstado.Children.Add(badge);
            dpEstado.Children.Add(new TextBlock
            {
                Text                = $"{m.NumClientes} cliente{(m.NumClientes == 1 ? "" : "s")}",
                FontSize            = 11,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment   = VerticalAlignment.Center,
                Foreground          = R("BrushTextMuted")
            });
            sp.Children.Add(dpEstado);

            // ── Botones: Editar | Desactivar/Activar ──
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var btnEditar = new Button
            {
                Style               = Application.Current.TryFindResource("BtnSecundario") as Style,
                Content             = "Editar",
                Padding             = new Thickness(0, 9, 0, 9),
                FontSize            = 12,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Tag                 = m
            };
            btnEditar.Click += BtnEditar_Click;
            Grid.SetColumn(btnEditar, 0);
            grid.Children.Add(btnEditar);

            bool puedeDes = m.Estado != "inactiva";
            var btnToggle = new Button
            {
                Style               = puedeDes
                                      ? Application.Current.TryFindResource("BtnSecundario") as Style
                                      : Application.Current.TryFindResource("BtnPrimario") as Style,
                Content             = puedeDes ? "Desactivar" : "Activar",
                Padding             = new Thickness(0, 9, 0, 9),
                FontSize            = 12,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Tag                 = m
            };
            btnToggle.Click += BtnToggleEstado_Click;
            Grid.SetColumn(btnToggle, 2);
            grid.Children.Add(btnToggle);

            sp.Children.Add(grid);

            return card;
        }

        // ════════════════════════════════════════════════════════
        //  EVENT HANDLERS
        // ════════════════════════════════════════════════════════

        private void BtnNuevoPlan_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new EditarMembresiaDialog { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true)
                CargarMembresias();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button)?.Tag is Membresia m)) return;
            var dlg = new EditarMembresiaDialog(m) { Owner = Window.GetWindow(this) };
            if (dlg.ShowDialog() == true)
                CargarMembresias();
        }

        private void BtnToggleEstado_Click(object sender, RoutedEventArgs e)
        {
            if (!((sender as Button)?.Tag is Membresia m)) return;

            string nuevoEstado = m.Estado == "inactiva" ? "activa" : "inactiva";
            string accion      = nuevoEstado == "inactiva" ? "desactivar" : "activar";

            var resp = MessageBox.Show(
                $"¿Seguro que deseas {accion} \"{m.NombreMembresia}\"?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resp != MessageBoxResult.Yes) return;

            try
            {
                _repo.CambiarEstado(m.IdMembresia, nuevoEstado);
                CargarMembresias();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar estado:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFiltro_Click(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is Border b)) return;
            _filtro = b.Tag?.ToString() ?? "todos";
            ActualizarBotonesFiltro();
            AplicarFiltros();
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
            => AplicarFiltros();

        // ════════════════════════════════════════════════════════
        //  HELPERS
        // ════════════════════════════════════════════════════════

        private void ActualizarBotonesFiltro()
        {
            var activoBg  = Application.Current.TryFindResource("BrushAccentMuted") as Brush
                            ?? new SolidColorBrush(Color.FromArgb(0x22, 0xF2, 0x64, 0x19));
            var inactivoBg = Application.Current.TryFindResource("BrushBgCard") as Brush
                             ?? new SolidColorBrush(Color.FromRgb(0x16, 0x16, 0x28));
            var borde      = Application.Current.TryFindResource("BrushBorder") as Brush
                             ?? new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
            var accentFg   = Application.Current.TryFindResource("BrushAccent") as Brush
                             ?? new SolidColorBrush(Color.FromRgb(0xF2, 0x64, 0x19));
            var mutedFg    = Application.Current.TryFindResource("BrushTextSecondary") as Brush
                             ?? new SolidColorBrush(Color.FromArgb(0x88, 0x88, 0x88, 0x88));

            var btns = new (string tag, Border btn)[]
            {
                ("todos",     BtnFiltroTodos),
                ("activos",   BtnFiltroActivos),
                ("inactivos", BtnFiltroInactivos),
            };

            foreach (var (tag, btn) in btns)
            {
                bool activo = tag == _filtro;
                btn.Background      = activo ? activoBg : inactivoBg;
                btn.BorderBrush     = activo ? Brushes.Transparent : borde;
                btn.BorderThickness = activo ? new Thickness(0) : new Thickness(1);
                if (btn.Child is TextBlock tb)
                    tb.Foreground = activo ? accentFg : mutedFg;
            }
        }

        private static Brush R(string key)
            => Application.Current.TryFindResource(key) as Brush ?? Brushes.Gray;

        private static UIElement Divider()
        {
            var r  = new Rectangle { Height = 1, Margin = new Thickness(0, 0, 0, 12) };
            var gb = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint   = new System.Windows.Point(1, 0)
            };
            gb.GradientStops.Add(new GradientStop(Colors.Transparent, 0));
            gb.GradientStops.Add(new GradientStop(Color.FromArgb(0x44, 0xFF, 0xFF, 0xFF), 0.5));
            gb.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
            r.Fill = gb;
            return r;
        }

        private static Color GetIconColor(string estado)
        {
            switch (estado)
            {
                case "oferta":    return Color.FromRgb(0xFB, 0xBF, 0x24);
                case "temporada": return Color.FromRgb(0x34, 0xD3, 0x99);
                case "inactiva":  return Color.FromRgb(0x88, 0x88, 0x88);
                default:          return Color.FromRgb(0xF2, 0x64, 0x19); // activa
            }
        }

        private static Color GetBadgeFg(string estado)
        {
            switch (estado)
            {
                case "activa":    return Color.FromRgb(0x34, 0xD3, 0x99);
                case "oferta":    return Color.FromRgb(0xFB, 0xBF, 0x24);
                case "temporada": return Color.FromRgb(0x34, 0xD3, 0x99);
                default:          return Color.FromRgb(0x88, 0x88, 0x88); // inactiva
            }
        }

        private static string FormatDuracion(int dias)
        {
            if (dias == 30)  return "/mes";
            if (dias == 60)  return "/2 meses";
            if (dias == 90)  return "/3 meses";
            if (dias == 180) return "/6 meses";
            if (dias == 365) return "/año";
            return $"/{dias} días";
        }
    }
}
