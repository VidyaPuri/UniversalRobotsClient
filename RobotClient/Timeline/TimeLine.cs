using Caliburn.Micro;
using System;

namespace RobotInterface.Timeline
{
	public class TimeLine
	{
		public BindableCollection<TimeLineEvent> Events { get; set; } = new BindableCollection<TimeLineEvent>();
		public TimeSpan Duration { get; } = new TimeSpan(0, 0, 20);
		public string Name { get; set; }
	}
}
