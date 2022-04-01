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

    public class OnlineMeetingOptionEditRequest
    {
        public OnlineMeetingOptionEditRequestItem[] options { get; set; }
    }

    public class OnlineMeetingOptionEditRequestItem
    {
        public string currentValue { get; set; }
        public string name { get; set; }
        public string type { get; set; }

    }

    public class OnlineMeetingOptionEditResponse
    {
        public bool? success { get; set; }
    }
}
