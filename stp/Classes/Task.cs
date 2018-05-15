using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stp.Classes
{
    class Task
    {
        public string TaskName { get; set; }
        
        public int TaskId;
        public bool Done { get; set; }
        public override string ToString()
        {
            return TaskName;
        }
    }
}
