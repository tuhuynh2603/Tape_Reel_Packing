using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TapeReelPacking.UI
{
    public class CommandBase:ICommand
    {
        public event EventHandler CanExecuteChanged;
        private Action<string> _execute;

        public CommandBase(Action<string> execute)
        {
            _execute = execute;
        }

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }
        public  void Execute(object parameter)
        {
            _execute.Invoke(parameter as string);
        }

        protected void OnCanExecutedChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
