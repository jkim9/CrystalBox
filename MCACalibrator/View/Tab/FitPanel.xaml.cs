using CITHEPLib;
using CITHEPLib.PeakLocator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCACalibrator.View.Tab
{
    /// <summary>
    /// Interaction logic for FitPanel.xaml
    /// Sorry this is quite a mess...I need to finish it before leaving
    /// </summary>
    public partial class FitPanel : UserControl
    {
        public PeakFinderSetting Channel0PeakFinderSetting { get; set; }
        public PeakFinderSetting Channel1PeakFinderSetting { get; set; }
        public SimpleCommand<object> DebugPrint { get; private set; }
        public SimpleCommand<InitialPeakGuess> UsePeakGuessForChannel0Command { get; private set; }
        public SimpleCommand<InitialPeakGuess> UsePeakGuessForChannel1Command { get; private set; }
        public FitPanel()
        {
            Channel0PeakFinderSetting = new PeakFinderSetting() { Channel = 0 };
            Channel1PeakFinderSetting = new PeakFinderSetting() { Channel = 1 };
            DebugPrint = new SimpleCommand<object>(o => Debug.WriteLine(o));
            UsePeakGuessForChannel0Command = new SimpleCommand<InitialPeakGuess>(o => UsePeakGuess(o, 0));
            UsePeakGuessForChannel1Command = new SimpleCommand<InitialPeakGuess>(o => UsePeakGuess(o, 1));
            InitializeComponent();
        }

        public void UsePeakGuess(InitialPeakGuess guess, int channel){
            if (guess == null) { return; }
            ExperimentalRun run = DataContext as ExperimentalRun;
            if (channel == 0)
            {
                run.Log.Channel0InitialPeakGuess = new InitialPeakGuess(guess);
            }
            else if (channel == 1)
            {
                run.Log.Channel1InitialPeakGuess = new InitialPeakGuess(guess);
            }
        }
    }
}
