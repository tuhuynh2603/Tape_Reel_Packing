using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class PaneVM: INotifyPropertyChanged
    {
        public string Title { get; set; }
        public ObservableCollection<DocumentVM> Documents { get; set; }

        public PaneVM(string title, List<DocumentVM> documents)
        {
            Title = title;
            Documents = new ObservableCollection<DocumentVM>(documents);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
