using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class LatestLog
    {      
        public string Message { get; set; }       
        public string Level { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Exception { get; set; }
    }
}
