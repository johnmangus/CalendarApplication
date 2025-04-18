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
    public partial class MonthlyView : Form
    {

        private DatabaseHelper _databaseHelper;

        public MonthlyView(DatabaseHelper databaseHelper)
        {
            InitializeComponent();
            _databaseHelper = databaseHelper;

            InitializeListView();
            PopulateMonthYearDropdowns();
            DisplayEvents();
        }

        private void InitializeListView()
        {
            listViewEvents.Columns.Clear();
            listViewEvents.Columns.Add("Event Title", 150);  
            listViewEvents.Columns.Add("Date", 100);         
            listViewEvents.Columns.Add("Time", 100);         
            listViewEvents.Columns.Add("Description", 250);  
            listViewEvents.View = View.Details;
            listViewEvents.FullRowSelect = true;  // if want to make it so user can click on whole row for event
        }

        // populate month and year dropdowns
        private void PopulateMonthYearDropdowns()
        {
            for (int year = DateTime.Now.Year - 10; year <= DateTime.Now.Year + 10; year++)
            {
                comboBoxYear.Items.Add(year);
            }
            comboBoxYear.SelectedItem = DateTime.Now.Year;

            for (int month = 1; month <= 12; month++)
            {
                comboBoxMonth.Items.Add(new DateTime(2000, month, 1).ToString("MMMM"));
            }
            comboBoxMonth.SelectedIndex = DateTime.Now.Month - 1;
        }
        //method to display events in the listview
        private void DisplayEvents()
        {
            if (comboBoxMonth.SelectedIndex < 0 || comboBoxYear.SelectedIndex < 0)
                return;

            int year = int.Parse(comboBoxYear.SelectedItem.ToString());
            int month = comboBoxMonth.SelectedIndex + 1;
            listViewEvents.Items.Clear(); //clear listView

            // get sorted events directly using GetSortedEventsByMonth method and database helper
            var sortedEvents = _databaseHelper.GetSortedEventsByMonth(year, month);
            foreach (var eventDetails in sortedEvents)
            {
                var item = new ListViewItem(new[] {
                eventDetails.EventTitle,
                eventDetails.EventDate.ToString("MM-dd-yy"),
                eventDetails.StartTime?.ToString(@"hh\:mm") ?? "All day",
                eventDetails.EventDescription
                });
                listViewEvents.Items.Add(item);
            }
        }
        //if month selection is changed
        private void comboBoxMonth_SelectedIndexChanged(object sender, EventArgs e)
        {

            DisplayEvents();
        }
        //if year selection is changed
        private void comboBoxYear_SelectedIndexChanged(object sender, EventArgs e)
        {

            DisplayEvents();
        }
        //if either is changed, update the listview
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}

