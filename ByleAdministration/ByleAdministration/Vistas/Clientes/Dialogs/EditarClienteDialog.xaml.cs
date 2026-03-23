using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ByleAdministration.Vistas.Clientes.Dialogs
{
    public partial class EditarClienteDialog : Window
    {
        private readonly RepositorioCliente _repositorio = new RepositorioCliente();
        private Cliente _cliente;
        private int _capturasHuella = 0;
        private bool _sensorConectado = false;

        // Propiedad para saber si se guardó
        public bool SeGuardo { get; private set; } = false;

        public EditarClienteDialog(Cliente cliente)
        {
            InitializeComponent();
            _cliente = cliente;
            CargarDatos();
            VerificarSensor();
        }

        private void CargarDatos()
        {
            TxtSubtitulo.Text = "ID: " + _cliente.IdUsuario;
            TxtNombre.Text = _cliente.NombreCompleto;
            TxtEdad.Text = _cliente.Edad.ToString();
            TxtCiudad.Text = _cliente.Ciudad;
            TxtCorreo.Text = _cliente.Correo;
            TxtTelefono.Text = _cliente.Telefono.ToString();
            TxtTelEmergencia.Text = _cliente.TelefonoEmergencia.ToString();

            // Seleccionar estado en ComboBox
            foreach (ComboBoxItem item in CmbEstado.Items)
            {
                if (item.Content.ToString() == _cliente.Estado.ToLower())
                {
                    CmbEstado.SelectedItem = item;
                    break;
                }
            }

            // Verificar si ya tiene huella en la BD
            VerificarHuellaExistente();
        }

        // ═══════════════════════════════════════
        //  SENSOR DE HUELLA — UI placeholder
        // ═══════════════════════════════════════

        private void VerificarSensor()
        {
            // TODO: Aquí va la lógica real del SDK DigitalPersona
            // Por ahora simulamos el estado desconectado
            ActualizarEstadoSensor(false, "Sensor no conectado",
                "Conecta el DigitalPersona 4500 por USB");
            AgregarLog("[Sistema] Esperando conexión del sensor...");
        }

        private void VerificarHuellaExistente()
        {
            try
            {
                bool tieneHuella = _repositorio.TieneHuella(_cliente.IdUsuario);
                if (tieneHuella)
                {
                    TxtMensajeHuella.Text = "Huella registrada";
                    TxtMensajeHuella.Foreground = FindBrush("BrushSuccess");
                    TxtInstruccionHuella.Text = "El cliente ya tiene huella enrolada";
                    IconHuella.Foreground = FindBrush("BrushSuccess");
                    BarraProgreso.Value = 4;
                    TxtProgreso.Text = "Completado";
                }
            }
            catch
            {
                // Si la tabla biometria no existe aún, no pasa nada
            }
        }

        private void ActualizarEstadoSensor(bool conectado, string estado, string detalle)
        {
            _sensorConectado = conectado;
            TxtEstadoSensor.Text = estado;
            TxtDetalleSensor.Text = detalle;

            LedSensor.Background = conectado
                ? new SolidColorBrush(Color.FromRgb(0x2E, 0xCC, 0x71))  // verde
                : new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)); // rojo
        }

        private void AgregarLog(string mensaje)
        {
            string hora = DateTime.Now.ToString("HH:mm:ss");
            if (!string.IsNullOrEmpty(TxtLogSensor.Text)
                && !TxtLogSensor.Text.EndsWith("..."))
            {
                TxtLogSensor.Text += "\n";
            }
            else
            {
                TxtLogSensor.Text = "";
            }
            TxtLogSensor.Text += $"[{hora}] {mensaje}";
        }

        private void BtnEnrolar_Click(object sender, RoutedEventArgs e)
        {
            if (!_sensorConectado)
            {
                MessageBox.Show("Conecta el sensor DigitalPersona 4500 antes de enrolar.",
                    "Sensor no detectado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Aquí inicia la captura real con el SDK
            // El flujo sería:
            // 1. Llamar al SDK para iniciar captura
            // 2. Cada captura exitosa incrementar _capturasHuella
            // 3. Al llegar a 4 capturas, guardar el template en la BD

            AgregarLog("[Enrolamiento] Coloca el dedo en el sensor...");
            TxtMensajeHuella.Text = "Esperando dedo...";
            TxtMensajeHuella.Foreground = FindBrush("BrushAccent");
            TxtInstruccionHuella.Text = "Coloca tu dedo en el sensor";
            IconHuella.Foreground = FindBrush("BrushAccent");

            // Simulación de progreso (reemplazar con lógica real del SDK)
            SimularCaptura();
        }

        private void SimularCaptura()
        {
            // NOTA: Esto es solo visual para que veas cómo se ve.
            // Reemplaza con la integración real del SDK DigitalPersona.
            _capturasHuella++;
            BarraProgreso.Value = _capturasHuella;
            TxtProgreso.Text = $"{_capturasHuella} / 4 capturas";
            AgregarLog($"[Captura] Muestra {_capturasHuella} de 4 registrada");

            if (_capturasHuella >= 4)
            {
                TxtMensajeHuella.Text = "Huella enrolada exitosamente";
                TxtMensajeHuella.Foreground = FindBrush("BrushSuccess");
                TxtInstruccionHuella.Text = "Template listo para guardar";
                IconHuella.Foreground = FindBrush("BrushSuccess");
                AgregarLog("[Enrolamiento] Template generado correctamente");
                _capturasHuella = 0; // resetear para futuro re-enrolamiento
            }
        }

        private void BtnVerificar_Click(object sender, RoutedEventArgs e)
        {
            if (!_sensorConectado)
            {
                MessageBox.Show("Conecta el sensor primero.",
                    "Sensor no detectado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // TODO: Lógica real de verificación con SDK
            AgregarLog("[Verificación] Coloca el dedo para verificar...");
            TxtMensajeHuella.Text = "Verificando...";
            TxtInstruccionHuella.Text = "Coloca tu dedo en el sensor";
        }

        private void BtnEliminarHuella_Click(object sender, RoutedEventArgs e)
        {
            var resultado = MessageBox.Show(
                "¿Eliminar la huella registrada de este cliente?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                try
                {
                    _repositorio.EliminarHuella(_cliente.IdUsuario);
                    AgregarLog("[Sistema] Huella eliminada de la base de datos");
                    TxtMensajeHuella.Text = "Sin huella registrada";
                    TxtMensajeHuella.Foreground = FindBrush("BrushTextMuted");
                    TxtInstruccionHuella.Text = "Presiona 'Enrolar' para comenzar";
                    IconHuella.Foreground = FindBrush("BrushTextMuted");
                    BarraProgreso.Value = 0;
                    TxtProgreso.Text = "0 / 4 capturas";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar huella: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ═══════════════════════════════════════
        //  GUARDAR / CANCELAR
        // ═══════════════════════════════════════

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(TxtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNombre.Focus();
                return;
            }

            int edad;
            if (!int.TryParse(TxtEdad.Text.Trim(), out edad) || edad < 1 || edad > 120)
            {
                MessageBox.Show("Ingresa una edad válida.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtEdad.Focus();
                return;
            }

            long telefono;
            if (!long.TryParse(TxtTelefono.Text.Trim(), out telefono))
            {
                MessageBox.Show("Ingresa un teléfono válido.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTelefono.Focus();
                return;
            }

            long telEmergencia;
            if (!long.TryParse(TxtTelEmergencia.Text.Trim(), out telEmergencia))
            {
                MessageBox.Show("Ingresa un teléfono de emergencia válido.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTelEmergencia.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtCorreo.Text))
            {
                MessageBox.Show("El correo es obligatorio.", "Validación",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCorreo.Focus();
                return;
            }

            // Actualizar el objeto
            _cliente.NombreCompleto = TxtNombre.Text.Trim();
            _cliente.Edad = edad;
            _cliente.Ciudad = TxtCiudad.Text.Trim();
            _cliente.Correo = TxtCorreo.Text.Trim();
            _cliente.Telefono = telefono;
            _cliente.TelefonoEmergencia = telEmergencia;

            var itemEstado = CmbEstado.SelectedItem as ComboBoxItem;
            _cliente.Estado = itemEstado?.Content.ToString() ?? "activo";

            try
            {
                bool exito = _repositorio.Actualizar(_cliente);
                if (exito)
                {
                    SeGuardo = true;
                    MessageBox.Show("Cliente actualizado correctamente.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el cliente.", "Error",
                        MessageBoxButton.OK, Mess