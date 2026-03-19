using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ByleAdministration.Soportes
{
    public class VistaModeloBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string nombrePropiedad = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
        }
    }
}