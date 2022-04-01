using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Abstraction.Moodle
{
    public abstract class MoodleResponseBase
    {
        public bool validationerror { get; set; }
        public MoodleExceptionData exception { get; set; }
    }
}
