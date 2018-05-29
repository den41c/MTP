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
        private bool skipValueChanged;
        private List<Group> list_group = new List<Group>();
        //private List<TextBox> list_group_text_boxs = new List<TextBox>();
        public MainWindow()
        {
            InitializeComponent();

            SetVisible();
            TaskListBox.MouseDoubleClick += TaskListBox_MouseDoubleClick;

            TaskListBox.Items.Clear();
            TaskListBox.Items.Refresh();

            GroupListBox.SelectionChanged += GroupListBox_SelectionChanged;
            //var tasks = new List<Classes.Task>();
            var cmd = SqlClient.CreateCommand("select * from groups");
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list_group.Add(new Group() {
                    GroupId = (int)(long)reader["groupid"],
                    GroupName = (string)reader["groupname"],
                    stored_name_of_task = string.Empty
                });
            }
            
            GroupListBox.ItemsSource = list_group;
        }

        private void TaskListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void SetTasksListBox(int GroupCode)
        {
            if (list_group.Count > 0)
            {
                GroupListBox.SelectedItem = list_group.Find(w => (w.GroupId == GroupCode));
                TaskListBox.ItemsSource = list_group.Find(w => (w.GroupId == GroupCode) && (w.task_list != null)).task_list;
            }
            else
            {
                TaskListBox.ItemsSource = new List<ToDoTask>();
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
                if (list_group.Count > 0 )
                {
                    TaskListBox.SelectedItem = list_group.Find(w => w.GroupId == group.GroupId).task_list.FirstOrDefault();
                }

                TaskTextBox.Text = group.stored_name_of_task;
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
            if (list_group.Count > 0)
            {
                SetTasksListBox(list_group.Last().GroupId);
            }
            else
            {
                SetTasksListBox(0);
            }
           
            //TaskListBox.Items.Clear(); 
        }
  
        private void GroupTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            AddGroupButton.IsEnabled = GroupTextBox.Text != string.Empty;
        }

        private void TaskTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var item = GroupListBox.SelectedItem;
            if (item == null || !(item is Group))
            {
                return;
            }
            var group = (Group)item;
            group.stored_name_of_task = TaskTextBox.Text;
            AddTaskButton.IsEnabled = TaskTextBox.Text != string.Empty;
            if (!AddTaskButton.IsEnabled)
            {
                TaskTextBox.Text = string.Empty;
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var item = GroupListBox.SelectedItem;
            if (item == null || !(item is Group))
            {
                return;
            }
            var group = (Group)item;
         

            var cmd = SqlClient.CreateCommand(@"insert into Tasks (taskname, groupid, done, description) 
                                                    values(@taskname,@groupid,@done,@description);
                                                select last_insert_rowid();
                                                    ");
            cmd.Parameters.Add(new SQLiteParameter("taskname", TaskTextBox.Text));
            cmd.Parameters.Add(new SQLiteParameter("groupid", group.GroupId));
            cmd.Parameters.Add(new SQLiteParameter("done", "-"));
            cmd.Parameters.Add(new SQLiteParameter("description", "")); 
            var newTaskId = (int)(long)cmd.ExecuteScalar();
            list_group.Find(w => w.GroupId == group.GroupId).task_list.Add(new ToDoTask()
            {
                TaskName = TaskTextBox.Text,
                TaskId = newTaskId,
                Deadline = null
            });
            SetTasksListBox(group.GroupId);
            GroupListBox.Items.Refresh();
            TaskTextBox.Text = string.Empty;
        }

        private void RemoveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var taskItemToRemove = TaskListBox.SelectedItem;
            var groupItemToRemove = GroupListBox.SelectedItem;

            if (taskItemToRemove == null || !(taskItemToRemove is ToDoTask))
            {
                return;
            }
            if (groupItemToRemove == null || !(groupItemToRemove is Group))
            {
                return;
            }

            var taskToRemove = (ToDoTask)taskItemToRemove;
            var groupToRemove = (Group)groupItemToRemove;

            var cmd = SqlClient.CreateCommand(@"delete from tasks where groupid = @groupid and taskid = @taskid;");

            cmd.Parameters.Add(new SQLiteParameter("groupid", groupToRemove.GroupId));
            cmd.Parameters.Add(new SQLiteParameter("taskid", taskToRemove.TaskId));
            cmd.ExecuteNonQuery();
            list_group.Find(w => (w.GroupId == groupToRemove.GroupId) && (w.task_list != null)).
                task_list.Remove(taskToRemove);

            TaskListBox.Items.Refresh();
            //GroupListBox.ItemContainerGenerator.ContainerFromIndex(1);
            RemoveTaskButton.IsEnabled = false;

            TaskTextBox.IsEnabled = false;
            //TaskListBox.Items.Clear(); 
            GroupListBox.Items.Refresh();
        }

        private void TaskListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveTaskButton.IsEnabled = true;
            
            if (SetVisible())
                SetTaskInfo();
        }

        //private void CheckBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var cb = (CheckBox)sender;
        //    cb.IsChecked = !cb.IsChecked;
            
        //    //var t = DependencyProperty.Register("d",)
        //    //cb.
        //    //cb.SetValue(new DependencyProperty.,1);
        //    var task = (ToDoTask)TaskListBox.SelectedItem;

        //    var Editform = new Edit((int)cb.Tag);
        //    Editform.Show();

        //    this.Close();
        //}

        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            Calendars.Authorization();
        }

        private void Name_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!skipValueChanged)
            {
                list_group.Find(w => w.GroupId == SelectedGroup().GroupId).
                task_list.Find(w => w.TaskId == SelectedTask().TaskId).TaskName = Name_TextBox.Text;
                SaveChanges();
            }
            
        }
        private void SaveChanges()
        {

            var cmd = SqlClient.CreateCommand(@"update Tasks set 
                                                        taskname = @taskname,
                                                        deadline = @deadline,
                                                        description = @description
                                                where taskid = @taskid
                                                    ");
            cmd.Parameters.Add(new SQLiteParameter("taskname", Name_TextBox.Text));           
            cmd.Parameters.Add(new SQLiteParameter("description", Description_TextBox.Text));
            cmd.Parameters.Add(new SQLiteParameter("deadline", Deadline.SelectedDate));
            cmd.Parameters.Add(new SQLiteParameter("taskid", SelectedTask().TaskId));
            cmd.ExecuteNonQuery();
            TaskListBox.Items.Refresh();
        }
        private ToDoTask SelectedTask()
        {
            return (ToDoTask)TaskListBox.SelectedItem != null ?
                (ToDoTask)TaskListBox.SelectedItem
                : new ToDoTask();
            //return (ToDoTask)TaskListBox.SelectedItem;
        }

        private Group SelectedGroup()
        {
            return (Group)GroupListBox.SelectedItem != null ?
                 (Group)GroupListBox.SelectedItem
                : new Group();
            //return (Group)GroupListBox.SelectedItem;
        }

        private void SetTaskInfo()
        {
            //var eventn = Name_TextBox.TextChanged;
            skipValueChanged = true;
            Name_TextBox.Text = SelectedTask().TaskName;
            Description_TextBox.Text = SelectedTask().Desc;
            Deadline.SelectedDate = SelectedTask().Deadline;
            skipValueChanged = false;
            
        }

        private void Description_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!skipValueChanged)
            {
                list_group.Find(w => w.GroupId == SelectedGroup().GroupId).
                task_list.Find(w => w.TaskId == SelectedTask().TaskId).Desc = Description_TextBox.Text;
                SaveChanges();
            }
        }
        private bool SetVisible()
        {
            var visible = SelectedTask().TaskId == 0 ? Visibility.Hidden : Visibility.Visible;
            Name_Label.Visibility = visible;
            Name_TextBox.Visibility = visible;
            Description_Label.Visibility = visible;
            Description_TextBox.Visibility = visible;
            Deadline.Visibility = visible;
            Deadline_Label.Visibility = visible;
            return visible == Visibility.Visible;
        }

        private void Deadline_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!skipValueChanged)
            {
                list_group.Find(w => w.GroupId == SelectedGroup().GroupId).
               task_list.Find(w => w.TaskId == SelectedTask().TaskId).Deadline = (DateTime?)Deadline.SelectedDate;
                SaveChanges();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {          
            switch (((sender as ComboBox).SelectedItem as ComboBoxItem).Content as string)
            {
                case "Daily":
                    Daily.Visibility = Visibility.Visible;
                    Weekly.Visibility = Visibility.Hidden;
                    Monthly.Visibility = Visibility.Hidden;
                    break;
                case "Weekly":
                    Daily.Visibility = Visibility.Hidden;
                    Weekly.Visibility = Visibility.Visible;
                    Monthly.Visibility = Visibility.Hidden;
                    break;
                case "Monthly":
                    Daily.Visibility = Visibility.Hidden;
                    Weekly.Visibility = Visibility.Hidden;
                    Monthly.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Border border = new Border();
                    border.BorderThickness = new Thickness(1.0);
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Daily.Children.Add(border);
                }
            }
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Border border = new Border();
                    border.BorderThickness = new Thickness(1.0);
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Weekly.Children.Add(border);
                }
            }
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Border border = new Border();
                    border.BorderThickness = new Thickness(1.0);
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Monthly.Children.Add(border);
                }
            }
        }
    }
}
