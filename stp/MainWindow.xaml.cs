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
using System.IO;
using stp.Classes;
using System.Data.SQLite;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Xceed.Wpf.Toolkit;
using System.Globalization;
using System.Text.RegularExpressions;

namespace stp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool skipValueChanged;
        private List<Classes.Group> list_group = new List<Classes.Group>();
        //private List<TextBox> list_group_text_boxs = new List<TextBox>();
       
        public MainWindow()
        {
            
            Calendars.Accounts = new System.Collections.ObjectModel.ObservableCollection<Account>();
            InitializeComponent();
			TaskGrid.DataContext = new ToDoTask();
            this.DataContext = this;
            //AccountListView.ItemsSource = ;
            SetVisible();

            //TaskListBox.MouseDoubleClick += TaskListBox_MouseDoubleClick;


            //var tasks = new List<Classes.Task>();
            var cmd = SqlClient.CreateCommand("select * from groups");
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list_group.Add(new Classes.Group() {
                    GroupId = (int)(long)reader["groupid"],
                    GroupName = (string)reader["groupname"],
                    stored_name_of_task = string.Empty
                });
            }
            
            GroupListBox.ItemsSource = list_group;
        }

        private void Accounts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SetTasksGrid(int GroupCode)
        {
            if (list_group.Count > 0)
            {
                var cb = ComboBoxDone.SelectedIndex;
                GroupListBox.SelectedItem = list_group.Find(w => (w.GroupId == GroupCode));
                //TaskListBox.ItemsSource = list_group.Find(w => (w.GroupId == GroupCode) && (w.task_list != null)).task_list;
                TaskGrid.ItemsSource = list_group.Find(w => (w.GroupId == GroupCode) && (w.task_list != null)).task_list.Where(w=> cb == 1? ! w.Done: true);
            }
            else
            {
                TaskGrid.ItemsSource = new List<ToDoTask>();
                //TaskListBox.ItemsSource = new List<ToDoTask>();
            }
            TaskGrid.Items.Refresh();
           // TaskListBox.Items.Refresh();
        }

       

        private void GroupListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            RemoveGroupButton.IsEnabled = true;
            TaskTextBox.IsEnabled = true;
            var group = (Classes.Group)GroupListBox.SelectedItem;
            if (group != null)
            {
                SetTasksGrid(group.GroupId);
                if (list_group.Count > 0)
                {
                    //TaskListBox.SelectedItem = list_group.Find(w => w.GroupId == group.GroupId).task_list.FirstOrDefault();
                    TaskGrid.SelectedItem = list_group.Find(w => w.GroupId == group.GroupId).task_list.FirstOrDefault();
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
            list_group.Add(new Classes.Group()
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
            if (itemToRemove == null || !(itemToRemove is Classes.Group))
            {
                return;
            }
            var groupToRemove = (Classes.Group)itemToRemove;
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
                SetTasksGrid(list_group.Last().GroupId);
            }
            else
            {
                SetTasksGrid(0);
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
            if (item == null || !(item is Classes.Group))
            {
                return;
            }
            var group = (Classes.Group)item;
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
            if (item == null || !(item is Classes.Group))
            {
                return;
            }
            var group = (Classes.Group)item;


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
                Deadline = null,
                Files_names = new List<string>()
            });
            SetTasksGrid(group.GroupId);
            GroupListBox.Items.Refresh();
            TaskTextBox.Text = string.Empty;
        }

        private void RemoveTaskButton_Click(object sender, RoutedEventArgs e)
        {
            var taskItemToRemove = TaskGrid.SelectedItem;
            var groupItemToRemove = GroupListBox.SelectedItem;

            if (taskItemToRemove == null || !(taskItemToRemove is ToDoTask))
            {
                return;
            }
            if (groupItemToRemove == null || !(groupItemToRemove is Classes.Group))
            {
                return;
            }
            
            var taskToRemove = (ToDoTask)taskItemToRemove;
            var groupToRemove = (Classes.Group)groupItemToRemove;

            var cmd = SqlClient.CreateCommand(@"delete from tasks where groupid = @groupid and taskid = @taskid;");

            cmd.Parameters.Add(new SQLiteParameter("groupid", groupToRemove.GroupId));
            cmd.Parameters.Add(new SQLiteParameter("taskid", taskToRemove.TaskId));
            cmd.ExecuteNonQuery();
            list_group.Find(w => (w.GroupId == groupToRemove.GroupId) && (w.task_list != null)).
                task_list.Remove(taskToRemove);


            SetTasksGrid(groupToRemove.GroupId);
            //GroupListBox.ItemContainerGenerator.ContainerFromIndex(1);
            RemoveTaskButton.IsEnabled = false;

            TaskTextBox.IsEnabled = false;
            //TaskListBox.Items.Clear(); 
            GroupListBox.Items.Refresh();
            TaskGrid.Items.Refresh();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            ToDoTask seld_task = SelectedTask();

            if (seld_task.TaskId == 0) { return; }
            string[] droppedFiles = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }

            if ((null == droppedFiles) || (!droppedFiles.Any())) { return; }
                        //FilesList.Items.Clear();
            
            foreach (string s in droppedFiles)
            {
                FileInfo fileInfo = new FileInfo(s);                     
                SaveFile(fileInfo);
                //System.Diagnostics.Process.Start(fileInfo.FullName);
                seld_task.Files_names.Add(fileInfo.Name);

                FilesList.Items.Add(fileInfo.Name);
            }

            seld_task.SaveFiles();

            FilesList.Items.Refresh();
        }

        private void SaveFile(FileInfo fi)
        {
            string fileName = fi.Name;
            string sourcePath = fi.DirectoryName;
            string targetPath = @"files";

            // Use Path class to manipulate file and directory paths.
            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);

            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }

            // To copy a file to another location and 
            // overwrite the destination file if it already exists.
            File.Copy(sourceFile, destFile, true);

            // To copy all the files in one directory to another directory.
            // Get the files in the source folder. (To recursively iterate through
            // all subfolders under the current directory, see
            // "How to: Iterate Through a Directory Tree.")
            // Note: Check for target path was performed previously
            //       in this code example.
            if (Directory.Exists(sourcePath))
            {
                string[] files = System.IO.Directory.GetFiles(sourcePath);

                // Copy the files and overwrite destination files if they already exist.
                foreach (string file in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    fileName = System.IO.Path.GetFileName(file);
                    destFile = System.IO.Path.Combine(targetPath, fileName);
                    File.Copy(file, destFile, true);
                }
            }
            else
            {
                Console.WriteLine("Source path does not exist!");
            }
        }

        private void Grid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {

        }


        private void SaveChanges()
        {

            var cmd = SqlClient.CreateCommand(@"update Tasks set 
                                                        taskname = @taskname,
                                                        deadline = @deadline,
                                                        description = @description,
                                                        priority = @priority
                                                where taskid = @taskid
                                                    ");
            cmd.Parameters.Add(new SQLiteParameter("taskname", Name_TextBox.Text));           
            cmd.Parameters.Add(new SQLiteParameter("description", Description_TextBox.Text));
            cmd.Parameters.Add(new SQLiteParameter("deadline", Deadline.Value));
           
            cmd.Parameters.Add(new SQLiteParameter("taskid", SelectedTask().TaskId));
            cmd.Parameters.Add(new SQLiteParameter("priority", SelectedTask().Priority));
            cmd.ExecuteNonQuery();
            TaskGrid.Items.Refresh();
        }
        private ToDoTask SelectedTask()
        {
            return (ToDoTask)TaskGrid.SelectedItem != null ?
                (ToDoTask)TaskGrid.SelectedItem
                : new ToDoTask();
            //return (ToDoTask)TaskListBox.SelectedItem;
        }

        private Classes.Group SelectedGroup()
        {
            return (Classes.Group)GroupListBox.SelectedItem != null ?
                 (Classes.Group)GroupListBox.SelectedItem
                : new Classes.Group();
        }

        private void SetTaskInfo()
        {       
            skipValueChanged = true;
            var sT = SelectedTask();
            Name_TextBox.Text = sT.TaskName;
            Description_TextBox.Text = sT.Desc;
            Description_TextBox.Text = sT.Desc;
            Deadline.Value = sT.Deadline;
            //HourTB.Text = sT.Deadline.Value.Hour.ToString();
            //MinTB.Text = sT.Deadline.Value.Minute.ToString();
            FilesList.Items.Clear();
            foreach (var file in sT.Files_names)
            {
                HashSet<string> inListBox = new HashSet<string>();
                foreach (var item in FilesList.Items)
                {
                    inListBox.Add(item.ToString());
                }
               
                if (!inListBox.Contains(file) && file != string.Empty)
                {
                    FilesList.Items.Add(file);
                }
                
            }

            FilesList.Items.Refresh();       
            skipValueChanged = false;

        }

        private bool SetVisible()
        {
            var t = new ColorPicker();
          
            var visible = SelectedTask().TaskId == 0 ? Visibility.Hidden : Visibility.Visible;
            Name_Label.Visibility = visible;
            Name_TextBox.Visibility = visible;
            Description_Label.Visibility = visible;
            Description_TextBox.Visibility = visible;
            Deadline.Visibility = visible;
            Deadline_Label.Visibility = visible;
            Files_Label.Visibility = visible;
            FilesList.Visibility = visible;
            RemoveFile.Visibility = visible;
            

            return visible == Visibility.Visible;
        }

        #region ValuesChanged

        private void Name_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!skipValueChanged)
            {
                list_group.Find(w => w.GroupId == SelectedGroup().GroupId).
                task_list.Find(w => w.TaskId == SelectedTask().TaskId).TaskName = Name_TextBox.Text;
                SaveChanges();
            }

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
       

        private void Deadline_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!skipValueChanged)
            {
                list_group.Find(w => w.GroupId == SelectedGroup().GroupId).
               task_list.Find(w => w.TaskId == SelectedTask().TaskId).Deadline = (DateTime?)Deadline.Value;
                SaveChanges();
            }

        }
        #endregion


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
            #region DayGrid
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    Border border = new Border();
                    border.Name = "DayBorder" + i + j;
                    border.BorderThickness = new Thickness(1.0);
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Daily.Children.Add(border);
                }
            }
            #endregion
            #region WeekGrid
            DateTime dateTime = DateTime.Now;
            while (dateTime.DayOfWeek != DayOfWeek.Sunday)
            {
                dateTime = dateTime.AddDays(-1);
            }
            for (int i = 1; i < 8; i++)
            {
                Label label = new Label();
                label.Content = dateTime.ToString("ddd") + ' ' + dateTime.ToShortDateString();
                dateTime = dateTime.AddDays(1);
                Grid.SetColumn(label, i);
                Weekly.Children.Add(label);
            }
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Border border = new Border();
                    border.Name = "WeekBorder" + i + j;
                    border.BorderThickness = new Thickness(1.0);
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Weekly.Children.Add(border);
                }
            }
            #endregion
            #region MonthGrid
            dateTime = DateTime.Now;
            int currentMonth = dateTime.Month;
            while (dateTime.Day != 1)
            {
                dateTime = dateTime.AddDays(-1);
            }
            int row = 1, col = (int)dateTime.DayOfWeek;
            while(dateTime.Month == currentMonth)
            {
                Label label = new Label();
                label.Name = "MonthLabel" + row + col;
                this.RegisterName(label.Name, label);
                label.Content = dateTime.ToShortDateString();
                Grid.SetColumn(label, col);
                Grid.SetRow(label, row);
                col++;
                if (col == 7)
                {
                    col = 0;
                    row++;
                }
                Monthly.Children.Add(label);
                dateTime = dateTime.AddDays(1);
            }
            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    Border border = new Border();
                    
                    border.Name = "MonthBorder" + i + j;
                    border.BorderThickness = new Thickness(1.0);
                    border.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    Grid.SetRow(border, i);
                    Grid.SetColumn(border, j);
                    Monthly.Children.Add(border);
                }
            }
            #endregion

            Calendars.RunJob();
            LoadAccounts();
            #region MonthEvents
            if (Calendars.events.Items != null && Calendars.events.Items.Count > 0)
            {
                foreach (var eventItem in Calendars.events.Items)
                {
                    DateTime? when = eventItem.Start.DateTime;
                    dateTime = when.Value;
                    if (DateTime.Now.Month != dateTime.Month || DateTime.Now.Year != dateTime.Year)
                    {
                        continue;
                    }
                    while (dateTime.Day != 1)
                    {
                        dateTime = dateTime.AddDays(-1);
                    }
                    int row1 = 1, col1 = (int)dateTime.DayOfWeek;
                    while (dateTime != when.Value)
                    {
                        col1++;
                        if (col1 == 7)
                        {
                            col1 = 0;
                            row1++;
                        }
                        dateTime = dateTime.AddDays(1);

                    }
                    Label label = (Label)this.FindName("MonthLabel" + row1 + col1);
                    label.Content += "\n" + eventItem.Summary;
                }
                
            }
            #endregion
        }

        private void TaskGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "TaskId" || 
                e.Column.Header.ToString() == "Priority" || 
                e.Column.Header.ToString() == "Deadline"|| 
                e.Column.Header.ToString() == "Color" ||
                e.Column.Header.ToString() == "Done" 
                )
            {
                e.Cancel = true;
                
            }
        }

        private void TaskGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveTaskButton.IsEnabled = true;

            if (SetVisible())
                SetTaskInfo();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Calendars.AuthorizationNew();
        }
        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            var item_to_remove = (string)FilesList.SelectedValue;
            var seld_task = SelectedTask();
            seld_task.Files_names.Remove(item_to_remove);
            seld_task.SaveFiles();
            FilesList.Items.Remove(item_to_remove);
            FilesList.Items.Refresh();
            RemoveFile.IsEnabled = false;

        }
        public void LoadAccounts()
        {
            this.DataContext = Calendars.Accounts;
            AccountListView.ItemsSource = Calendars.Accounts;
            this.DataContext = Calendars.Accounts;
        }

        private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveFile.Visibility = FilesList.Items.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            RemoveFile.IsEnabled = true;
        }

        private void FilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string filename = (string)FilesList.SelectedValue;
            if (filename == null)
                return;
            string sourceFile = System.IO.Path.Combine("files", filename);
            System.Diagnostics.Process.Start(sourceFile);
        }

        private void ComboBoxDone_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedGroup().GroupId != 0) 
               SetTasksGrid(SelectedGroup().GroupId);
        }
		

     

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DataContext = Calendars.Accounts;
            AccountListView.ItemsSource = Calendars.Accounts;
            this.DataContext = Calendars.Accounts;
        }

        private void Deadline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!skipValueChanged)
            {
                list_group.Find(w => w.GroupId == SelectedGroup().GroupId).
               task_list.Find(w => w.TaskId == SelectedTask().TaskId).Deadline = (DateTime?)Deadline.Value;
                SaveChanges();
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //var t = Calendars.Accounts;
            //foreach (var t1 in t) {

            //t1.}


        }
    }
}
