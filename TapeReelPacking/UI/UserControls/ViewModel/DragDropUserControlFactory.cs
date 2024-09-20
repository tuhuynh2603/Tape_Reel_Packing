using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TapeReelPacking.Source.Model;
using TapeReelPacking.Source.Repository;
using TapeReelPacking.UI.UserControls.View;

namespace TapeReelPacking.UI.UserControls.ViewModel
{
    public class DragDropUserControlVM : DragDropUserControlAbstract
    {

        // Constructor for ViewModels without MainWindowVM or DatabaseContext
        public DragDropUserControlVM(Func<DragDropUserControlVM, BaseVM> initModelFcn)
        {
            CurrentViewModel = initModelFcn.Invoke(this);
        }

        // Constructor for ViewModels requiring MainWindowVM
        public DragDropUserControlVM(Func<DragDropUserControlVM, MainWindowVM, BaseVM> initModelFcn, MainWindowVM mainWindowVM)
            // CurrentViewModel = initModelFcn.Invoke(this);
        {
            CurrentViewModel = initModelFcn.Invoke(this, mainWindowVM);
        }

        public DragDropUserControlVM(Func<DragDropUserControlVM, DatabaseContext, BaseVM> initModelFcn, DatabaseContext db = null)
        {
            CurrentViewModel = initModelFcn.Invoke(this, db);
        }

        // Constructor for ViewModels requiring MainWindowVM and DatabaseContext
        public DragDropUserControlVM(Func<DragDropUserControlVM, MainWindowVM, DatabaseContext, BaseVM> initModelFcn, MainWindowVM mainWindowVM, DatabaseContext db = null)
        {
            CurrentViewModel = initModelFcn.Invoke(this, mainWindowVM, db);
        }
    }
}
