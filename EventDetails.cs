using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management.Instrumentation;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ForgetNot_Calendar
{
    
    public partial class EventDetails : Form
    {
        private DatabaseHelper _databaseHelper;
        private CalendarEvent _event;
        private MainPage _mainPage;

        // load event details to form from database
        public EventDetails(CalendarEvent calendarEvent, DatabaseHelper databaseHelper, MainPage mainPage)
        {
            InitializeComponent();
            _event = calendarEvent;
            _databaseHelper = databaseHelper;
            _mainPage = mainPage;

            LoadEventDetailsFromDatabase(); // call to database
        }
        //bind event details 
        private void BindEventDetails()
        {
            txtTitle.Text = _event.EventTitle;
            txtDate.Text = _event.EventDate.ToString("yyyy-MM-dd"); 
            txtStartTime.Text = _event.StartTime?.ToString(@"hh\:mm") ?? "";
            txtEndTime.Text = _event.EndTime?.ToString(@"hh\:mm") ?? "";
            txtDescription.Text = _event.EventDescription;
        }

        //load event details that uses database helper to load event details for form
        private void LoadEventDetailsFromDatabase()
        {
            if (_event != null)
            {
                _event = _databaseHelper.GetEventByID(_event.EventID); // get updated event details
                BindEventDetails(); // bind the updated event details to text boxes
            }
        }
        // delete button to delete event
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                _databaseHelper.DeleteEvent(_event.EventID); //using database helper to delete from database
                MessageBox.Show("Event deleted!");
                EventDeletedHandler?.Invoke(); // trigger the event

                _mainPage.RefreshCalendar();

                this.Close();
            }
            catch (Exception ex)
            {
                //show error message
                MessageBox.Show(ex.Message);
            }
        }
        // update button that allows event editing
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                //first make sure input is valid
                if (ValidateInput())
                {
                    DateTime eventDate = DateTime.Parse(txtDate.Text);
                    TimeSpan startTime = TimeSpan.Parse(txtStartTime.Text);
                    TimeSpan endTime = TimeSpan.Parse(txtEndTime.Text);

                    // check for conflicts before updating
                    if (_databaseHelper.HasEventConflict(eventDate, startTime, endTime))
                    {
                        MessageBox.Show("This event conflicts with an existing event. Please choose a different time.");
                        return;
                    }

                    CalendarEvent updatedEvent = new CalendarEvent
                    {
                        EventID = _event.EventID,
                        EventDate = eventDate,
                        EventTitle = txtTitle.Text,
                        EventDescription = txtDescription.Text,
                        StartTime = startTime,
                        EndTime = endTime
                    };
                    //use datbase helper to update event in the database
                    _databaseHelper.UpdateEvent(updatedEvent);
                    MessageBox.Show("Event updated successfully!");
                    EventModifiedHandler?.Invoke();
                    this.Close();
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Please check your input formats:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred:\n" + ex.Message);
            }
        }



        //method validateinput
        private bool ValidateInput()
        {
            if (!DateTime.TryParse(txtDate.Text, out _))
            {
                MessageBox.Show("Invalid date format. Please enter a valid date.");
                return false;
            }

            if (!TimeSpan.TryParse(txtStartTime.Text, out _) || !TimeSpan.TryParse(txtEndTime.Text, out _))
            {
                MessageBox.Show("Invalid time format. Please enter valid start and end times.");
                return false;
            }

            return true; 
        }



        public event Action EventDeletedHandler;
        public event Action EventModifiedHandler;
    }
}
