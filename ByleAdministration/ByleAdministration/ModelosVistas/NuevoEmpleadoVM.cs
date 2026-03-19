using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using ByleAdministration.Soportes;
using System.Collections.Generic;

namespace ByleAdministration.ModelosVistas
{
    public class NuevoEmpleadoVM : VistaModeloBase
    {
        private readonly RepositorioRol _rolRepository;

        private List<Roles> _roles;
        public List<Roles> Roles
        {
            get => _roles;
            set { _roles = value; OnPropertyChanged(); }
        }

        private Roles _rolSeleccionado;
        public Roles RolSeleccionado
        {
            get => _rolSeleccionado;
            set { _rolSeleccionado = value; OnPropertyChanged(); }
        }

        public NuevoEmpleadoVM()
        {
            _rolRepository = new RepositorioRol();  // ← sin parámetros
            CargarRoles();
        }

        private void CargarRoles()
        {
            Roles = _rolRepository.ObtenerTodos();
        }
    }
}