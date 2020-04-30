using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface.Timeline
{
    public class TimeLine
    {
		private BindableCollection<TimeLineEvent> _Events = new BindableCollection<TimeLineEvent>();
		private TimeSpan _Duration;

		public TimeSpan Duration
		{
			get { return _Duration; }
			set { _Duration = value; }
		}

		public BindableCollection<TimeLineEvent> Events
		{
			get { return _Events; }
			set { _Events = value; }
		}
	}
}
