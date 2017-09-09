using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DesktopManagerApplication.ViewModels
{
    public class ParameterlessDelegateCommand : ICommand
    {
        #region Life Cycle

        public ParameterlessDelegateCommand(Action action, bool canExecute = true)
        {
            this.action = action ?? throw new ArgumentNullException("action");
            this.canExecute = canExecute;
        }

        #endregion

        #region ICommand

        public event EventHandler CanExecuteChanged;

        private bool canExecute = true;

        public bool CanExecute(object parameter)
        {
            return canExecute;
        }

        private Action action;

        public void Execute(object parameter)
        {
            action();
        }

        #endregion
    }
}
