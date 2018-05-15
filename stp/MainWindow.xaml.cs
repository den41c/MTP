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
        //private List<TextBox> list_group_text_boxs = new List<TextBox>();
        public MainWindow()
        {
            InitializeComponent();

            GroupListBox.SelectionChanged += GroupListBox_SelectionChanged;
            var tasks = new List<Classes.Task>();
            var cmd = SqlClient.CreateCommand("select * from groups");
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list_group.Add(new Group() {
                    GroupId = reader.GetInt32(0),
                    GroupName = reader.GetFieldValue<string>(1),                 
                });

            }

            tasks.Add(new Classes.Task() { TaskName = "task 3", Done = true });
            tasks.Add(new Classes.Task() { TaskName = "task 4", Done = false });


            TaskListBox.Items.Add(new CheckBox() { IsChecked = false, Content = "2", Uid = "1" });
            GroupListBox.ItemsSource = list_group;
            //GroupListGrid.ItemsSource = list_group.First().task_list;
            foreach (var task in list_group.First().task_list)
            {
                TaskListBox.Items.Add(task);
            }

            //GroupListGrid.Columns.Where(w => w.Header.ToString() == "Done").First().IsReadOnly = false;

            //TaskTextBox.Text = GroupListGrid.Columns.se
            //GroupListGrid.Items.Add(new Classes.Task() { TaskName ="task 1", Done = true});
            //GroupListGrid.Items.Add(new Classes.Task() { TaskName = "task 2 ", Done = false });
            //list_group.ForEach(w => GroupListGrid.Items.Add(w));

        }
        //private List<Classes.Task> SetTasksList(int GroupCode)
        //{

        //    return 
        //}

        private void GroupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveGroupButton.IsEnabled = true;
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            list_group.Add(new Group()
            {
                GroupName = GroupTextBox.Text
            });
            GroupListBox.Items.Refresh();
            GroupTextBox.Text = string.Empty;
        }


        private void RemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            var itemToRemove = GroupListBox.SelectedItem;
            if (itemToRemove == null || !(itemToRemove is Group))
            {
                return;
            }
            list_group.Remove((Group)itemToRemove);
            GroupListBox.Items.Refresh();
            RemoveGroupButton.IsEnabled = false;
        }
        private void OKButton_Click()
        {
            return;
        }

        private void GroupTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            AddGroupButton.IsEnabled = GroupTextBox.Text != string.Empty;
        }
    }
}
