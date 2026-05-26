using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;

namespace ByleAdministration.Servicios
{
    public enum TipoAcceso { Entrada, Salida }

    public class ServicioAcceso
    {
        private readonly RepositorioAcceso _repo = new RepositorioAcceso();
        private readonly RepositorioCliente _repoCliente = new RepositorioCliente();

        /// <summary>
        /// Detecta automáticamente si el siguiente acceso debe ser entrada o salida:
        /// si el cliente ya tiene una entrada abierta hoy → salida; si no → entrada.
        /// </summary>
        public TipoAcceso DetectarTipo(int idUsuario)
            => _repo.EstaAdentro(idUsuario) ? TipoAcceso.Salida : TipoAcceso.Entrada;

        /// <summary>
        /// Registra el acceso y devuelve el tipo que se aplicó.
        /// </summary>
        public TipoAcceso RegistrarAcceso(int idUsuario, string metodo = "huella")
        {
            var tipo = DetectarTipo(idUsuario);
            if (tipo == TipoAcceso.Entrada)
                _repo.RegistrarEntrada(idUsuario, metodo);
            else
                _repo.RegistrarSalida(idUsuario);
            return tipo;
        }

        public bool EstaAdentro(int idUsuario) => _repo.EstaAdentro(idUsuario);
        public int PersonasDentro() => _repo.ContarPersonasDentro();
        public int EntradasHoy() => _repo.ContarEntradasHoy();
        public int SalidasHoy() => _repo.ContarSalidasHoy();
        public System.Collections.Generic.List<RegistroAcceso> BitacoraHoy(int n = 50)
            => _repo.ObtenerBitacoraHoy(n);

        /// <summary>
        /// Verifica si la membresía del cliente está vigente.
        /// </summary>
        public bool MembresiasVigente(Cliente c)
            => c.FechaRenovacion == null || c.FechaRenovacion.Value >= System.DateTime.Today;

        /// <summary>
        /// true si la membresía vence en los próximos <paramref name="dias"/> días.
        /// </summary>
        public bool ProximoAVencer(Cliente c, int dias = 7)
        {
            if (c.FechaRenovacion == null) return false;
            var restantes = (c.FechaRenovacion.Value - System.DateTime.Today).TotalDays;
            return restantes >= 0 && restantes <= dias;
        }
    }
}