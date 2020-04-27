using Caliburn.Micro;

namespace RobotInterface.Helpers
{
    public static class DialogEventAggregatorProvider
    {
        public static EventAggregator EG { get; set; } = new EventAggregator();
    }
}
