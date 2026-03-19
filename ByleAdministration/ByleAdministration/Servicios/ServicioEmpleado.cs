using ByleAdministration.Modelos;
using ByleAdministration.Repositorios;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByleAdministration.Servicios
{
    public class ServicioEmpleado
    {
        private RepositorioEmpleado _repositorio = new RepositorioEmpleado();
        public bool Registrar(Empleados empleado)
        {
            Empleados existente = _repositorio.ObtenerPorCorreo(empleado.Correo);

            if (existente != null)
            {
                return false;
            }
            return _repositorio.Insertar(empleado);
        }
   
        public bool Login(string correo, string contrasena)
        {
            Empleados empleado = _repositorio.ObtenerPorCorreo(correo);

            if (empleado == null) return false;

            return empleado.ContraseñaHash == contrasena;
        }
    }
}
