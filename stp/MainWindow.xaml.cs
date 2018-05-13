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
using System.Windows.Navigation;
using System.Windows.Shapes;
using stp.Classes;

namespace stp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Group> list_group = new List<Group>();
        public MainWindow()
        {
            InitializeComponent();
            //var list = new List<Group>();
            
            for (int i=0;i<3;i++)
            {
                list_group.Add(new Group() {
                    GroupName = "football"
                });
            }
            
            //ListGroup1.ItemsSource = list_group;


           
            GroupList.ItemsSource = list_group;



        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
           // ListGroup.
            //var listbox = (ListBox)sender;
            list_group.Add(new Group() {
                GroupName = "volleyball"
            });
            //ListGroup1.ItemsSource = list_group;

            GroupList.Items.Refresh();
                
        }
    }
}
