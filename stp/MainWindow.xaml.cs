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
using System.Linq;

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
            GroupListGrid.SelectedCellsChanged += GroupList_SelectedCellsChanged;
            //var list = new List<Group>();

            for (int i=0;i<3;i++)
            {
                list_group.Add(new Group() {
                    GroupName = "football"
                });
            }

            //ListGroup1.ItemsSource = list_group;


            list_group.ForEach(w => GroupListGrid.Items.Add(w));
            //GroupList.Items.Add(list_group.First());//.ItemsSource = list_group;



        }

        private void GroupList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            RemoveGroup.IsEnabled = true;
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            // ListGroup.
            //var listbox = (ListBox)sender;

            //ListGroup1.ItemsSource = list_group;
           
            GroupListGrid.Items.Add(new Group()
            {
                GroupName = "volleyball"
            });
           
                
        }

     

        //private void GroupList_AddingNewItem(object sender, AddingNewItemEventArgs e)
        //{

        //}

        private void RemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            var listToRemove = GroupListGrid.SelectedCells;
            foreach (var itemToRemove in listToRemove)
            {
                GroupListGrid.Items.Remove(itemToRemove.Item);
            }
            GroupListGrid.Items.Refresh();
            RemoveGroup.IsEnabled = false;
        }

    
    }
}
