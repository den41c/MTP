using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;

namespace stp.Classes
{
    public class Images
    {

        public static List<Images> list_image = new List<Images>()
        {
            new Images("Important") ,
            new Images("Unimportant")
        };


        public Images()
        {

        }
        string _priority;
        string _image;
        public Images(string priority)
        {
            _priority = priority;
            string ret;
            switch (priority)
            {
                case "Important":         
                    ret = Path.Combine(Environment.CurrentDirectory ,"flags\\red_flag.png");
                    break;
                case "Unimportant":
                    ret = Path.Combine(Environment.CurrentDirectory, "flags\\green_flag.png");
                    break;
                default:
                    ret = Path.Combine(Environment.CurrentDirectory, "Common");
                    break;
            }
            _image = ret;
        }
        public string Priority { get => _priority; set => _priority = value; }
        public string Image { get => _image; set { _image = value; } }

    }
}
