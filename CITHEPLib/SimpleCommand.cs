using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CITHEPLib
{

    public class SimpleCommand<T> : ICommand
    {
        public Predicate<T> CanExecuteDelegate = o => true;
        public Action<T> ExecuteDelegate { get; set; }
        public bool Async = false;
        private BackgroundWorker bw = null;
        public SimpleCommand(Action<T> Execute=null, Predicate<T> CanExecute = null, bool Async=false)
        {
            this.ExecuteDelegate = Execute;
            if(CanExecute!=null) this.CanExecuteDelegate = CanExecute;
            this.Async = Async;
            if (this.Async)
            {
                bw = new BackgroundWorker();
                bw.DoWork += (s, e) =>
                {
                    ExecuteDelegate((T)e.Argument);
                    
                };
                bw.RunWorkerCompleted += (s, e) =>
                {
                    CommandManager.InvalidateRequerySuggested();
                };
            }
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            if (Async && bw.IsBusy) { return false; }//prevent double click
            if (CanExecuteDelegate != null)
                return CanExecuteDelegate((T)parameter);
            return true;// if there is no can execute default to true
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteDelegate != null)
            {
                if (!Async)
                {
                    ExecuteDelegate((T)parameter);
                }
                else
                {
                    bw.RunWorkerAsync(parameter);
                }
            }
        }

        #endregion
    }
}
