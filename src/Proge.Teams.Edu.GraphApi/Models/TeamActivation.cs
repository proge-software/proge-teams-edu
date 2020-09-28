using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.GraphApi.Models
{
    public class Value
    {
        public string status { get; set; }
    }

    public class TeamActivation
    {
        public Value value { get; set; }
        public string type { get; set; }
    }
}
