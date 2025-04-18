using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ForgetNot_Calendar
{
    public class CalendarEvent
    {
        // properties of the CalendarEvent class
        public int EventID { get; set; }
        public string EventTitle { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string EventDescription { get; set; }
       


        public CalendarEvent() {

        }

        // constructor that initializes all properties
        public CalendarEvent(int eventId, string title, DateTime eventDate, string description, TimeSpan? startTime, TimeSpan? endTime)
        {
            EventID = eventId;
            EventTitle = title;
            EventDate = eventDate;
            StartTime = startTime;
            EndTime = endTime;
            EventDescription = description;
            
        }
    }


}
