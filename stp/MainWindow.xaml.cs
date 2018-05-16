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
using System.Data.SQLite;
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
            //var tasks = new List<Classes.Task>();
            var cmd = SqlClient.CreateCommand("select * from groups");
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list_group.Add(new Group() {
                    GroupId = (int)(long)reader["groupid"],
                    GroupName =  (string)reader["groupname"],                 
                });
            }
            
            GroupListBox.ItemsSource = list_group;
        }

        private void SetTasksListBox(int GroupCode)
        {
            TaskListBox.Items.Clear();
            foreach (var task in list_group.Find(w=>(w.GroupId == GroupCode) && (w.task_list != null)).task_list)
            {
                TaskListBox.Items.Add(new CheckBox() { IsChecked = task.Done, Content = task.TaskName });
            }
            TaskListBox.Items.Refresh();
        }

        private void GroupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveGroupButton.IsEnabled = true;
            TaskTextBox.IsEnabled = true;
            var group = (Group)GroupListBox.SelectedItem;
            if (group != null)
            {
                SetTasksListBox(group.GroupId);
            }
            
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var cmd = SqlClient.CreateCommand(@"insert into GROUPS (groupname) values(@groupname);
                                                select last_insert_rowid();
                                                    ");
            cmd.Parameters.Add(new SQLiteParameter("groupname", GroupTextBox.Text));
            var newGroupId = (int)(long)cmd.ExecuteScalar();
            list_group.Add(new Group()
            {
                GroupId = newGroupId,
                GroupName = GroupTextBox.Text,
                //task_list = new List<Classes.Task>()
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
            var groupToRemove = (Group)itemToRemove;
            list_group.Remove(groupToRemove);
           
            var cmd = SqlClient.CreateCommand(@"delete from groups where groupid = @groupid;
                                                delete from tasks where groupid = @groupid;");

            cmd.Parameters.Add(new SQLiteParameter("groupid", groupToRemove.GroupId));
            cmd.ExecuteNonQuery();

            
            GroupListBox.Items.Refresh();
            //GroupListBox.ItemContainerGenerator.ContainerFromIndex(1);
            RemoveGroupButton.IsEnabled = false;
            TaskTextBox.IsEnabled = false;
            TaskListBox.Items.Clear(); 
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
