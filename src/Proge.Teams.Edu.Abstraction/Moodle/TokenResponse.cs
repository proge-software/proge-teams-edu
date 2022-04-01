using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.Abstraction.Moodle
{
    public class TokenResponse : MoodleResponseBase
    {
        public string token { get; set; }
        public string privatetoken { get; set; }
    }
}
