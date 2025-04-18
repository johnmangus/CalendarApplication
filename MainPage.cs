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
    public partial class MainPage : Form
    {
        private int currentYear;
        private int currentMonth;
        
        private DatabaseHelper _databaseHelper;
        private CalendarEvent _event;
        

        public MainPage()
        {
            InitializeComponent();
            ConfigureTableLayout();
            

            currentYear = DateTime.Now.Year;
            currentMonth = DateTime.Now.Month;

            _databaseHelper = new DatabaseHelper();

            PopulateCalendar(currentYear, currentMonth);
            UpdateMonthYearLabel(currentYear, currentMonth);
        }

        private void ConfigureTableLayout()
        {
            // configure columns
            tableLayoutPanel1.ColumnStyles.Clear();  
            for (int col = 0; col < 7; col++)  // 7 columns for days of the week
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 111));  // column width
            }

            // configure rows
            tableLayoutPanel1.RowStyles.Clear();  // clear existing 
            for (int row = 0; row < 6; row++)  // up to 6 rows to account for how weeks could be layed out
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));  // row height
            }
        }
        
        
        private void PopulateCalendar(int year, int month)
        {
            if (year < 1 || month < 1 || month > 12)
            {
                MessageBox.Show("Invalid year or month input.");
                return; // Exit the method if invalid month/year
            }

            tableLayoutPanel1.SuspendLayout(); // suspend layout 
            tableLayoutPanel1.Controls.Clear(); // clear previous buttons and controls

            // configure both columns and rows of calendar
            for (int col = 0; col < 7; col++)
            {
                tableLayoutPanel1.ColumnStyles[col] = new ColumnStyle(SizeType.Percent, 100f / 7);
            }

            for (int row = 0; row < 6; row++)
            {
                if (tableLayoutPanel1.RowStyles.Count > row)
                    tableLayoutPanel1.RowStyles[row] = new RowStyle(SizeType.Percent, 100f / 6);
                else
                    tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6));
            }

            int currentDay = 1;
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    if ((row == 0 && col < (int)new DateTime(year, month, 1).DayOfWeek) || currentDay > DateTime.DaysInMonth(year, month))
                    {
                        // empty labels for days outside the month for correct month config
                        tableLayoutPanel1.Controls.Add(new Label() { Text = "", Dock = DockStyle.Fill }, col, row);
                    }
                    else
                    {
                        //call method haseventsonday (just need to know if there is event)
                        bool hasEvents = _databaseHelper.HasEventsOnDay(year, month, currentDay);

                        Button dayButton = new Button
                        {
                            Dock = DockStyle.Fill,
                            Text = currentDay.ToString() + (hasEvents ? "\nEvents" : ""),
                            Margin = new Padding(0),
                            Tag = new DateTime(year, month, currentDay),
                            Font = new Font("MS Reference Sans Serif", 10, FontStyle.Regular)
                        };
                        dayButton.Click += DayButton_Click;
                        tableLayoutPanel1.Controls.Add(dayButton, col, row);
                        currentDay++;
                    }
                }
            }

            tableLayoutPanel1.ResumeLayout(false); 
            tableLayoutPanel1.PerformLayout(); 
        }


        public void RefreshCalendar()
        {
            // refresh the calendar
            PopulateCalendar(currentYear, currentMonth);
            UpdateMonthYearLabel(currentYear, currentMonth);
        }




        private void ShowEventDetails(CalendarEvent calendarEvent)
        {
            EventDetails eventDetails = new EventDetails(calendarEvent, _databaseHelper, this);
            eventDetails.EventDeletedHandler += new Action(EventDeletedHandler);
            eventDetails.EventModifiedHandler += new Action(EventModifiedHandler);
            eventDetails.ShowDialog();
        }

        private void EventDeletedHandler()
        {
            // call to refresh the calendar after deletion action
            MessageBox.Show("Event was deleted. Refreshing the calendar...");
            RefreshCalendar();
        }

        private void EventModifiedHandler()
        {
            // call to refresh the calendar after update action
            MessageBox.Show("Event was updated. Refreshing the calendar...");
            RefreshCalendar();
        }

        private void AddEventForm_EventAdded(object sender, EventArgs e)
        {
            //call to refresh the calendar after addevent action
            RefreshCalendar();  
        }

             
        //when user clicks on specific day button
        private void DayButton_Click(object sender, EventArgs e)
        {
            Button dayButton = sender as Button;
            DateTime date = (DateTime)dayButton.Tag;

            //call geteventsbyday method
            var events = _databaseHelper.GetEventsByDay(date.Year, date.Month, date.Day);
            //if event?
            if (events.Any())
            {
                //call showeventdetails  
                CalendarEvent eventOfDay = events.First();
                ShowEventDetails(eventOfDay);
            }
            else
            {
                //no events
                MessageBox.Show("No events on this day.");
            }
        }

        //button for moving forward through calendar months
        private void btnNext_Click(object sender, EventArgs e)
        {
            // increment the current month
            currentMonth++; 
            if (currentMonth > 12)
            {
                currentMonth = 1; // reset to January if currentMonth goes beyond December
                currentYear++; // increment the current year
            }
            //refresh calendar
            PopulateCalendar(currentYear, currentMonth);
            UpdateMonthYearLabel(currentYear, currentMonth);
            Console.WriteLine($"Next Clicked: Year - {currentYear}, Month - {currentMonth}");
        }

        //button for moving backwards through calendar months
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            // decrement the current month
            currentMonth--; 
            if (currentMonth < 1)
            {
                currentMonth = 12; 
                currentYear--; 
            }
            //refresh calendar
            PopulateCalendar(currentYear, currentMonth);
            UpdateMonthYearLabel(currentYear, currentMonth);
            Console.WriteLine($"Previous Clicked: Year - {currentYear}, Month - {currentMonth}");
        }

        // calendar month/year lable that updates 
        private void UpdateMonthYearLabel(int year, int month)
        {
            // set the text of lblMonthYear to show the month and year
            lblMonthYear.Text = new DateTime(year, month, 1).ToString("MMMM yyyy");
            lblMonthYear.Refresh(); // force refresh
        }

        // button that takes us to monthly view form
        private void btnShowMonthlyView_Click(object sender, EventArgs e)
        {
            MonthlyView monthlyViewForm = new MonthlyView(_databaseHelper);
            monthlyViewForm.Show();
        }

        // button that takes us to add event form
        private void btnAddEvent_Click(object sender, EventArgs e)
        {
            AddEvent addEventForm = new AddEvent(_databaseHelper);
            addEventForm.EventAdded += AddEventForm_EventAdded;  // subscribe to the event
            addEventForm.ShowDialog();  // show the AddEvent form as a dialog
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }
    }
    }

