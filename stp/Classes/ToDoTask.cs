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
        //public Image Image { get {
        //        var im = new Image();
        //        switch (Priority)
        //        {
        //            case "Common":
        //                im.Source = "dsadsa";

        //        }
        //        return new Image() { Source = };
        //    } }
        //public string Priority2
        //{
        //    get { return _priority; }
        //    set
        //    {
        //        var cmd = SqlClient.CreateCommand(@"update tasks set Priority = @Priority  
        //                                            where taskid=@taskid ");
        //        cmd.Parameters.Add(new SQLiteParameter("Priority", value));
        //        cmd.Parameters.Add(new SQLiteParameter("taskid", TaskId));
        //        //cmd.Parameters.Add(new SQLiteParameter("groupid", value ? "+" : "-"));
        //        cmd.ExecuteNonQuery();
        //        _priority = value;
        //        NotifyPropertyChanged("Priority");
        //    }
        //}
        //public string Priority2
        //{
        //    get { return _priority; }
        //    set
        //    {
        //        _priority = value;

        //        switch (this.Priority2)
        //        {
        //            case "Important":
        //                Color = Brushes.Black;// Brushes.LightYellow;
        //                break;
        //            case "Unimportant":
        //                Color = Brushes.Black;//Brushes.LimeGreen;
        //                break;
        //            case "Common":
        //                Color = Brushes.Black;//Brushes.Orchid;
        //                break;
        //        }
        //    }
        //}

        public string TaskName { get; set; }
        
        private int taskId;
        public int TaskId { get { return taskId; } set { taskId = value;  /*NotifyPropertyChanged("List_image");*/ } }
        public string Desc;
        public DateTime? Deadline { get; set; }

        public Brush Color;
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
