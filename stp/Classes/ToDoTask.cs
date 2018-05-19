using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
namespace stp.Classes
{
    public class ToDoTask
    {
        public string TaskName { get; set; }
        
        public int TaskId;
        public bool done;
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
