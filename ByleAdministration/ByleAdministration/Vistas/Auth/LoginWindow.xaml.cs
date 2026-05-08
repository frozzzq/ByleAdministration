using ByleAdministration.Servicios;
using ByleAdministration.Vistas.Auth;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ByleAdministration.Vistas.Auth
{
    public partial class LoginWindow : Window
    {
        private readonly ServicioEmpleado _servicio = new ServicioEmpleado();

        public LoginWindow()
        {
            InitializeComponent();
        }

        // ── Dragging (ventana sin borde) ──────────────────────────
        private void LogoBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        // ── Cierre de ventana ─────────────────────────────────────
        private void BtnCerrarLogin_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ── Focus visual en contenedores de campo ─────────────────
        //    Cuando el TextBox / PasswordBox interno recibe focus,
        //    el borde del contenedor cambia al color de acento.
        private void InputContainer_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Border b)
                b.BorderBrush = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xF2, 0x64, 0x19)); // #F26419
        }

        private void InputContainer_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Border b)
                b.BorderBrush = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x2A, 0x2A, 0x4A)); // #2A2A4A
        }

        // ── Login ─────────────────────────────────────────────────
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
                new MainWindow().Show();
                Close();
            }
            else
            {
                MessageBox.Show("Correo o contraseña incorrectos.", "Error de acceso",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                PbxContrasena.Clear();
                TxtCorreo.Focus();
            }
        }

        // ── Registro ──────────────────────────────────────────────
        private void LnkRegistro_Click(object sender, RoutedEventArgs e)
        {
            new RegistroEmpleadoWindow().ShowDialog();
        }
    }
}
