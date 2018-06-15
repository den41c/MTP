using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stp.Classes
{
    public enum  TypeOfItem
    {
        Event, 
        ToDoTask
    }
    public class ItemList
    {
        public TypeOfItem Type;
        public string ID;
    }
}
