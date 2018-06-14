using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;


using System.Text.RegularExpressions;
namespace stp.Classes
{
   
    class Group
    {
        public bool All = true;
        public List<ToDoTask> task_list;
        public string GroupName { get; set; }
        public string stored_name_of_task;
        private int group_id;
        public int GroupId
        {
            get { return group_id; } 
            set
            {
                group_id = value;
                task_list = new List<ToDoTask>();
               
                var cmd = SqlClient.CreateCommand("select taskid,taskname, done, description, deadline,priority, files from TASKS where GROUPID = @groupid");
                cmd.Parameters.Add(new SQLiteParameter("groupid", value));
                
                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read())
                    {

                        task_list.Add(new ToDoTask()
                        {
                            TaskId = (int)(long)reader["taskid"],//reader.GetInt32(0),
                            TaskName = (string)reader["taskname"],
                            _done = (string)reader["done"] == "+",
                            Desc = (string)reader["description"],
                            Deadline = reader.IsDBNull(4) ? null : (DateTime?)reader["deadline"],
                            _priority = reader.IsDBNull(5) ? "Common" : (string)reader["priority"],
                            Files_names = reader.IsDBNull(6) ? new List<string>() : ((string)reader["files"]).Split(',').ToList()
                    });
                    }
                    
                }
            }
        }
        public override string ToString()
        {

            return GroupName + "(" + task_list.Where(w => All ? true: !w.Done ).ToList().Count.ToString() + ")";
        }

     
    }
}
