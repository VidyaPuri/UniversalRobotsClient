using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface.Timeline
{
    public class TimeLineEvent
    {
		public TimeSpan Start { get; set; }
		public TimeSpan Duration { get; set; }
		public string Name { get; set; }
	}
}
