using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Abstraction.Moodle
{
    public class MoodleExceptionData
    {
        public string exception { get; set; }
        public string errorcode { get; set; }
        public string error { get; set; }
        public string message { get; set; }
        public string debuginfo { get; set; }
        public string stacktrace { get; set; }
        public string reproductionlink { get; set; }
    }
}
