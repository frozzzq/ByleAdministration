using ByleAdministration.Servicios;
using ByleAdministration.Vistas.Auth;
using System.Windows;
using System.Windows.Navigation;

namespace ByleAdministration.Vistas.Auth
{
    public partial class LoginWindow : Window
    {
        private ServicioEmpleado _servicio = new ServicioEmpleado();

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnIngresar_Click(object sender, RoutedEventArgs e)
        {
            string correo = TxtCorreo.Text.Trim();
            string contrasena = PbxContrasena.Password;

            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
            {
                MessageBox.Show("Completa todos los campos.", "Aviso",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool exitoso = _servicio.Login(correo, contrasena);

            if (exitoso)
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();  // ← cierra el login
            }
            else
            {
                MessageBox.Show("Correo o contraseña incorrectos.", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LnkRegistro_Click(object sender, RoutedEventArgs e)
        {
            RegistroEmpleadoWindow registro = new RegistroEmpleadoWindow();
            registro.ShowDialog();
        }
    }
}