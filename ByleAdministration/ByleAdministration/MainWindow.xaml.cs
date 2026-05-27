using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using ByleAdministration.Vistas.Acceso;
using ByleAdministration.Vistas.Clases;
using ByleAdministration.Vistas.Clientes;
using ByleAdministration.Vistas.Clientes.Dialogs;
using ByleAdministration.Vistas.Dashboard;
using ByleAdministration.Vistas.Membresias;
using ByleAdministration.Vistas.Reportes;
using ByleAdministration.Vistas.Sistema;
using ByleAdministration.Vistas.Ventas;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ByleAdministration
{
    public partial class MainWindow : Window
    {
        private bool _modoClaro = false;

        // ── Pre-registros ────────────────────────────────────────
        private readonly RepositorioPreRegistro _repoPreReg = new RepositorioPreRegistro();
        private DispatcherTimer _timerNotif;
        private List<PreRegistro> _preRegistrosCached = new List<PreRegistro>();
        private readonly List<DispatcherTimer> _cardTimers = new List<DispatcherTimer>();

        // ── Pinceles reutilizables ───────────────────────────────
        private static readonly SolidColorBrush _brushAccent =
            new SolidColorBrush(Color.FromRgb(0xF2, 0x64, 0x19));
        private static readonly SolidColorBrush _brushWhite =
            new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush _brushCardBg =
            new SolidColorBrush(Color.FromArgb(0x22, 0xFF, 0xFF, 0xFF));
        private static readonly SolidColorBrush _brushCardBorder =
            new SolidColorBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));
        private static readonly SolidColorBrush _brushMuted =
            new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                MainContent.Content = new AccesoView();

                // Iniciar polling de pre-registros cada 5 segundos
                _timerNotif = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                _timerNotif.Tick += (s, e) => PollearPreRegistros();
                _timerNotif.Start();
                PollearPreRegistros();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en MainWindow: " + ex.ToString());
            }
        }

        // ════════════════════════════════════════════════════════
        //  PRE-REGISTROS — polling y tarjetas
        // ════════════════════════════════════════════════════════

        private void PollearPreRegistros()
        {
            try
            {
                _repoPreReg.ExpirarVencidos();
                _preRegistrosCached = _repoPreReg.ObtenerPendientes();
                ActualizarBadge(_preRegistrosCached.Count);
                if (NotifPopup.IsOpen)
                    ReconstruirTarjetas();
            }
            catch { /* DB no disponible — ignorar silenciosamente */ }
        }

        private void ActualizarBadge(int count)
        {
            if (count > 0)
            {
                BellBadge.Visibility = Visibility.Visible;
                BellBadgeCount.Text  = count > 9 ? "9+" : count.ToString();
            }
            else
            {
                BellBadge.Visibility = Visibility.Collapsed;
            }
        }

        private void Bell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ReconstruirTarjetas();
            NotifPopup.IsOpen = !NotifPopup.IsOpen;
        }

        private void ReconstruirTarjetas()
        {
            // Detener timers de tarjetas anteriores
            foreach (var t in _cardTimers) t.Stop();
            _cardTimers.Clear();
            NotifList.Children.Clear();

            NotifEmpty.Visibility = _preRegistrosCached.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;

            foreach (var pr in _preRegistrosCached)
            {
                DispatcherTimer ct;
                var card = CrearTarjetaPreReg(pr, out ct);
                _cardTimers.Add(ct);
                NotifList.Children.Add(card);
            }
        }

        private Border CrearTarjetaPreReg(PreRegistro pr, out DispatcherTimer cardTimer)
        {
            int segsRestantes = (int)(pr.ExpiraEn - DateTime.Now).TotalSeconds;
            if (segsRestantes < 0) segsRestantes = 0;

            // Countdown TextBlock
            var txtCountdown = new TextBlock
            {
                FontSize   = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = _brushAccent,
                Margin     = new Thickness(0, 3, 0, 0),
            };

            void RefrescarTexto(int s)
            {
                int m = s / 60, seg = s % 60;
                txtCountdown.Text = $"Expira en: {m}:{seg:D2}";
            }
            RefrescarTexto(segsRestantes);

            int segsLocal = segsRestantes;
            var ct = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            ct.Tick += (sender, e) =>
            {
                segsLocal--;
                if (segsLocal <= 0) { ct.Stop(); txtCountdown.Text = "Expirado"; return; }
                RefrescarTexto(segsLocal);
            };
            ct.Start();
            cardTimer = ct;

            // Botón "Llevar a Registro"
            var btn = new Button
            {
                Content         = "Llevar a Registro",
                Margin          = new Thickness(0, 8, 0, 0),
                Padding         = new Thickness(10, 5, 10, 5),
                FontSize        = 11.5,
                Cursor          = Cursors.Hand,
                Background      = _brushAccent,
                Foreground      = _brushWhite,
                BorderThickness = new Thickness(0),
            };
            btn.Click += (s, e) =>
            {
                ct.Stop();
                NotifPopup.IsOpen = false;
                AbrirRegistroConPreReg(pr);
            };

            var sp = new StackPanel();
            sp.Children.Add(new TextBlock
            {
                Text       = pr.NombreCompleto,
                FontSize   = 12.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = _brushWhite,
            });
            sp.Children.Add(new TextBlock
            {
                Text        = $"Plan: {pr.NombreMembresia ?? "Sin plan"}  •  {pr.Correo ?? "—"}",
                FontSize    = 11,
                Foreground  = _brushMuted,
                TextWrapping = TextWrapping.Wrap,
                Margin      = new Thickness(0, 2, 0, 0),
            });
            sp.Children.Add(txtCountdown);
            sp.Children.Add(btn);

            return new Border
            {
                CornerRadius    = new CornerRadius(8),
                Padding         = new Thickness(10),
                Margin          = new Thickness(0, 0, 0, 6),
                Background      = _brushCardBg,
                BorderThickness = new Thickness(1),
                BorderBrush     = _brushCardBorder,
                Child           = sp,
            };
        }

        private void AbrirRegistroConPreReg(PreRegistro preReg)
        {
            NavClientes.IsChecked = true;
            TxtSeccionActual.Text = "Clientes";
            MainContent.Content   = new ClientesView();

            var dialog = new EditarClienteDialog(preReg) { Owner = this };
            dialog.ShowDialog();
            PollearPreRegistros();
        }

        // ════════════════════════════════════════════════════════
        //  NAVEGACIÓN
        // ════════════════════════════════════════════════════════

        private void NavBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;
            if (!(sender is RadioButton btn)) return;

            string seccion = btn.Name.Replace("Nav", "");

            TxtSeccionActual.Text = seccion switch
            {
                "Dashboard"  => "Dashboard",
                "Acceso"     => "Acceso",
                "Ventas"     => "Ventas / Caja",
                "Clientes"   => "Clientes",
                "Membresias" => "Membresías",
                "Clases"     => "Clases y Progreso",
                "Reportes"   => "Reportes",
                "Sistema"    => "Sistema",
                _            => "Acceso"
            };

            MainContent.Content = seccion switch
            {
                "Dashboard"  => new DashboardView(),
                "Acceso"     => new AccesoView(),
                "Ventas"     => new VentasView(),
                "Clientes"   => new ClientesView(),
                "Membresias" => new MembresiasView(),
                "Clases"     => new ClasesView(),
                "Reportes"   => new ReportesView(),
                "Sistema"    => new SistemaView(),
                _            => new AccesoView()
            };
        }

        // ════════════════════════════════════════════════════════
        //  TEMA + VENTANA
        // ════════════════════════════════════════════════════════

        private void BtnPantallaCompleta_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                IconPantallaCompleta.Icon = FontAwesome.Sharp.IconChar.Expand;
                BtnPantallaCompleta.ToolTip = "Pantalla completa";
            }
            else
            {
                WindowState = WindowState.Maximized;
                IconPantallaCompleta.Icon = FontAwesome.Sharp.IconChar.Compress;
                BtnPantallaCompleta.ToolTip = "Salir de pantalla completa";
            }
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

            ThemeIcon.Text  = _modoClaro ? "🌙" : "☀";
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
