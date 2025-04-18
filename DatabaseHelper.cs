using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace ForgetNot_Calendar
{

    
    public class DatabaseHelper
    {
        private string connectionString = "server=csitmariadb.eku.edu;user=student;password=Maroon@21?;database=csc340_db;";


        public DatabaseHelper()
        {
            InitializeDatabase();
        }
        
        //open a connection to the database
        private MySqlConnection OpenConnection()
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        //Initialize database (Use CREATE TABLE IF NOT EXISTS)
        private void InitializeDatabase()
        {
            using (var connection = OpenConnection())
            {
                var createTableQuery = @"
            
                CREATE TABLE IF NOT EXISTS manguscalendarevents (
                eventid INT AUTO_INCREMENT PRIMARY KEY,
                eventtitle VARCHAR(255) NOT NULL,
                eventdate DATE NOT NULL,
                starttime TIME,
                endtime TIME,
                eventdescription TEXT
                );";

                using (var createCmd = new MySqlCommand(createTableQuery, connection))
                {
                    createCmd.ExecuteNonQuery();
                }
            }
        }


        //Add event method passing title, date, starttime, endtime, description
        public void AddEvent(string title, DateTime eventDate, string description, TimeSpan? startTime = null, TimeSpan? endTime = null)
        {
            using (var connection = OpenConnection())
            {
                //Add event query
                string query = @"INSERT INTO manguscalendarevents (eventtitle, eventdate, starttime, endtime, eventdescription) 
            VALUES (@Title, @EventDate, @StartTime, @EndTime, @Description)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Title", title);
                    cmd.Parameters.AddWithValue("@EventDate", eventDate.Date);
                    cmd.Parameters.AddWithValue("@StartTime", startTime.HasValue ? (object)startTime.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndTime", endTime.HasValue ? (object)endTime.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        //Geteventsbyday method (more detailed) retrieves all data from event using eventdate
        public List<CalendarEvent> GetEventsByDay(int year, int month, int day)
        {
            List<CalendarEvent> events = new List<CalendarEvent>();
            try
            {
                using (var connection = OpenConnection())
                {
                    //Geteventsbyday query
                    string query = @"SELECT eventid, eventtitle, eventdate, starttime, endtime, eventdescription FROM manguscalendarevents WHERE eventdate = @EventDate";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        DateTime eventDate = new DateTime(year, month, day);
                        
                        cmd.Parameters.AddWithValue("@EventDate", new DateTime(year, month, day));

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                events.Add(new CalendarEvent
                                {
                                    EventID = reader.GetInt32("eventid"),
                                    EventTitle = reader.GetString("eventtitle"),
                                    EventDate = reader.GetDateTime("eventdate"),
                                    StartTime = reader.IsDBNull(reader.GetOrdinal("starttime")) ? (TimeSpan?)null : reader.GetTimeSpan("starttime"),
                                    EndTime = reader.IsDBNull(reader.GetOrdinal("endtime")) ? (TimeSpan?)null : reader.GetTimeSpan("endtime"),
                                    EventDescription = reader.IsDBNull(reader.GetOrdinal("eventdescription")) ? null : reader.GetString("eventdescription")
                                }); ;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors with error message
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return events;
        }


        // Method to get events by month (pull eventID from database where date is in specified month)
        public List<int> GetEventIDsByMonth(int year, int month)
        {
            var eventIDs = new List<int>();
            using (var connection = OpenConnection())
            {
                //GeteventIDsbymonth query
                string query = "SELECT eventid FROM manguscalendarevents WHERE YEAR(eventdate) = @Year AND MONTH(eventdate) = @Month";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Month", month);
                   

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            eventIDs.Add(reader.GetInt32("eventid"));
                        }
                    }
                }
            }
            return eventIDs;
        }


        //Update event method
        public void UpdateEvent(CalendarEvent calendarEvent)
        {
            using (var connection = OpenConnection())
            {
                //Update event query
                string query = @"UPDATE manguscalendarevents SET eventtitle = @Title, eventdate = @EventDate, starttime = @StartTime, endtime = @EndTime, 
                eventdescription = @Description WHERE eventid = @EventID";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@EventID", calendarEvent.EventID);
                    cmd.Parameters.AddWithValue("@Title", calendarEvent.EventTitle);
                    cmd.Parameters.AddWithValue("@EventDate", calendarEvent.EventDate);
                    cmd.Parameters.AddWithValue("@StartTime", calendarEvent.StartTime.HasValue ? (object)calendarEvent.StartTime.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndTime", calendarEvent.EndTime.HasValue ? (object)calendarEvent.EndTime.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", calendarEvent.EventDescription);

                    cmd.ExecuteNonQuery();
                }
            }
        }



        //Delete event method
        public void DeleteEvent(int eventId)
        {
            using (var connection = OpenConnection())
            {
                //Delete event query
                var query = "DELETE FROM manguscalendarevents WHERE eventid = @EventID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventId);
                    int result = cmd.ExecuteNonQuery();

                    //check the result to see how many rows were affected
                    if (result == 0)
                    {
                        throw new Exception("No record was deleted. Check the EventID.");
                    }
                }
            }
        }

        //Method that retrieves an event from clicking on day button
        public CalendarEvent GetEventByID(int eventID)
        {
            using (var connection = OpenConnection())
            {
                string query = "SELECT eventid, eventtitle, eventdate, starttime, endtime, eventdescription FROM manguscalendarevents WHERE eventid = @EventID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@EventID", eventID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new CalendarEvent
                            {
                                EventID = reader.GetInt32("eventid"),
                                EventTitle = reader.GetString("eventtitle"),
                                EventDate = reader.GetDateTime("eventdate"),
                                StartTime = reader.IsDBNull(reader.GetOrdinal("starttime")) ? (TimeSpan?)null : reader.GetTimeSpan("starttime"),
                                EndTime = reader.IsDBNull(reader.GetOrdinal("endtime")) ? (TimeSpan?)null : reader.GetTimeSpan("endtime"),
                                EventDescription = reader.IsDBNull(reader.GetOrdinal("eventdescription")) ? null : reader.GetString("eventdescription"),
                            };
                        }
                        // Handle case where event with given ID is not found
                        return null;
                    }
                }
            }
        }
        //haseventsonday (instead of month) (simpler faster check) passing year, month, day
        public bool HasEventsOnDay(int year, int month, int day)
        {
            using (var connection = OpenConnection())
            {
                //query to check if event or not for day
                string query = "SELECT COUNT(*) FROM manguscalendarevents WHERE YEAR(eventdate) = @Year AND MONTH(eventdate) = @Month AND DAY(eventdate) = @Day";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Month", month);
                    cmd.Parameters.AddWithValue("@Day", day);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        //get sorted events by month for monthly view form passed month/year
        public List<CalendarEvent> GetSortedEventsByMonth(int year, int month)
        {
            List<CalendarEvent> sortedEvents = new List<CalendarEvent>();

            using (var connection = OpenConnection())
            {
                //query for all information about event by month/year
                string query = @"SELECT eventid, eventtitle, eventdate, starttime, endtime, eventdescription FROM manguscalendarevents WHERE YEAR(eventdate) = @Year AND MONTH(eventdate) = @Month ORDER BY eventdate, starttime, endtime";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Year", year);
                    cmd.Parameters.AddWithValue("@Month", month);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var eventDetails = new CalendarEvent
                            {
                                EventID = reader.GetInt32("eventid"),
                                EventTitle = reader.GetString("eventtitle"),
                                EventDate = reader.GetDateTime("eventdate"),
                                StartTime = reader.IsDBNull(reader.GetOrdinal("starttime")) ? null : (TimeSpan?)reader.GetTimeSpan("starttime"),
                                EndTime = reader.IsDBNull(reader.GetOrdinal("endtime")) ? null : (TimeSpan?)reader.GetTimeSpan("endtime"),
                                EventDescription = reader.IsDBNull(reader.GetOrdinal("eventdescription")) ? "" : reader.GetString("eventdescription")
                            };
                            sortedEvents.Add(eventDetails);
                        }
                    }
                }
            }
            return sortedEvents;

        }
    
        //event conflict checker being passed eventdate, starttime, and endtime 
        public bool HasEventConflict(DateTime eventDate, TimeSpan startTime, TimeSpan endTime)
        {
            using (var connection = OpenConnection())
            {
                string query = @"SELECT COUNT(*) FROM manguscalendarevents WHERE eventdate = @EventDate AND (starttime < @EndTime AND endtime > @StartTime)";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@EventDate", eventDate.Date);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        

    }

}

