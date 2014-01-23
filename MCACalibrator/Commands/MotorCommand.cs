using MCACalibrator.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MCACalibrator.Commands
{
    
    public class MoveToCM : ICommand
    {
        MainViewModel mvm;
        public MoveToCM(MainViewModel mvm){
            this.mvm = mvm;
        }
        public bool CanExecute(object parameter)
        {
            return mvm.LinearMotor.IsConnected();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            double target = Convert.ToDouble(parameter);
            mvm.LinearMotor.MoveTocm(target);
        }
    }
    public class ToggleMotorPower : ICommand
    {
        public ToggleMotorPower(MainViewModel mvm)
        {
            this.mvm = mvm;
        }
        MainViewModel mvm;
        public bool CanExecute(object parameter)
        {
            return mvm.LinearMotor.IsConnected();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            mvm.LinearMotor.SetMotorPower(mvm.LinearMotor.MotorPower() ? 0 : 1);
        }
    }
    public class CalibrateMotor : ICommand
    {
        MainViewModel mvm;
        public CalibrateMotor(MainViewModel mvm)
        {
            this.mvm = mvm;
        }


        public bool CanExecute(object parameter)
        {
            return mvm.LinearMotor.IsConnected();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            mvm.LinearMotor.Calibrate();
        }
    }
    public class StopMotor : ICommand
    {
        MainViewModel mvm;
        public StopMotor(MainViewModel mvm)
        {
            this.mvm = mvm;
        }

        public bool CanExecute(object parameter)
        {
            return mvm.LinearMotor.IsConnected() && mvm.LinearMotor.IsRunning();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            mvm.LinearMotor.Stop();
        }
    }


    public class MotorCommand
    {
        MainViewModel mvm;
        public MoveToCM MoveToCM { get; private set; }
        public ToggleMotorPower ToggleMotorPower { get; private set; }
        public CalibrateMotor CalibrateMotor { get; private set; }
        public StopMotor StopMotor { get; private set; }

        public MotorCommand(MainViewModel mvm)
        {
            this.mvm = mvm;
            MoveToCM = new MoveToCM(mvm);
            ToggleMotorPower = new ToggleMotorPower(mvm);
            CalibrateMotor = new CalibrateMotor(mvm);
            StopMotor = new StopMotor(mvm);
        }
    }
}
