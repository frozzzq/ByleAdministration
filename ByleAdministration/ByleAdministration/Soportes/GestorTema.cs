using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Windows;


namespace ByleAdministration.Soportes
{
    public enum TemaActual
    {
        Oscuro,
        Claro
    }

    public static class GestorTema
    {
        private static TemaActual _temaActual = TemaActual.Oscuro;

        public static TemaActual TemaActual
        {
            get => _temaActual;
            set
            {
                if (_temaActual != value)
                {
                    _temaActual = value;
                    AplicarTema(value);
                }
            }
        }

        /// <summary>
        /// Cambia dinámicamente el tema de toda la aplicación (GLOBAL)
        /// </summary>
        private static void AplicarTema(TemaActual tema)
        {
            var resourceDictionary = new ResourceDictionary();

            // Cargar el diccionario de tema correcto
            if (tema == TemaActual.Oscuro)
            {
                resourceDictionary.Source = new Uri("Recursos/Estilos/TemaOscuro.xaml", UriKind.Relative);
            }
            else // Claro
            {
                resourceDictionary.Source = new Uri("Recursos/Estilos/TemaClaro.xaml", UriKind.Relative);
            }

            // Buscar y reemplazar el tema anterior en los diccionarios de la app
            var dictsCambiar = Application.Current.Resources.MergedDictionaries;

            // El diccionario de tema debe estar en posición 1 (después de Colores.xaml)
            if (dictsCambiar.Count > 1)
            {
                // Verificar si ya es un diccionario de tema (contiene BrushBgPrimary)
                if (dictsCambiar[1].Contains("BrushBgPrimary"))
                {
                    dictsCambiar.RemoveAt(1);
                    dictsCambiar.Insert(1, resourceDictionary);
                    return;
                }
            }

            // Si no encontramos, agregarlo después de Colores (seguridad)
            dictsCambiar.Insert(1, resourceDictionary);
        }

        /// <summary>
        /// Obtiene el tema actual como string para debugging
        /// </summary>
        public static string ObtenerTemaActual()
        {
            return _temaActual == TemaActual.Oscuro ? "Oscuro" : "Claro";
        }

        /// <summary>
        /// Toggle del tema (cambiar entre oscuro y claro)
        /// </summary>
        public static void AlternarTema()
        {
            TemaActual = _temaActual == TemaActual.Oscuro ? TemaActual.Claro : TemaActual.Oscuro;
        }
    }
}