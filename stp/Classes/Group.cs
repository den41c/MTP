using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stp.Classes
{
    class Group
    {
        private string group_name;
        public string GroupName
        {
            get { return group_name; }
            set { group_name = value; }
        }
        private int GroupId;
        public override string ToString()
        {
            return GroupName;
        }
    }
}
