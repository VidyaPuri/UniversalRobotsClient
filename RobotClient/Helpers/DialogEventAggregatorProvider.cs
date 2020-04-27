using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface.Helpers
{
    public static class DialogEventAggregatorProvider
    {
        public static EventAggregator EG { get; set; } = new EventAggregator();

    }
}
