using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using ByleAdministration.Vistas.Clientes.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ByleAdministration.Vistas.Clientes
{
    public partial class ClientesView : UserControl
    {
        private readonly RepositorioCliente _repositorio = new RepositorioCliente();
        private readonly RepositorioBiometria _repoBio = new RepositorioBiometria();
        private List<Cliente> _todosClientes;
        private Cliente _clienteSeleccionado;

        // ── Pinceles reutilizables ──────────────────────────────────
        private static readonly SolidColorBrush _brushVerde =
            new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x71));
        private static readonly SolidColorBrush _brushRojo =
            new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C));
        private static readonly SolidColorBrush _brushFondoVerde =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0x3D, 0x2B));
        private static readonly SolidColorBrush _brushFondoRojo =
            new SolidColorBrush(Color.FromArgb(0xFF, 0x3D, 0x1A, 0x1A));

        public ClientesView()
        {
            InitializeComponent();
            Loaded += (s, e) => CargarClientes();
        }

        // ══════════════════════════════════════════════════════════
        // CARGA Y VISUALIZACIÓN
        // ══════════════════════════════════════════════════════════

        private void CargarClientes()
        {
            try
            {
                _todosClientes = _repositorio.ObtenerTodos();
                AplicarFiltro();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltro()
        {
            if (_todosClientes == null) return;

            // Filtro por estado
            List<Cliente> filtrados;
            if (RbActivos?.IsChecked == true)
                filtrados = _todosClientes.Where(c => c.EstaActivo).ToList();
            else if (RbInactivos?.IsChecked == true)
                filtrados = _todosClientes.Where(c => !c.EstaActivo).ToList();
            else
                filtrados = _todosClientes;

            // Filtro por búsqueda
            string termino = TxtBuscar?.Text?.Trim() ?? "";
            if (!string.IsNullOrEmpty(termino))
                filtrados = filtrados
                    .Where(c =>
                        c.NombreCompleto.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        c.Correo.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        c.Telefono.ToString().Contains(termino))
                    .ToList();

            MostrarClientes(filtrados);
        }

        private void MostrarClientes(List<Cliente> clientes)
        {
            PanelClientes.Children.Clear();
            TxtContador.Text = clientes.Count == 1 ? "1 cliente" : $"{clientes.Count} clientes";

            foreach (var c in clientes)
                PanelClientes.Children.Add(CrearFilaCliente(c));

            // Reseleccionar el mismo cliente si sigue en la lista
            if (_clienteSeleccionado != null)
            {
                var mismo = clientes.FirstOrDefault(c => c.IdUsuario == _clienteSeleccionado.IdUsuario);
                if (mismo != null) { SeleccionarCliente(mismo); return; }
            }

            if (clientes.Count > 0)
                SeleccionarCliente(clientes[0]);
        }

        private Grid CrearFilaCliente(Cliente cliente)
        {
            var grid = new Grid
            {
                Height = 54,
                Margin = new Thickness(0, 0, 0, 1),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Fondo alternado
            int idx = PanelClientes.Children.Count;
            if (idx % 2 == 0)
                grid.SetResourceReference(Grid.BackgroundProperty, "BrushBgCardHover");

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });

            // Col 0: indicador + avatar + nombre + correo
            var panelInfo = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };

            var indicador = new System.Windows.Shapes.Rectangle
            {
                Width = 3,
                Height = 28,
                RadiusX = 2,
                RadiusY = 2,
                Margin = new Thickness(0, 0, 10, 0),
                Fill = cliente.EstaActivo ? _brushVerde : _brushRojo
            };
            panelInfo.Children.Add(indicador);

            var avatar = new Border
            {
                Width = 36,
                Height = 36,
                CornerRadius = new CornerRadius(18),
                Margin = new Thickness(0, 0, 10, 0)
            };
            avatar.SetResourceReference(Border.BackgroundProperty, "BrushAccentMuted");
            var avatarTxt = new TextBlock
            {
                Text = cliente.Iniciales,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatarTxt.SetResourceReference(TextBlock.ForegroundProperty, "BrushAccent");
            avatar.Child = avatarTxt;
            panelInfo.Children.Add(avatar);

            var datosStack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var nombre = new TextBlock
            {
                Text = cliente.NombreCompleto,
                FontSize = 12.5,
                FontWeight = FontWeights.Medium
            };
            nombre.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextPrimary");
            datosStack.Children.Add(nombre);
            var correo = new TextBlock { Text = cliente.Correo, FontSize = 10.5 };
            correo.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextMuted");
            datosStack.Children.Add(correo);
            panelInfo.Children.Add(datosStack);

            Grid.SetColumn(panelInfo, 0);
            grid.Children.Add(panelInfo);

            // Col 1: Teléfono
            var tel = new TextBlock
            {
                Text = cliente.TelefonoFormateado,
                FontSize = 11.5,
                VerticalAlignment = VerticalAlignment.Center
            };
            tel.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextSecondary");
            Grid.SetColumn(tel, 1);
            grid.Children.Add(tel);

            // Col 2: Membresía
            var mem = new TextBlock
            {
                Text = cliente.NombreMembresia,
                FontSize = 11.5,
                VerticalAlignment = VerticalAlignment.Center
            };
            mem.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextPrimary");
            Grid.SetColumn(mem, 2);
            grid.Children.Add(mem);

            // Col 3: Badge estado
            var badge = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 3, 6, 3),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var badgeTxt = new TextBlock
            {
                Text = cliente.EstaActivo ? "ACTIVO" : "INACTIVO",
                FontSize = 9,
                FontWeight = FontWeights.Bold
            };
            if (cliente.EstaActivo)
            {
                badge.Background = _brushFondoVerde;
                badgeTxt.Foreground = _brushVerde;
            }
            else
            {
                badge.Background = _brushFondoRojo;
                badgeTxt.Foreground = _brushRojo;
            }
            badge.Child = badgeTxt;
            Grid.SetColumn(badge, 3);
            grid.Children.Add(badge);

            grid.MouseLeftButtonDown += (s, e) => SeleccionarCliente(cliente);
            return grid;
        }

        private void SeleccionarCliente(Cliente c)
        {
            _clienteSeleccionado = c;

            TxtInicialesPerfil.Text = c.Iniciales;
            TxtNombrePerfil.Text = c.NombreCompleto;
            TxtCorreoPerfil.Text = c.Correo;
            TxtTelefonoPerfil.Text = c.TelefonoFormateado;
            TxtMembresiaPerfil.Text = c.NombreMembresia;
            TxtEdadPerfil.Text = c.Edad + " años";
            TxtCiudadPerfil.Text = c.Ciudad;
            TxtInscripcionPerfil.Text = c.FechaInscripcion.ToString("dd/MM/yyyy");
            TxtRenovacionPerfil.Text = c.FechaRenovacion?.ToString("dd/MM/yyyy") ?? "—";

            // Huella
            bool tieneHuella = _repoBio.ExisteEnrolamiento(c.IdUsuario);
            TxtHuellaPerfil.Text = tieneHuella ? "Registrada" : "Sin huella";
            TxtHuellaPerfil.Foreground = tieneHuella ? _brushVerde : _brushRojo;
            IconHuellaPerfil.Foreground = tieneHuella ? _brushVerde : _brushRojo;

            // Estado badge
            if (c.EstaActivo)
            {
                TxtEstadoPerfil.Text = "Membresía activa";
                TxtEstadoPerfil.Foreground = _brushVerde;
                BorderEstadoPerfil.Background = _brushFondoVerde;
                // Ellipse dentro del badge
                if (BorderEstadoPerfil.Child is StackPanel sp && sp.Children.Count > 0
                    && sp.Children[0] is System.Windows.Shapes.Ellipse dot)
                    dot.Fill = _brushVerde;
            }
            else
            {
                TxtEstadoPerfil.Text = "Membresía inactiva";
                TxtEstadoPerfil.Foreground = _brushRojo;
                BorderEstadoPerfil.Background = _brushFondoRojo;
                if (BorderEstadoPerfil.Child is StackPanel sp && sp.Children.Count > 0
                    && sp.Children[0] is System.Windows.Shapes.Ellipse dot)
                    dot.Fill = _brushRojo;
            }
        }

        // ══════════════════════════════════════════════════════════
        // NUEVO CLIENTE
        // ══════════════════════════════════════════════════════════

        private void BtnNuevoCliente_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EditarClienteDialog();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                _clienteSeleccionado = null;
                CargarClientes();
            }
        }

        // ══════════════════════════════════════════════════════════
        // EDITAR CLIENTE
        // ══════════════════════════════════════════════════════════

        private void BtnEditarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteSeleccionado == null)
            {
                MessageBox.Show("Selecciona un cliente para editar.",
                    "Sin selección", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new EditarClienteDialog(_clienteSeleccionado.IdUsuario);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
                CargarClientes();
        }

        // ══════════════════════════════════════════════════════════
        // BÚSQUEDA Y FILTROS
        // ══════════════════════════════════════════════════════════

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
            => AplicarFiltro();

        private void BtnLimpiarBusqueda_Click(object sender, RoutedEventArgs e)
            => TxtBuscar.Text = "";

        private void RbFiltro_Checked(object sender, RoutedEventArgs e)
            => AplicarFiltro();
    }
}