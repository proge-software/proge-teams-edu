using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Proge.Teams.Edu.DAL.Entities
{
    public class Log 
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string MessageTemplate { get; set; }
        public string Level { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public string Exception { get; set; }
        public string Properties { get; set; }        
        public XElement PropertiesWrapper
        {
            get { return XElement.Parse(Properties); }
            set { Properties = value.ToString(); }
        }
        public string LogEvent { get; set; }

    }
}
