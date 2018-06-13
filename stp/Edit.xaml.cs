using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using stp.Classes;
using System.Data.SQLite;
namespace stp
{
    /// <summary>
    /// Логика взаимодействия для Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        private ToDoTask task;
        public int ThisTaskId;
        public Edit()
        {
            
            InitializeComponent();

        }

        public Edit(int taskId)
        {

            InitializeComponent();

            task = new ToDoTask() { };
            var cmd = SqlClient.CreateCommand("select taskid, taskname, done, description from TASKS where TASKID = @taskid");
            cmd.Parameters.Add(new SQLiteParameter("taskid", taskId));
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {

                    task.TaskId = taskId;
                    task.TaskName = (string)reader["taskname"];
                    //task.done = (string)reader["done"] == "+";
                    task.Desc = (string)reader["description"];
                }

            }
            Name_TextBox.Text = task.TaskName;
            DoneCb.IsChecked = task.Done;
            Description_TextBox.Text = task.Desc;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var cmd = SqlClient.CreateCommand(@"update Tasks set 
                                                        taskname = @taskname,
                                                        done = @done,
                                                        description = @description,
                                                        priority = @priority
                                                where taskid = @taskid
                                                    "); 
            cmd.Parameters.Add(new SQLiteParameter("taskname", Name_TextBox.Text));
            cmd.Parameters.Add(new SQLiteParameter("done", (bool)DoneCb.IsChecked ? "+":"-"));
            cmd.Parameters.Add(new SQLiteParameter("description", Description_TextBox.Text));
            cmd.Parameters.Add(new SQLiteParameter("taskid", task.TaskId));
            cmd.Parameters.Add(new SQLiteParameter("priority", task.Priority));
            
            cmd.ExecuteNonQuery();



            var MainW = new MainWindow();
            MainW.Show();
            this.Close();
        }
        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                //string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                //// Assuming you have one file that you care about, pass it off to whatever
                //// handling code you have defined.
                //HandleFileOpen(files[0]);
            }
        }
    }
}
