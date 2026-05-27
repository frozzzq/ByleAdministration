// ── El alias resuelve el conflicto entre:
//    - ByleAdministration.Vistas.Clientes  (este namespace)
//    - ByleAdministration.Modelos.Clientes (el modelo)
// Sin el alias, C# lee "Clientes" como el segmento del namespace, no como el tipo.
using ModeloCliente = ByleAdministration.Modelos.Cliente;

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using ByleAdministration.Servicios;
using ByleAdministration.Soportes;
using ByleAdministration.Utilidades;

namespace ByleAdministration.Vistas.Clientes.Dialogs
{
    public partial class EditarClienteDialog : Window
    {
        // ── Servicios ───────────────────────────────────────────────
        private readonly ServicioCliente _servicioCliente = new ServicioCliente();
        private readonly ServicioBiometria _servicioBio = new ServicioBiometria();
        private LectorHuella _lector;

        // ── Estado de la sesión ─────────────────────────────────────
        private readonly int? _idUsuario;
        private readonly int? _idPreRegistro;
        private byte[] _templateBytes;
        private int _capturas = 0;

        // Datos originales del cliente al editar — necesarios para saber
        // si cambió la membresía y recalcular FechaRenovacion solo en ese caso.
        private int _idMembresiaOriginal = 0;
        private DateTime? _fechaRenovacionOriginal = null;

        // ── Pinceles para los dots de progreso ──────────────────────
        private static readonly SolidColorBrush _brushNaranja = new SolidColorBrush(Color.FromRgb(0xF2, 0x64, 0x19));
        private static readonly SolidColorBrush _brushGris = new SolidColorBrush(Color.FromRgb(0x44, 0x44, 0x5A));
        private static readonly SolidColorBrush _brushVerde = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99));
        private static readonly SolidColorBrush _brushRojo = new SolidColorBrush(Color.FromRgb(0xF8, 0x71, 0x71));
        private static readonly SolidColorBrush _brushAmarillo = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24));

        // ══════════════════════════════════════════════════════════
        // CONSTRUCTOR
        // ══════════════════════════════════════════════════════════

        public EditarClienteDialog(int? idUsuario = null)
        {
            InitializeComponent();
            _idUsuario = idUsuario;

            if (idUsuario.HasValue)
            {
                TxtTitulo.Text = "Editar cliente";
                TxtSubtitulo.Text = $"ID: {idUsuario}";
            }

            GestorSensor.Instancia.EstadoCambiado += OnEstadoSensorCambiado;
            OnEstadoSensorCambiado(GestorSensor.Instancia.Estado);

            CargarMembresías();

            if (idUsuario.HasValue)
                CargarCliente(idUsuario.Value);

            // Log inicial en Loaded para que el ScrollViewer ya esté medido
            Loaded += (s, e) =>
            {
                AgregarEvento("[Sistema] Esperando conexión del sensor...");
                if (idUsuario.HasValue && _servicioBio.TieneHuella(idUsuario.Value))
                    AgregarEvento("[Huella] El cliente ya tiene huella registrada. Enrola de nuevo para actualizarla.");
            };
        }

        // Constructor llamado desde bandeja de notificaciones — pre-llena el formulario
        public EditarClienteDialog(PreRegistro preReg) : this((int?)null)
        {
            _idPreRegistro = preReg.IdPre;
            TxtSubtitulo.Text = "Desde pre-registro web";

            TxtNombreCompleto.Text = preReg.NombreCompleto ?? "";
            TxtEdad.Text           = preReg.Edad?.ToString() ?? "";
            TxtCiudad.Text         = preReg.Ciudad ?? "Los Mochis";
            TxtCorreo.Text         = preReg.Correo ?? "";
            TxtTelefono.Text       = preReg.Telefono?.ToString() ?? "";
            TxtTelefonoEmergencia.Text = preReg.TelefonoEmergencia?.ToString() ?? "";

            if (preReg.IdMembresia.HasValue)
                foreach (var item in CmbMembresia.Items)
                    if (item is Membresia m && m.IdMembresia == preReg.IdMembresia.Value)
                    { CmbMembresia.SelectedItem = item; break; }
        }

        // ══════════════════════════════════════════════════════════
        // CARGA DE DATOS
        // ══════════════════════════════════════════════════════════

        private void CargarMembresías()
        {
            try
            {
                var planes = _servicioCliente.ObtenerMembresiasActivas();
                CmbMembresia.ItemsSource = planes;
                if (planes.Count > 0) CmbMembresia.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                AgregarEvento($"[Error] No se pudieron cargar las membresías: {ex.Message}");
            }
        }

        private void CargarCliente(int id)
        {
            try
            {
                ModeloCliente c = _servicioCliente.Obtener(id);
                if (c == null) return;

                TxtNombreCompleto.Text = c.NombreCompleto;
                TxtEdad.Text = c.Edad.ToString();
                TxtCiudad.Text = c.Ciudad;
                TxtCorreo.Text = c.Correo;
                TxtTelefono.Text = c.Telefono.ToString();
                TxtTelefonoEmergencia.Text = c.TelefonoEmergencia.ToString();

                // Guardar datos originales para comparar al guardar
                _idMembresiaOriginal = c.IdMembresia;
                _fechaRenovacionOriginal = c.FechaRenovacion;

                foreach (var item in CmbMembresia.Items)
                    if (item is Membresia m && m.IdMembresia == c.IdMembresia)
                    { CmbMembresia.SelectedItem = item; break; }
            }
            catch (Exception ex)
            {
                AgregarEvento($"[Error] No se pudo cargar el cliente: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════
        // ESTADO DEL SENSOR
        // ══════════════════════════════════════════════════════════

        private void OnEstadoSensorCambiado(EstadoSensor estado)
        {
            SolidColorBrush colorDot;
            string texto;

            switch (estado)
            {
                case EstadoSensor.Listo:
                    colorDot = _brushVerde;
                    texto = "Listo";
                    AgregarEvento($"[Sensor] {GestorSensor.Instancia.ModeloSensor} conectado y listo.");
                    BtnEnrolar.IsEnabled = true;
                    break;
                case EstadoSensor.Conectado:
                    colorDot = _brushVerde;
                    texto = "Conectado";
                    BtnEnrolar.IsEnabled = true;
                    break;
                case EstadoSensor.Capturando:
                    colorDot = _brushAmarillo;
                    texto = "Capturando...";
                    break;
                case EstadoSensor.Error:
                    colorDot = _brushRojo;
                    texto = "Error en sensor";
                    AgregarEvento("[Error] Ocurrió un error en el sensor.");
                    break;
                default: // Desconectado
                    colorDot = _brushRojo;
                    texto = "Sensor no conectado";
                    AgregarEvento("[Sistema] Sensor desconectado. Conecta el DigitalPersona 4500 por USB.");
                    BtnEnrolar.IsEnabled = false;
                    break;
            }

            DotSensor.Fill = colorDot;
            TxtEstadoSensor.Text = texto;
            TxtEstadoSensor.Foreground = colorDot;
        }

        // ══════════════════════════════════════════════════════════
        // ENROLAMIENTO BIOMÉTRICO
        // ══════════════════════════════════════════════════════════

        private void BtnEnrolar_Click(object sender, RoutedEventArgs e)
        {
            if (GestorSensor.Instancia.Estado == EstadoSensor.Desconectado ||
                GestorSensor.Instancia.Estado == EstadoSensor.Error)
            {
                AgregarEvento("[Error] Conecta el sensor antes de enrolar.");
                return;
            }
            IniciarEnrolamiento();
        }

        private void IniciarEnrolamiento()
        {
            _lector?.Detener();
            _lector?.Dispose();
            _capturas = 0;
            _templateBytes = null;

            _lector = new LectorHuella(ModoLector.Enrolamiento);
            _lector.ProgresoEnrolamiento += OnProgreso;
            _lector.EnrolamientoCompleto += OnEnrolamientoCompleto;
            _lector.Error += OnErrorLector;
            _lector.FingerTouched += OnFingerTouched;
            _lector.Iniciar();

            MostrarPanel(PanelEnrolando);
            BtnEnrolar.IsEnabled = false;
            BtnReiniciar.IsEnabled = true;
            ActualizarDots(0);
            TxtContador.Text = "0 / 4 capturas";
            TxtMensajeCaptura.Text = "Acerca el dedo al sensor...";

            AgregarEvento("[Info] Enrolamiento iniciado. Se requieren 4 capturas.");
            AgregarEvento("[Info] Coloca el dedo sobre el sensor...");
        }

        private void OnProgreso(int restantes)
        {
            _capturas = 4 - restantes;
            ActualizarDots(_capturas);
            TxtContador.Text = $"{_capturas} / 4 capturas";
            AgregarEvento($"[Captura {_capturas}/4] ✓ Muestra aceptada. Faltan {restantes}.");
            if (restantes > 0)
            {
                TxtMensajeCaptura.Text = "Levanta el dedo y vuelve a colocarlo";
                AgregarEvento($"[Info] Levanta el dedo y colócalo de nuevo ({_capturas + 1}ª captura)...");
            }
        }

        private void OnEnrolamientoCompleto(byte[] templateBytes)
        {
            _templateBytes = templateBytes;
            MostrarPanel(PanelCompleto);
            ActualizarDots(4);
            TxtContador.Text = "4 / 4 capturas";
            BtnEnrolar.IsEnabled = false;
            BtnReiniciar.IsEnabled = true;
            AgregarEvento("[✓] Enrolamiento completado. Template generado correctamente.");
            AgregarEvento("[Info] Presiona 'Guardar cambios' para registrar al cliente.");
        }

        private void OnErrorLector(string mensaje)
        {
            AgregarEvento($"[Error] {mensaje}");
            if (PanelEnrolando.IsVisible)
                TxtMensajeCaptura.Text = "Captura fallida — intenta de nuevo";
        }

        private void OnFingerTouched(bool tocando)
        {
            if (!PanelEnrolando.IsVisible) return;
            TxtMensajeCaptura.Text = tocando
                ? "Mantén el dedo quieto..."
                : (_capturas < 4 ? "Levanta el dedo y vuelve a colocarlo" : "Procesando...");
        }

        private void BtnReiniciar_Click(object sender, RoutedEventArgs e)
        {
            _lector?.Detener();
            _lector?.Dispose();
            _lector = null;
            _capturas = 0;
            _templateBytes = null;

            MostrarPanel(PanelSinHuella);
            ActualizarDots(0);
            TxtContador.Text = "0 / 4 capturas";
            BtnEnrolar.IsEnabled = GestorSensor.Instancia.Estado != EstadoSensor.Desconectado;
            BtnReiniciar.IsEnabled = false;
            AgregarEvento("[Info] Enrolamiento reiniciado. Presiona 'Enrolar' para comenzar de nuevo.");
        }

        private void MostrarPanel(System.Windows.Controls.Panel panel)
        {
            PanelSinHuella.Visibility = panel == PanelSinHuella ? Visibility.Visible : Visibility.Collapsed;
            PanelEnrolando.Visibility = panel == PanelEnrolando ? Visibility.Visible : Visibility.Collapsed;
            PanelCompleto.Visibility = panel == PanelCompleto ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ActualizarDots(int llenos)
        {
            var dots = new[] { CapDot1, CapDot2, CapDot3, CapDot4 };
            for (int i = 0; i < dots.Length; i++)
                dots[i].Fill = i < llenos ? _brushNaranja : _brushGris;
        }

        // ══════════════════════════════════════════════════════════
        // LOG DE EVENTOS
        // ══════════════════════════════════════════════════════════

        private void AgregarEvento(string mensaje)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            TxtEventos.Text += $"[{ts}] {mensaje}\n";
            ScrollEventos.UpdateLayout();
            ScrollEventos.ScrollToEnd();
        }

        // ══════════════════════════════════════════════════════════
        // GUARDAR
        // ══════════════════════════════════════════════════════════

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos()) return;
            BtnGuardar.IsEnabled = false;

            try
            {
                int idFinal;

                if (_idUsuario.HasValue)
                {
                    ModeloCliente cliente = ConstruirModelo();
                    cliente.IdUsuario = _idUsuario.Value;
                    _servicioCliente.Actualizar(cliente);
                    idFinal = _idUsuario.Value;
                    AgregarEvento("[BD] Datos del cliente actualizados correctamente.");
                }
                else
                {
                    ModeloCliente cliente = ConstruirModelo();
                    idFinal = _servicioCliente.Registrar(cliente);
                    AgregarEvento($"[BD] Cliente registrado. ID asignado: {idFinal}.");
                }

                if (_templateBytes != null)
                {
                    bool ok = _servicioBio.RegistrarHuella(idFinal, _templateBytes);
                    AgregarEvento(ok
                        ? "[BD] Huella digital guardada exitosamente."
                        : "[Advertencia] No se pudo guardar la huella en la BD.");
                }

                if (_idPreRegistro.HasValue)
                {
                    try
                    {
                        new RepositorioPreRegistro().MarcarAtendido(_idPreRegistro.Value);
                        AgregarEvento("[Pre-registro] Marcado como atendido.");
                    }
                    catch { /* no crítico */ }
                }

                var planVenta = (Membresia)CmbMembresia.SelectedItem;
                bool esNuevo          = !_idUsuario.HasValue;
                bool cambioMembresia  = _idUsuario.HasValue && planVenta.IdMembresia != _idMembresiaOriginal;
                if (esNuevo || cambioMembresia)
                {
                    try
                    {
                        new RepositorioVenta().Insertar(
                            idFinal,
                            SesionActual.IdEmpleado,
                            planVenta.Precio,
                            "membresia");
                        AgregarEvento($"[Venta] Registrada: ${planVenta.Precio:N0} — {planVenta.NombreMembresia}");
                    }
                    catch (Exception exV)
                    {
                        AgregarEvento($"[Advertencia] No se pudo registrar la venta: {exV.Message}");
                    }
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                BtnGuardar.IsEnabled = true;
                AgregarEvento($"[Error] {ex.Message}");
                MessageBox.Show($"Error al guardar:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>Crea un ModeloCliente con los valores actuales del formulario.</summary>
        private ModeloCliente ConstruirModelo()
        {
            var plan = (Membresia)CmbMembresia.SelectedItem;
            var ahora = DateTime.Now;

            // ── Cálculo de fechas ──────────────────────────────────────
            // NUEVO cliente: inscripción = ahora, renovación = ahora + duracion_dias
            // EDITAR cliente sin cambiar membresía: conservar la renovación original
            // EDITAR cliente cambiando membresía: recalcular renovación desde ahora
            DateTime fechaInscripcion;
            DateTime? fechaRenovacion;

            if (!_idUsuario.HasValue)
            {
                // Registro nuevo
                fechaInscripcion = ahora;
                fechaRenovacion = ahora.AddDays(plan.DuracionDias);
                AgregarEvento($"[Info] Inscripción: {fechaInscripcion:dd/MM/yyyy} — " +
                              $"Vence: {fechaRenovacion:dd/MM/yyyy} ({plan.DuracionDias} días).");
            }
            else if (plan.IdMembresia != _idMembresiaOriginal)
            {
                // Cambio de membresía durante edición → renovar desde hoy
                fechaInscripcion = ahora;
                fechaRenovacion = ahora.AddDays(plan.DuracionDias);
                AgregarEvento($"[Info] Membresía cambiada. Nueva renovación: " +
                              $"{fechaRenovacion:dd/MM/yyyy} ({plan.DuracionDias} días).");
            }
            else
            {
                // Edición sin cambio de membresía → conservar fechas originales
                fechaInscripcion = ahora;                  // se actualiza la inscripción por si era null
                fechaRenovacion = _fechaRenovacionOriginal;
            }

            return new ModeloCliente
            {
                NombreCompleto = TxtNombreCompleto.Text.Trim(),
                Edad = int.Parse(TxtEdad.Text.Trim()),
                Ciudad = TxtCiudad.Text.Trim(),
                Correo = TxtCorreo.Text.Trim(),
                Telefono = long.Parse(TxtTelefono.Text.Trim()),
                TelefonoEmergencia = long.Parse(TxtTelefonoEmergencia.Text.Trim()),
                IdMembresia = plan.IdMembresia,
                Estado = "activo",
                FechaInscripcion = fechaInscripcion,
                FechaRenovacion = fechaRenovacion
            };
        }

        // ── Validación ──────────────────────────────────────────────
        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(TxtNombreCompleto.Text))
            { AgregarEvento("[Validación] El nombre completo es obligatorio."); TxtNombreCompleto.Focus(); return false; }

            if (!int.TryParse(TxtEdad.Text.Trim(), out int edad) || edad < 10 || edad > 100)
            { AgregarEvento("[Validación] Ingresa una edad válida (10 – 100)."); TxtEdad.Focus(); return false; }

            if (string.IsNullOrWhiteSpace(TxtCiudad.Text))
            { AgregarEvento("[Validación] La ciudad es obligatoria."); TxtCiudad.Focus(); return false; }

            if (string.IsNullOrWhiteSpace(TxtCorreo.Text) || !TxtCorreo.Text.Contains("@"))
            { AgregarEvento("[Validación] Ingresa un correo válido."); TxtCorreo.Focus(); return false; }

            if (!long.TryParse(TxtTelefono.Text.Trim(), out _))
            { AgregarEvento("[Validación] El teléfono solo debe contener números."); TxtTelefono.Focus(); return false; }

            if (!long.TryParse(TxtTelefonoEmergencia.Text.Trim(), out _))
            { AgregarEvento("[Validación] El tel. emergencia solo debe contener números."); TxtTelefonoEmergencia.Focus(); return false; }

            if (CmbMembresia.SelectedItem == null)
            { AgregarEvento("[Validación] Selecciona un plan de membresía."); return false; }

            if (_templateBytes == null)
            {
                var resp = MessageBox.Show(
                    "No se registró huella digital.\n¿Guardar el cliente sin huella?",
                    "Sin huella digital", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resp == MessageBoxResult.No)
                { AgregarEvento("[Info] Cancelado. Registra la huella primero."); return false; }

                AgregarEvento("[Advertencia] El cliente se guardará SIN huella digital.");
            }
            return true;
        }

        // ── Drag + Cierre ────────────────────────────────────────────
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        { if (e.ButtonState == MouseButtonState.Pressed) DragMove(); }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => CerrarDialog();
        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => CerrarDialog();

        private void CerrarDialog()
        {
            _lector?.Detener();
            _lector?.Dispose();
            DialogResult = false;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            GestorSensor.Instancia.EstadoCambiado -= OnEstadoSensorCambiado;
            _lector?.Detener();
            _lector?.Dispose();
            base.OnClosed(e);
        }
    }
}