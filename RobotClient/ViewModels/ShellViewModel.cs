using System.Windows.Media.Animation;
using RobotInterface.ViewModels;
using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using System;

namespace RobotClient.ViewModels
{
    public class ShellViewModel : Conductor<Screen>.Collection.AllActive
    {
        #region Window Control

        private WindowState windowState;
        public WindowState WindowState
        {
            get { return windowState; }
            set
            {
                windowState = value;
                NotifyOfPropertyChange(() => WindowState);
            }
        }

        public void MaximizeWindow()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        public void MinimizeWindow()
        {
            WindowState = WindowState.Minimized;
        }

        public bool myCondition { get { return (false); } }

        public void CloseWindow()
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Constructor

        public ShellViewModel(
            NetworkingViewModel networkingViewModel,
            TimelineViewModel timelineViewModel
            )
        {
            // View model screens initialisation
            NetworkingViewModel = networkingViewModel;
            TimelineViewModel = timelineViewModel;
        }

        #endregion

        #region ViewModelInitialisation

        public NetworkingViewModel NetworkingViewModel { get; set; }
        public TimelineViewModel TimelineViewModel { get; set; }

        #endregion

    }
}
