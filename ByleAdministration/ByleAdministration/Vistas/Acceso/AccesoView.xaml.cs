using ByleAdministration.Modelos;
using ByleAdministration.Servicios;
using ByleAdministration.Utilidades;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ByleAdministration.Vistas.Acceso
{
    public partial class AccesoView : UserControl
    {
        // ── Servicios ───────────────────────────────────────────────
        private readonly ServicioAcceso _servicioAcceso = new ServicioAcceso();
        private readonly ServicioBiometria _servicioBiometria = new ServicioBiometria();
        private LectorHuella _lector;

        // ── Estado actual ────────────────────────────────────────────
        private Cliente _clienteActual;     // cliente identificado o buscado
        private bool _enEspera = false;  // true = esperando que el usuario presione entrada/salida

        // ── Timers ───────────────────────────────────────────────────
        private DispatcherTimer _timerContador;   // actualiza contadores cada 15s
        private DispatcherTimer _timerReiniciar;  // tras identificar, vuelve a Idle si no hay acción

        // ── Colores ──────────────────────────────────────────────────
        private static readonly Color _colorNaranja = Color.FromRgb(0xF2, 0x64, 0x19);
        private static readonly Color _colorVerde = Color.FromRgb(0x2E, 0xCC, 0x71);
        private static readonly Color _colorRojo = Color.FromRgb(0xE7, 0x4C, 0x3C);
        private static readonly Color _colorGris = Color.FromRgb(0x70, 0x70, 0xA0);
    
        // ══════════════════════════════════════════════════════════
        // CICLO DE VIDA
        // ══════════════════════════════════════════════════════════
        public AccesoView()
        {
            InitializeComponent();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Sensor global
            GestorSensor.Instancia.EstadoCambiado += OnEstadoSensorGlobal;
            ActualizarBadgeSensor(GestorSensor.Instancia.Estado);

            // Lector en modo verificación (continuo)
            IniciarLector();

            // Timer contador (cada 15 segundos)
            _timerContador = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
            _timerContador.Tick += (s, ev) => ActualizarContadores();
            _timerContador.Start();

            // Cargar bitácora y contadores al abrir
            ActualizarContadores();
            CargarBitacora();

            // Animación idle del sensor
            BeginStoryboard((Storyboard)Resources["SbIdle"]);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            GestorSensor.Instancia.EstadoCambiado -= OnEstadoSensorGlobal;
            _lector?.Detener();
            _lector?.Dispose();
            _timerContador?.Stop();
            _timerReiniciar?.Stop();
        }

        // ══════════════════════════════════════════════════════════
        // SENSOR BIOMÉTRICO
        // ══════════════════════════════════════════════════════════

        private void IniciarLector()
        {
            _lector?.Detener();
            _lector?.Dispose();

            _lector = new LectorHuella(ModoLector.Verificacion);
            _lector.FingerTouched += OnFingerTouched;
            _lector.MuestraCaptured += OnMuestraCaptured;
            _lector.Error += OnErrorLector;
            _lector.Iniciar();
        }

        private void OnFingerTouched(bool tocando)
        {
            if (tocando)
            {
                // Dedo en sensor → animación naranja rápida
                SetVisualSensor(_colorNaranja, "Escaneando...", "Mantén el dedo quieto");
                var sb = (Storyboard)Resources["SbEscaneando"];
                BeginStoryboard(sb);
            }
            else
            {
                // Dedo levantado
                if (!_enEspera)
                {
                    SetVisualSensor(_colorNaranja, "Acercar dedo al sensor...",
                        "El sistema identificará al cliente automáticamente");
                    BeginStoryboard((Storyboard)Resources["SbIdle"]);
                }
            }
        }

        private void OnMuestraCaptured(DPFP.Sample muestra)
        {
            // Detener lector mientras procesamos (evitar doble disparo)
            _lector.Detener();

            int? idUsuario = _servicioBiometria.Identificar(muestra);

            if (idUsuario.HasValue)
            {
                // Obtener cliente completo
                var repo = new Repositorios.RepositorioCliente();
                var cliente = repo.ObtenerPorId(idUsuario.Value);

                if (cliente != null)
                    MostrarClienteIdentificado(cliente, "huella");
                else
                    MostrarNoIdentificado();
            }
            else
            {
                MostrarNoIdentificado();
            }
        }

        private void OnErrorLector(string msg)
        {
            // Ignorar errores de calidad silenciosamente en modo verificación
        }

        // ══════════════════════════════════════════════════════════
        // PRESENTACIÓN DEL CLIENTE IDENTIFICADO
        // ══════════════════════════════════════════════════════════

        private void MostrarClienteIdentificado(Cliente c, string metodo)
        {
            _clienteActual = c;
            _enEspera = true;

            // Visual sensor → verde
            SetVisualSensor(_colorVerde, "¡Cliente identificado!",
                "Registra la entrada o salida");
            BrushCirculo.Color = Color.FromArgb(0x2A, 0x2E, 0xCC, 0x71);

            // Llenar tarjeta
            TxtCardIniciales.Text = c.Iniciales;
            TxtCardNombre.Text = c.NombreCompleto;
            TxtCardMembresia.Text = c.NombreMembresia;

            bool vigente = _servicioAcceso.MembresiasVigente(c);
            if (vigente)
            {
                TxtEstadoCard.Text = "ACTIVO";
                TxtEstadoCard.Foreground = new SolidColorBrush(_colorVerde);
                BorderEstadoCard.Background = new SolidColorBrush(Color.FromArgb(0x25, 0x2E, 0xCC, 0x71));
            }
            else
            {
                TxtEstadoCard.Text = "VENCIDA";
                TxtEstadoCard.Foreground = new SolidColorBrush(_colorRojo);
                BorderEstadoCard.Background = new SolidColorBrush(Color.FromArgb(0x25, 0xE7, 0x4C, 0x3C));
            }

            TxtCardVencimiento.Text = c.FechaRenovacion.HasValue
                ? $"Vence: {c.FechaRenovacion.Value:dd 'de' MMMM 'de' yyyy}"
                : "Sin fecha de vencimiento";

            // Alerta si vence pronto
            if (_servicioAcceso.ProximoAVencer(c))
            {
                int dias = (int)(c.FechaRenovacion.Value - DateTime.Today).TotalDays;
                TxtAlerta.Text = dias == 0 ? "Vence hoy" : $"Vence en {dias} día{(dias == 1 ? "" : "s")}";
                BorderAlerta.Visibility = Visibility.Visible;
            }
            else
            {
                BorderAlerta.Visibility = Visibility.Collapsed;
            }

            // Auto-detectar si corresponde entrada o salida
            bool dentroAhora = _servicioAcceso.EstaAdentro(c.IdUsuario);
            BtnEntrada.IsEnabled = !dentroAhora;
            BtnSalida.IsEnabled = dentroAhora;

            // Resaltar el botón sugerido
            if (!dentroAhora)
            {
                BtnEntrada.Opacity = 1.0;
                BtnSalida.Opacity = 0.45;
            }
            else
            {
                BtnEntrada.Opacity = 0.45;
                BtnSalida.Opacity = 1.0;
            }

            BorderCard.Visibility = Visibility.Visible;

            // Si no hay acción en 20 segundos, volver a idle
            _timerReiniciar?.Stop();
            _timerReiniciar = new DispatcherTimer { Interval = TimeSpan.FromSeconds(20) };
            _timerReiniciar.Tick += (s, ev) =>
            {
                _timerReiniciar.Stop();
                VolverlAIdle();
            };
            _timerReiniciar.Start();
        }

        private void MostrarNoIdentificado()
        {
            SetVisualSensor(_colorRojo, "No identificado",
                "Usa la búsqueda manual para registrar");
            BrushCirculo.Color = Color.FromArgb(0x2A, 0xE7, 0x4C, 0x3C);

            // Volver a idle tras 3 segundos
            var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            t.Tick += (s, ev) => { t.Stop(); VolverlAIdle(); };
            t.Start();
        }

        private void VolverlAIdle()
        {
            _clienteActual = null;
            _enEspera = false;
            BorderCard.Visibility = Visibility.Collapsed;
            BorderAlerta.Visibility = Visibility.Collapsed;
            SetVisualSensor(_colorNaranja, "Acercar dedo al sensor...",
                "El sistema identificará al cliente automáticamente");
            BrushCirculo.Color = Color.FromArgb(0x1A, 0xF2, 0x64, 0x19);
            BeginStoryboard((Storyboard)Resources["SbIdle"]);

            // Reiniciar lector
            IniciarLector();
        }

        // ══════════════════════════════════════════════════════════
        // REGISTRO DE ACCESO
        // ══════════════════════════════════════════════════════════

        private void BtnEntrada_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteActual == null) return;
            _timerReiniciar?.Stop();

            _servicioAcceso.RegistrarAcceso(_clienteActual.IdUsuario,
                _clienteActual != null ? "huella" : "manual");

            ActualizarContadores();
            CargarBitacora();
            VolverlAIdle();
        }

        private void BtnSalida_Click(object sender, RoutedEventArgs e)
        {
            if (_clienteActual == null) return;
            _timerReiniciar?.Stop();

            _servicioAcceso.RegistrarAcceso(_clienteActual.IdUsuario,
                _clienteActual != null ? "huella" : "manual");

            ActualizarContadores();
            CargarBitacora();
            VolverlAIdle();
        }

        // ══════════════════════════════════════════════════════════
        // BÚSQUEDA MANUAL
        // ══════════════════════════════════════════════════════════

        private void TxtBusqueda_KeyDown(object sender,
            System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                BuscarManual();
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
            => BuscarManual();

        private void BuscarManual()
        {
            string termino = TxtBusquedaManual.Text.Trim();
            if (string.IsNullOrEmpty(termino)) return;

            var repo = new Repositorios.RepositorioCliente();
            var lista = repo.BuscarPorNombre(termino);

            if (lista.Count == 0)
            {
                MessageBox.Show("No se encontró ningún cliente con ese nombre.",
                    "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Con 1 resultado: mostrar directo; con varios: mostrar el primero
            MostrarClienteIdentificado(lista[0], "manual");
            TxtBusquedaManual.Text = "";
        }

        // ══════════════════════════════════════════════════════════
        // CONTADORES Y BITÁCORA
        // ══════════════════════════════════════════════════════════

        private void ActualizarContadores()
        {
            try
            {
                TxtPersonasDentro.Text = _servicioAcceso.PersonasDentro().ToString();
                TxtEntradasHoy.Text = _servicioAcceso.EntradasHoy().ToString();
                TxtSalidasHoy.Text = _servicioAcceso.SalidasHoy().ToString();
            }
            catch { /* ignorar si BD no disponible */ }
        }

        private void CargarBitacora()
        {
            try
            {
                var eventos = _servicioAcceso.BitacoraHoy(40);
                PanelBitacora.Children.Clear();

                foreach (var reg in eventos)
                    PanelBitacora.Children.Add(CrearFilaBitacora(reg));
            }
            catch { }
        }

        private UIElement CrearFilaBitacora(RegistroAcceso reg)
        {
            var grid = new Grid
            {
                Height = 52,
                Margin = new Thickness(0, 0, 0, 1)
            };
            int idx = PanelBitacora.Children.Count;
            if (idx % 2 == 0)
                grid.SetResourceReference(Grid.BackgroundProperty, "BrushBgCardHover");

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(36) });

            // Barra lateral de color
            var barra = new System.Windows.Shapes.Rectangle
            {
                Width = 3,
                RadiusX = 2,
                RadiusY = 2,
                Fill = new SolidColorBrush(reg.EsEntrada ? _colorVerde : _colorNaranja)
            };
            Grid.SetColumn(barra, 0);
            grid.Children.Add(barra);

            // Avatar
            var avatar = new Border
            {
                Width = 34,
                Height = 34,
                CornerRadius = new CornerRadius(17),
                Margin = new Thickness(5, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatar.SetResourceReference(Border.BackgroundProperty, "BrushAccentMuted");
            var avatarTxt = new TextBlock
            {
                Text = reg.Iniciales,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            avatarTxt.SetResourceReference(TextBlock.ForegroundProperty, "BrushAccent");
            avatar.Child = avatarTxt;
            Grid.SetColumn(avatar, 1);
            grid.Children.Add(avatar);

            // Nombre + membresía
            var info = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
            var nombre = new TextBlock { Text = reg.NombreCompleto, FontSize = 12, FontWeight = FontWeights.Medium };
            nombre.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextPrimary");
            info.Children.Add(nombre);
            var mem = new TextBlock { Text = reg.NombreMembresia, FontSize = 10 };
            mem.SetResourceReference(TextBlock.ForegroundProperty, "BrushTextMuted");
            info.Children.Add(mem);
            Grid.SetColumn(info, 2);
            grid.Children.Add(info);

            // Hora
            var hora = new TextBlock
            {
                Text = reg.HoraFormateada,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(_colorGris),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            Grid.SetColumn(hora, 3);
            grid.Children.Add(hora);

            // Flecha entrada/salida
            var flechaBorder = new Border
            {
                Width = 24,
                Height = 24,
                CornerRadius = new CornerRadius(6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(
                    reg.EsEntrada
                        ? Color.FromArgb(0x25, 0x2E, 0xCC, 0x71)
                        : Color.FromArgb(0x20, 0xF2, 0x64, 0x19))
            };

            // Flecha via RotateTransform sobre un camino simple
            var flecha = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    reg.EsEntrada ? "M 6 3 L 12 9 L 6 15 M 12 9 L 0 9" : "M 6 15 L 12 9 L 6 3 M 12 9 L 0 9"),
                Stroke = new SolidColorBrush(reg.EsEntrada ? _colorVerde : _colorNaranja),
                StrokeThickness = 1.8,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Stretch = Stretch.Uniform,
                Width = 14,
                Height = 14
            };
            flechaBorder.Child = flecha;
            Grid.SetColumn(flechaBorder, 4);
            grid.Children.Add(flechaBorder);

            return grid;
        }

        // ══════════════════════════════════════════════════════════
        // ESTADO DEL SENSOR (badge superior)
        // ══════════════════════════════════════════════════════════

        private void OnEstadoSensorGlobal(EstadoSensor estado)
            => ActualizarBadgeSensor(estado);

        private void ActualizarBadgeSensor(EstadoSensor estado)
        {
            bool activo = estado == EstadoSensor.Listo ||
                          estado == EstadoSensor.Capturando ||
                          estado == EstadoSensor.Conectado;

            Color col = activo ? _colorVerde : _colorRojo;
            BrushDotSensor.Color = col;
            BrushTextoSensor.Color = col;
            BrushEstadoFondo.Color = Color.FromArgb(0x20,
                col.R, col.G, col.B);
            TxtEstadoSensor.Text = activo
                ? "Sensor biométrico activo"
                : "Sensor no conectado";
        }

        // ══════════════════════════════════════════════════════════
        // HELPERS VISUALES DEL SENSOR
        // ══════════════════════════════════════════════════════════

        private void SetVisualSensor(Color color, string mensaje, string sub)
        {
            BrushIcono.Color = color;
            GlowStop.Color = color;
            BrushMensaje.Color = Color.FromRgb(0xF0, 0xF0, 0xF5);
            TxtSensorMensaje.Text = mensaje;
            TxtSensorSub.Text = sub;
        }

        private void BeginStoryboard(Storyboard sb)
        {
            sb.Stop(this);
            sb.Begin(this, true);
        }
    }
}