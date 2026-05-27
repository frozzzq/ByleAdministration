using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ByleAdministration.Vistas.Membresias.Dialogs
{
    public partial class EditarMembresiaDialog : Window
    {
        private readonly Membresia _original;
        private readonly RepositorioMembresia _repo = new RepositorioMembresia();

        public EditarMembresiaDialog(Membresia membresia = null)
        {
            InitializeComponent();
            _original = membresia;

            if (membresia != null)
            {
                TxtTitulo.Text      = "Editar plan";
                TxtNombre.Text      = membresia.NombreMembresia;
                TxtPrecio.Text      = membresia.Precio.ToString("F2");
                TxtDias.Text        = membresia.DuracionDias.ToString();
                TxtDescripcion.Text = membresia.Descripcion ?? "";

                foreach (ComboBoxItem item in CmbEstado.Items)
                    if (item.Content?.ToString() == membresia.Estado)
                    { CmbEstado.SelectedItem = item; break; }
            }
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNombre.Focus();
                return;
            }

            if (!decimal.TryParse(TxtPrecio.Text.Trim(), out decimal precio) || precio <= 0)
            {
                MessageBox.Show("Ingresa un precio válido mayor a 0.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPrecio.Focus();
                return;
            }

            if (!int.TryParse(TxtDias.Text.Trim(), out int dias) || dias <= 0)
            {
                MessageBox.Show("Ingresa una duración válida (número de días).", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDias.Focus();
                return;
            }

            string estado = ((ComboBoxItem)CmbEstado.SelectedItem)?.Content?.ToString() ?? "activa";

            var m = new Membresia
            {
                NombreMembresia = TxtNombre.Text.Trim(),
                Precio          = precio,
                DuracionDias    = dias,
                Descripcion     = string.IsNullOrWhiteSpace(TxtDescripcion.Text)
                                  ? null : TxtDescripcion.Text.Trim(),
                Estado          = estado
            };

            try
            {
                if (_original == null)
                    _repo.Insertar(m);
                else
                {
                    m.IdMembresia = _original.IdMembresia;
                    _repo.Actualizar(m);
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        { DialogResult = false; Close(); }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        { DialogResult = false; Close(); }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); }
    }
}
