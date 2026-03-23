using ByleAdministration.Vistas.Acceso;
using ByleAdministration.Vistas.Clases;
using ByleAdministration.Vistas.Clientes;
using ByleAdministration.Vistas.Dashboard;
using ByleAdministration.Vistas.Membresias;
using ByleAdministration.Vistas.Reportes;
using ByleAdministration.Vistas.Sistema;
using ByleAdministration.Vistas.Ventas;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ByleAdministration
{
    public partial class MainWindow : Window
    {
        private bool _modoClaro = false;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                MessageBox.Show("InitializeComponent OK");
                MainContent.Content = new AccesoView();
                MessageBox.Show("AccesoView cargada OK");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en MainWindow: " + ex.ToString());
            }
        }

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
           
            
            
                
                if (!IsLoaded) return;  

                if (!(sender is RadioButton btn)) return;
                string seccion = btn.Name.Replace("Nav", "");
               

                TxtSeccionActual.Text = seccion switch
                {
                    "Dashboard" => "Dashboard",
                    "Acceso" => "Acceso",
                    "Ventas" => "Ventas / Caja",
                    "Clientes" => "Clientes",
                    "Membresias" => "Membresías",
                    "Clases" => "Clases y Progreso",
                    "Reportes" => "Reportes",
                    "Sistema" => "Sistema",
                    _ => "Acceso"
                };

                MainContent.Content = seccion switch
                {

                    "Dashboard" => new DashboardView(),
                    "Acceso" => new AccesoView(),
                    "Ventas" => new VentasView(),
                    "Clientes" => new ClientesView(),
                    "Membresias" => new MembresiasView(),
                    "Clases" => new ClasesView(),
                    "Reportes" => new ReportesView(),
                    "Sistema" => new SistemaView(),
                    _ => new AccesoView()
                };
            
            
            

            
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            _modoClaro = !_modoClaro;

            var dicts = Application.Current.Resources.MergedDictionaries;
            for (int i = dicts.Count - 1; i >= 0; i--)
            {
                string src = dicts[i].Source?.ToString() ?? "";
                if (src.Contains("Tema")) { dicts.RemoveAt(i); break; }
            }

            string tema = _modoClaro
                ? "Recursos/Estilos/TemaClaro.xaml"
                : "Recursos/Estilos/TemaOscuro.xaml";

            dicts.Insert(0, new ResourceDictionary
            {
                Source = new Uri(tema, UriKind.Relative)
            });

            ThemeIcon.Text = _modoClaro ? "🌙" : "☀";
            ThemeLabel.Text = _modoClaro ? "Modo oscuro" : "Modo claro";
        }

        private void pnlControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                WindowState = WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
            else
                DragMove();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;
    
       
    }
}