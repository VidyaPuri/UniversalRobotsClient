using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface.Timeline
{
    public class TimeLineEvent
    {
		private TimeSpan _Start;
		private TimeSpan _Duration;
		private string _Name;

		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		public TimeSpan Start
		{
			get { return _Start; }
			set { _Start = value; }
		}

		public TimeSpan Duration
		{
			get { return _Duration; }
			set { _Duration = value; }
		}
	}
}
