using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ForgetNot_Calendar
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create an instance of your main form (assuming it's named MainForm)
            MainPage mainPage= new MainPage();

            // Run the application's main message loop with your main form
            Application.Run(mainPage);
        }
    }
    }

