using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;

namespace ByleAdministration.Servicios
{
    /// <summary>
    /// Lógica de negocio para clientes del gimnasio.
    /// Coordina entre <see cref="RepositorioCliente"/> y el resto de la app.
    /// </summary>
    public class ServicioCliente
    {
        private readonly RepositorioCliente _repoCliente = new RepositorioCliente();
        private readonly RepositorioMembresia _repoMembresia = new RepositorioMembresia();

        // ─────────────────────────────────────────────────────────────
        // REGISTRO Y EDICIÓN
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Registra un nuevo cliente.
        /// Asigna estado "activo" y calcula fecha_renovacion según la membresía.
        /// </summary>
        /// <returns>ID del nuevo cliente generado por la BD.</returns>
        public int Registrar(Cliente cliente)
        {
            cliente.Estado = "activo";
            cliente.FechaInscripcion = DateTime.Now;

            // Calcular fecha de renovación basada en la membresía seleccionada
            Membresia plan = _repoMembresia.ObtenerPorId(cliente.IdMembresia);
            if (plan != null)
                cliente.FechaRenovacion = cliente.FechaInscripcion.AddDays(plan.DuracionDias);

            return _repoCliente.Insertar(cliente);
        }

        /// <summary>
        /// Actualiza los datos del cliente existente.
        /// Recalcula fecha_renovacion si cambió la membresía.
        /// </summary>
        public bool Actualizar(Cliente cliente)
        {
            return _repoCliente.Actualizar(cliente);
        }

        /// <summary>Da de baja un cliente (estado = "inactivo").</summary>
        public bool DarDeBaja(int idUsuario)
            => _repoCliente.CambiarEstado(idUsuario, "inactivo");

        /// <summary>Reactiva un cliente (estado = "activo").</summary>
        public bool Reactivar(int idUsuario)
            => _repoCliente.CambiarEstado(idUsuario, "activo");

        // ─────────────────────────────────────────────────────────────
        // CONSULTAS
        // ─────────────────────────────────────────────────────────────

        public Cliente Obtener(int id)
            => _repoCliente.ObtenerPorId(id);

        public List<Cliente> ObtenerTodos()
            => _repoCliente.ObtenerTodos();

        public List<Cliente> ObtenerActivos()
            => _repoCliente.ObtenerActivos();

        public List<Cliente> Buscar(string termino)
            => _repoCliente.BuscarPorNombre(termino);

        /// <summary>Clientes cuya membresía vence en los próximos <paramref name="dias"/> días.</summary>
        public List<Cliente> ObtenerPorVencer(int dias = 7)
            => _repoCliente.ObtenerPorVencer(dias);

        // ─────────────────────────────────────────────────────────────
        // MEMBRESÍAS (para combos y selects)
        // ─────────────────────────────────────────────────────────────

        public List<Membresia> ObtenerMembresiasActivas()
            => _repoMembresia.ObtenerActivas();
    }
}