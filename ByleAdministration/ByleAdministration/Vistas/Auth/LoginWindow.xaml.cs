using ByleAdministration.Servicios;
using ByleAdministration.Vistas.Auth;
using System.Windows;

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
<<<<<<< HEAD
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
=======
                MainWindow dashboard = new MainWindow();
                dashboard.Show();
                this.Close();  
>>>>>>> b06ef71253c07dea675ab2a00ddfcf1bd2eb3839
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

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();
    }
}