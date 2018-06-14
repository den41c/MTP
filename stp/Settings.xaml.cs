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
    public class Setting
    {
        public string Email { get; set; }
        
        private bool _checked;
        public bool Checked
        {
            get { return _checked; }
            set
            {
                _checked = value;
                foreach (var acc in Calendars.Accounts.Where(w => w.Email == Email))
                {
                    acc.ToDoEnable = value;
                }
            }
        }
    }
    /// <summary>
    /// Логика взаимодействия для Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        private MainWindow main;

        public Edit()
        {
        }
        public List<Setting> Setting;
        public Edit(MainWindow main)
        {
            this.main = main;
            Setting = new List<Setting>();
            foreach (var acc in Calendars.Accounts)
            {
                Setting.Add(new stp.Setting() { Checked = acc.ToDoEnable, Email = acc.Email });
            }
           

            InitializeComponent();

            SettingListBox.ItemsSource = Setting.GroupBy(p => p.Email)
              .Select(g => g.First()).ToList();

        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            
            main.SetEventList();
            this.Close();
        }
    }
}
