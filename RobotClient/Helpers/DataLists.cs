using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotInterface.Helpers
{
    public static class DataLists
    {
        /// <summary>
        /// Returns the list of baud rates
        /// </summary>
        /// <returns></returns>
        public static string[] GetBaudRates()
        {
            string[] output = { "300", "1200", "2400", "4800", "9600", "19200", "38400", "57600", "1000000" };

            return output;
        }

        /// <summary>
        /// Returns the list of step types on stepper motor
        /// </summary>
        /// <returns></returns>
        public static string[] GetStepTypes()
        {
            string[] output = { "Single", "Double", "Interleave", "Microstep" };

            return output;
        }
    }
}
