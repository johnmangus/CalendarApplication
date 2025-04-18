using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ForgetNot_Calendar
{
    public partial class AddEvent : Form
    {
        
        public event EventHandler EventAdded;
        private DatabaseHelper _databaseHelper;
        
        

        public AddEvent(DatabaseHelper databaseHelper)
        {
            InitializeComponent();
            _databaseHelper = databaseHelper;
        }

        // save button with input validation
        private void btnSave_Click(object sender, EventArgs e)
        {
            string title = txtTitle.Text;
            DateTime eventDate = dtpEventDate.Value;
            TimeSpan? startTime = dtpStartTime.Checked ? (TimeSpan?)dtpStartTime.Value.TimeOfDay : null;
            TimeSpan? endTime = dtpEndTime.Checked ? (TimeSpan?)dtpEndTime.Value.TimeOfDay : null;
            string description = txtDescription.Text;

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a title for the event.");
                return;
            }

            // ensure both start and end times are selected
            if (!startTime.HasValue || !endTime.HasValue)
            {
                MessageBox.Show("Please select both start and end times.");
                return;
            }

            // check if the start time is before the end time
            if (startTime.Value >= endTime.Value)
            {
                MessageBox.Show("The end time must be later than the start time.");
                return;
            }

            // check for event time conflicts
            try
            {
                if (_databaseHelper.HasEventConflict(eventDate, startTime.Value, endTime.Value))
                {
                    MessageBox.Show("This event conflicts with another scheduled event. Please choose a different time.");
                    return;
                }

                // add event if all checks pass
                _databaseHelper.AddEvent(title, eventDate, description, startTime, endTime);
                EventAdded?.Invoke(this, EventArgs.Empty);

                MessageBox.Show("Event saved successfully.");
                this.Close();  
            }
            catch (Exception ex)
            {
                //display with error message
                MessageBox.Show($"Failed to save the event. Error: {ex.Message}");
            }
        }










    }
}

