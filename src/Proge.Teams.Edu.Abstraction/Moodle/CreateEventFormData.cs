using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Proge.Teams.Edu.Abstraction.Moodle;
public class CreateEventFormData
{
    public CreateEventFormData() { }
    public CreateEventFormData(int Id, string Name, DateTime startDateTime, string Description) : this(Name, startDateTime, Description)
    {
        id = Id;
        qf__core_calendar_local_event_forms_update = id == 0 ? 0 : 1;
        qf__core_calendar_local_event_forms_create = id == 0 ? 1 : 0;
    }
    public CreateEventFormData(string Name, DateTime startDateTime, string Description)
    {
        userid = 2;
        visible = 1;
        instance = 0;
        modulename = "scorm";
        mform_showmore_id_general = 1;
        name = Name;
        timestart_year = startDateTime.Year;
        timestart_month = startDateTime.Month;
        timestart_day = startDateTime.Day;
        timestart_hour = startDateTime.Hour;
        timestart_minute = startDateTime.Minute;
        eventtype = "site";
        description_text = Description;
        description_format = 1;
        duration = 0;
        location = string.Empty;
    }

    public string Serialize()
    {        
        description_text = PercentEncode(description_text);
        string qString = $"id={id}&repeat=0&userid={userid}&instance=0&visible={visible}&_qf__core_calendar_local_event_forms_create={qf__core_calendar_local_event_forms_create}&_qf__core_calendar_local_event_forms_update={qf__core_calendar_local_event_forms_update}&mform_showmore_id_general={mform_showmore_id_general}&name={name}&timestart%5Bday%5D={timestart_day}&timestart%5Bmonth%5D={timestart_month}&timestart%5Byear%5D={timestart_year}&timestart%5Bhour%5D={timestart_hour}&timestart%5Bminute%5D={timestart_minute}&eventtype={eventtype}&description%5Btext%5D={description_text}&description%5Bformat%5D={description_format}&location={location}&duration={duration}";
        qString = HttpUtility.UrlEncode(qString);
        return qString;
    }

    public static string PercentEncode(string value)
    {
        StringBuilder retval = new StringBuilder();
        foreach (char c in value)
        {
            if ((c >= 48 && c <= 57) || //0-9  
                (c >= 65 && c <= 90) || //a-z  
                (c >= 97 && c <= 122) || //A-Z                    
                (c == 45 || c == 46 || c == 95 || c == 126)) // period, hyphen, underscore, tilde  
            {
                retval.Append(c);
            }
            else
            {
                retval.AppendFormat("%{0:X2}", ((byte)c));
            }
        }
        return retval.ToString();
    }

    public int id { get; set; }
    public int userid { get; set; }
    public string modulename { get; set; }
    public int instance { get; set; }
    public int visible { get; set; }
    public string sesskey { get; set; }
    public int qf__core_calendar_local_event_forms_create { get; set; }
    public int qf__core_calendar_local_event_forms_update { get; set; }
    public int mform_showmore_id_general { get; set; }
    public string name { get; set; }
    public int timestart_day { get; set; }
    public int timestart_month { get; set; }
    public int timestart_year { get; set; }
    public int timestart_hour { get; set; }
    public int timestart_minute { get; set; }
    public string eventtype { get; set; }
    public string description_text { get; set; }
    public int description_format { get; set; }
    public string location { get; set; }
    public int duration { get; set; }
}
public class Icon
{
    public string key { get; set; }
    public string component { get; set; }
    public string alttext { get; set; }
}

public class Subscription
{
    public bool displayeventsource { get; set; }
}

public class CalendarEvent
{
    public int id { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public int descriptionformat { get; set; }
    public string location { get; set; }
    public object categoryid { get; set; }
    public object groupid { get; set; }
    public int userid { get; set; }
    public object repeatid { get; set; }
    public object eventcount { get; set; }
    public object component { get; set; }
    public string modulename { get; set; }
    public object instance { get; set; }
    public string eventtype { get; set; }
    public int timestart { get; set; }
    public int timeduration { get; set; }
    public int timesort { get; set; }
    public int timeusermidnight { get; set; }
    public int visible { get; set; }
    public int timemodified { get; set; }
    public Icon icon { get; set; }
    public Subscription subscription { get; set; }
    public bool canedit { get; set; }
    public bool candelete { get; set; }
    public string deleteurl { get; set; }
    public string editurl { get; set; }
    public string viewurl { get; set; }
    public string formattedtime { get; set; }
    public bool isactionevent { get; set; }
    public bool iscourseevent { get; set; }
    public bool iscategoryevent { get; set; }
    public object groupname { get; set; }
    public string normalisedeventtype { get; set; }
    public string normalisedeventtypetext { get; set; }
    public string url { get; set; }
}

public class CalendarEventResponse : MoodleResponseBase
{
    public CalendarEvent @event { get; set; }
   
}

public class CalendarEventsResponse : MoodleResponseBase
{
    public IEnumerable<CalendarEvent> Events { get; set; }

}