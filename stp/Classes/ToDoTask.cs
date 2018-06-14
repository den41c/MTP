using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;

namespace stp.Classes
{


    public class ToDoTask : INotifyPropertyChanged
    {


        public ToDoTask()
        {
        }
        public bool Done
        {
            get { return _done; }
            set
            {

                var cmd = SqlClient.CreateCommand(@"update tasks set done = @done  
                                                    where taskid=@taskid ");
                cmd.Parameters.Add(new SQLiteParameter("done", value ? "+" : "-"));
                cmd.Parameters.Add(new SQLiteParameter("taskid", TaskId));
                //cmd.Parameters.Add(new SQLiteParameter("groupid", value ? "+" : "-"));
                cmd.ExecuteNonQuery();
                _done = value;
            }
        }
        public bool _done;
        //public string _priority;
        
        public string _priority;
        public string Priority { get { return _priority; }
            set
            {
                var cmd = SqlClient.CreateCommand(@"update tasks set Priority = @Priority  
                                                    where taskid=@taskid ");
                cmd.Parameters.Add(new SQLiteParameter("Priority", value));
                cmd.Parameters.Add(new SQLiteParameter("taskid", TaskId));             
                cmd.ExecuteNonQuery();

                _priority = value;
                NotifyPropertyChanged("Priority");
            }
        }
      

        public string TaskName { get; set; }
        
        private int taskId;
        public int TaskId { get { return taskId; } set { taskId = value;  /*NotifyPropertyChanged("List_image");*/ } }
        public string Desc;
        private DateTime? _deadline;
        public DateTime? Deadline
        {
            get { return _deadline; }
            set
            {
                _deadline = value;
                Color = Brushes.Transparent;

                if (_deadline.Value == DateTime.MinValue || _deadline.Value == null)
                {
                    return;
                }
                if (DateTime.Now > _deadline.Value)
                    Color = Brushes.Orange;
                if (DateTime.Now > _deadline.Value.AddDays(2))
                    Color = Brushes.Red;
            }
        }

        public Brush Color { get; set;}
        //public List<string> _files_names;

        public List<string> Files_names;

        public void SaveFiles()
        {
            var cmd = SqlClient.CreateCommand(@"update tasks set files = @files  
                                                    where taskid=@taskid ");
            cmd.Parameters.Add(new SQLiteParameter("files", string.Join(",", this.Files_names)));
            cmd.Parameters.Add(new SQLiteParameter("taskid", this.TaskId));
            cmd.ExecuteNonQuery();
        }
      
        public event PropertyChangedEventHandler PropertyChanged;



        public override string ToString()
        {
            return TaskName;
        }
        public void NotifyPropertyChanged(string t)
        {
            if (t != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(t));
            }
        }
    }
}
