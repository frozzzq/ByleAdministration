using System;
using System.Windows;

namespace ByleAdministration.Soportes
{
    public enum TemaActual
    {
        Oscuro,
        Claro
    }

    /// <summary>
    /// Gestor de temas dinámico para la aplicación
    /// Cambia todos los ResourceDictionary de tema instantáneamente
    /// </summary>
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
                    AplicarTemaGlobalmente(value);
                }
            }
        }

        /// <summary>
        /// Aplica el tema a TODA la aplicación
        /// Reemplaza el diccionario de tema en App.xaml MergedDictionaries
        /// </summary>
        private static void AplicarTemaGlobalmente(TemaActual tema)
        {
            try
            {
                string uriTema = tema == TemaActual.Oscuro
                    ? "Recursos/Estilos/TemaOscuro.xaml"
                    : "Recursos/Estilos/TemaClaro.xaml";

                // Crear nuevo diccionario de tema
                var nuevoTema = new ResourceDictionary
                {
                    Source = new Uri(uriTema, UriKind.Relative)
                };

                var dicts = Application.Current.Resources.MergedDictionaries;

                // Buscar el diccionario de tema (está en posición 1, después de Colores.xaml)
                // Estructura esperada:
                // [0] = Colores.xaml
                // [1] = TemaOscuro.xaml O TemaClaro.xaml
                // [2] = Estilos.xaml

                bool encontrado = false;

                for (int i = 0; i < dicts.Count; i++)
                {
                    // Verificar si es un diccionario de tema (contiene brushes de tema)
                    if (dicts[i].Contains("BrushBgPrimary") ||
                        dicts[i].Contains("primaryBackColor1") ||
                        dicts[i].Source?.OriginalString?.Contains("Tema") == true)
                    {
                        dicts.RemoveAt(i);
                        dicts.Insert(i, nuevoTema);
                        encontrado = true;
                        break;
                    }
                }

                // Si no encontramos, insertarlo en posición 1 (seguridad)
                if (!encontrado && dicts.Count > 0)
                {
                    dicts.Insert(1, nuevoTema);
                }

                // Log para debugging
                System.Diagnostics.Debug.WriteLine($"[GestorTema] Tema cambiado a: {tema}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GestorTema] Error al aplicar tema: {ex.Message}");
            }
        }

        /// <summary>
        /// Alterna entre tema oscuro y claro
        /// </summary>
        public static void AlternarTema()
        {
            TemaActual = _temaActual == TemaActual.Oscuro ? TemaActual.Claro : TemaActual.Oscuro;
        }

        /// <summary>
        /// Obtiene el tema actual como string
        /// </summary>
        public static string ObtenerTemaActual()
        {
            return _temaActual == TemaActual.Oscuro ? "Oscuro" : "Claro";
        }
    }
}