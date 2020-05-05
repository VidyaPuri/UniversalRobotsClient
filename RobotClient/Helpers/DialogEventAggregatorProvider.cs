using Caliburn.Micro;
using System.Diagnostics;

namespace RobotInterface.Helpers
{
    public static class DialogEventAggregatorProvider
    {
        public static EventAggregator EA { get; set; } = new EventAggregator();

        public static void Presure()
        {
            EA.BeginPublishOnUIThread("waka");
        }
    }
}
