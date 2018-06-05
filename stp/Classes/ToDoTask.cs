using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Windows.Media;

namespace stp.Classes
{
    public class ToDoTask
    {
        public string TaskName { get; set; }
        
        public int TaskId { get; set;}
        public string Desc;
        public bool done;
        public DateTime? Deadline { get; set; }

        public string _priority;

        public string Priority {
            get { return _priority; }
            set { _priority = value;

                switch (this.Priority)
                {
                    case "Important":
                        Color = Brushes.Black;// Brushes.LightYellow;
                        break;
                    case "Unimportant":
                        Color = Brushes.Black;//Brushes.LimeGreen;
                        break;
                    case "Common":
                        Color = Brushes.Black;//Brushes.Orchid;
                        break;
                }
            } }

        public Brush Color  { get; set; }
        
        public bool Done
        {
            get { return done; }
            set
            {
               
                var cmd = SqlClient.CreateCommand(@"update tasks set done = @done  
                                                    where taskid=@taskid ");
                cmd.Parameters.Add(new SQLiteParameter("done", value ? "+" : "-"));
                cmd.Parameters.Add(new SQLiteParameter("taskid", TaskId));
                //cmd.Parameters.Add(new SQLiteParameter("groupid", value ? "+" : "-"));
                cmd.ExecuteNonQuery();
                done = value;
            }
        }
        public override string ToString()
        {
            return TaskName;
        }
    }
}
