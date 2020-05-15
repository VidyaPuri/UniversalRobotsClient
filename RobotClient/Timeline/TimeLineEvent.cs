using System;

namespace RobotInterface.Timeline
{
    public class TimeLineEvent
    {
		public TimeSpan Duration { get; set; }
		public TimeSpan Start { get; set; }
		public string Name { get; set; }
	}
}
