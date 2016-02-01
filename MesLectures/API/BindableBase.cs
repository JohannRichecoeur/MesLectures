using System.ComponentModel;

namespace MesLectures.API
{
    /// <summary>
    /// Implémentation de <see cref="INotifyPropertyChanged"/> pour simplifier les modèles.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Événement multidiffusion pour les notifications de modification de propriétés.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}