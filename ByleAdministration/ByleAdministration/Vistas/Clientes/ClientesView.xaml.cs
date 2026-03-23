using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ByleAdministration.Vistas.Clientes.Dialogs;
namespace ByleAdministration.Vistas.Clientes
{
    public partial class ClientesView : UserControl
    {
        private RepositorioCliente _repositorio = new RepositorioCliente();
        private List<Cliente> _todosClientes;
        private Cliente _clienteSeleccionado;

        public ClientesView()
        {
            InitializeComponent();
            CargarClientes();
        }

        private void CargarClientes()
        {
            try
            {
                _todosClientes = _repositorio.ObtenerTodos();
                MostrarClientes(_todosClientes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar clientes: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarClientes(List<Cliente> clientes)
        {
            PanelClientes.Children.Clear();
            TxtContador.Text = clientes.Count + " clientes";

            foreach (var cliente in clientes)
            {
                var fila = CrearFilaCliente(cliente);
                PanelClientes.Children.Add(fila);
            }

            // Seleccionar el primero si hay
            if (clientes.Count > 0 && _clienteSeleccionado == null)
            {
                SeleccionarCliente(clientes[0]);
            }
        }

        private Grid CrearFilaCliente(Cliente cliente)
        {
            var grid = new Grid
            {
                Height = 52,
                Margin = new Thickness(0, 0, 0, 1),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            // Fondo alternado
            int index = PanelClientes.Children.Count;
            if (index % 2 == 0)
                grid.SetResourceReference(Grid.BackgroundProperty, "BrushBgCardHover");

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(110) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });

            // Col 0: indicador + avatar + datos
            var panelInfo = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

            // Indicador lateral de color
            var indicador = new System.Windows.Shapes.Rectangle
            {
                Width = 3,
                RadiusX = 2,
                RadiusY = 2,
                Margin = new Thickness(0, 0, 8, 0),
                Fill = cliente.EstaActivo
                    ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x2E, 0xCC, 0x71))
                    : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xE7, 0x4C, 0x3C))
            };
            panelInfo.Children.Add(indicador);

            // Avatar
            var avatarBorder = new Border
            {
                Width = 34,
                Height = 34,
                CornerRadius = new CornerRadius(17),
                Margin = new Thickness(0, 0, 10, 0)
            };
            avatarBorder.SetResourceReference(Border.BackgroundProperty, "BrushAccentMuted");
            var avatarText = new TextBlock
            {
                Text = cliente.Iniciales,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatarText.SetResourceReference(TextBlock.ForegroundProperty, "BrushAccent");
            avatarBorder.Child = avatarText;
            panelInfo.Children.Add(avatarBorder);

            // Nombre y correo
            var datosPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var txtNombre = new TextBlock
            {
                Text = cliente.NombreCompleto,
                FontSize = 12,
                FontWeight = FontWeights.Medium
            };
            txtNombre.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextPrimary");
            datosPanel.Children.Add(txtNombre);

            var txtCorreo = new TextBlock { Text = cliente.Correo, FontSize = 10 };
            txtCorreo.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextMuted");
            datosPanel.Children.Add(txtCorreo);

            panelInfo.Children.Add(datosPanel);
            Grid.SetColumn(panelInfo, 0);
            grid.Children.Add(panelInfo);

            // Col 1: Teléfono
            var txtTel = new TextBlock
            {
                Text = cliente.TelefonoFormateado,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            };
            txtTel.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextSecondary");
            Grid.SetColumn(txtTel, 1);
            grid.Children.Add(txtTel);

            // Col 2: Membresía
            var txtMem = new TextBlock
            {
                Text = cliente.NombreMembresia,
                FontSize = 11,
                VerticalAlignment = VerticalAlignment.Center
            };
            txtMem.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextPrimary");
            Grid.SetColumn(txtMem, 2);
            grid.Children.Add(txtMem);

            // Col 3: Estado badge
            var estadoBorder = new Border
            {
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(6, 3, 6, 3),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            var estadoText = new TextBlock
            {
                Text = cliente.EstaActivo ? "ACTIVO" : "INACTIVO",
                FontSize = 9,
                FontWeight = FontWeights.Bold
            };

            if (cliente.EstaActivo)
            {
                estadoBorder.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(0xFF, 0x1A, 0x3D, 0x2B));
                estadoText.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x2E, 0xCC, 0x71));
            }
            else
            {
                estadoBorder.SetResourceReference(Border.BackgroundProperty, "BrushErrorMuted");
                estadoText.SetResourceReference(TextBlock.ForegroundProperty, "BrushError");
            }

            estadoBorder.Child = estadoText;
            Grid.SetColumn(estadoBorder, 3);
            grid.Children.Add(estadoBorder);

            // Click para seleccionar
            grid.MouseLeftButtonDown += (s, e) => SeleccionarCliente(cliente);

            return grid;
        }

        private void SeleccionarCliente(Cliente cliente)
        {
            _clienteSeleccionado = cliente;

            TxtNombrePerfil.Text = cliente.NombreCompleto;
            TxtInicialesPerfil.Text = cliente.Iniciales;
            TxtTelefonoPerfil.Text = cliente.TelefonoFormateado;
            TxtCorreoPerfil.Text = cliente.Correo;
            TxtMembresiaPerfil.Text = cliente.NombreMembresia;
            TxtEdadPerfil.Text = cliente.Edad.ToString() + " años";
            TxtCiudadPerfil.Text = cliente.Ciudad;
            TxtInscripcionPerfil.Text = cliente.FechaInscripcion.ToString("dd/MM/yyyy");
            TxtRenovacionPerfil.Text = cliente.FechaRenovacion?.ToString("dd/MM/yyyy") ?? "—";

            // Estado
            if (cliente.EstaActivo)
            {
                TxtEstadoPerfil.Text = "● Membresía activa";
                TxtEstadoPerfil.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x2E, 0xCC, 0x71));
                BorderEstadoPerfil.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(0xFF, 0x1A, 0x3D, 0x2B));
            }
            else
            {
                TxtEstadoPerfil.Text = "● Membresía inactiva";
                TxtEstadoPerfil.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xE7, 0x4C, 0x3C));
                BorderEstadoPerfil.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(0xFF, 0x3D, 0x1A, 0x1A));
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            string termino = TxtBuscar.Text.Trim();

            if (string.IsNullOrEmpty(termino))
            {
                MostrarClientes(_todosClientes);
            }
            else
            {
                // Filtrar local para no bombardear la BD en cada tecla
                var filtrados = _todosClientes
                    .Where(c => c.NombreCompleto.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0
                             || c.Correo.IndexOf(termino, StringComparison.OrdinalIgnoreCase) >= 0
                             || c.Telefono.ToString().Contains(termino))
                    .ToList();
                MostrarClientes(filtrados);
            }
        }

        private void BtnLimpiarBusqueda_Click(object sender, RoutedEventArgs e)
        {
            TxtBuscar.Text = "";
        }

        private void CmbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_todosClientes == null) return;

            var item = CmbFiltroEstado.SelectedItem as ComboBoxItem;
            string filtro = item?.Content?.ToString() ?? "Todos";

            List<Cliente> filtrados;
            switch (filtro)
            {
                case "Activos":
                    filtrados = _todosClientes.Where(c => c.EstaActivo).ToList();
                    break;
                case "Inactivos":
                    filtrados = _todosClientes.Where(c => !c.EstaActivo).ToList();
                    break;
                default:
                    filtrados = _todosClientes;
                    break;
            }
            MostrarClientes(filtrados);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            EditarClienteDialog dialog = new EditarClienteDialog();
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }
    }
}