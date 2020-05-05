using System.Windows.Media.Animation;
using System.Windows.Controls;
using RobotInterface.Helpers;
using Caliburn.Micro;
using System;

namespace RobotInterface.Views
{
    /// <summary>
    /// Interaction logic for TimelineView.xaml
    /// </summary>
    public partial class TimelineView : UserControl
    {
        public TimelineView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// TimeLine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void CurrentTimeInvalidatedEventHandler(object sender, EventArgs eventArgs)
        {
            Clock time = (Clock)sender;
            DialogEventAggregatorProvider.EA.PublishOnUIThread(time);
        }
    }
}
