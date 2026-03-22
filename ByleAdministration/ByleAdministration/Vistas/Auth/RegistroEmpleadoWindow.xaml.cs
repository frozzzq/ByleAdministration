using ByleAdministration.Modelos;
using ByleAdministration.ModelosVistas;
using ByleAdministration.Servicios;
using System.Windows;

namespace ByleAdministration.Vistas.Auth
{
    public partial class RegistroEmpleadoWindow : Window
    {
        private ServicioEmpleado _servicio = new ServicioEmpleado();

        public RegistroEmpleadoWindow()
        {
            InitializeComponent();
            DataContext = new NuevoEmpleadoVM();  // ← faltaba el punto y coma
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtNombre.Text) ||
                string.IsNullOrEmpty(TxtCorreo.Text) ||
                string.IsNullOrEmpty(TxtRfc.Text) ||
                string.IsNullOrEmpty(PbxContrasena.Password) ||
                CmbRol.SelectedItem == null)
            {
                MessageBox.Show("Completa todos los campos.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var vm = (NuevoEmpleadoVM)DataContext;

            Empleados nuevo = new Empleados
            {
                NombreCompleto = TxtNombre.Text.Trim(),
                Correo = TxtCorreo.Text.Trim(),
                Telefono = TxtTelefono.Text.Trim(),
                Rfc = TxtRfc.Text.Trim().ToUpper(),
                ContraseñaHash = PbxContrasena.Password,
                IdRol = vm.RolSeleccionado.IdRol
            };

            bool exitoso = _servicio.Registrar(nuevo);
            if (exitoso)
            {
                MessageBox.Show("Empleado registrado correctamente.", "Éxito",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("El correo ya está registrado.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
            => this.Close();
        private void CmbRol_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }
    }
}