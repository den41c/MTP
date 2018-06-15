using System;
using System.Threading;
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace stp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static System.Globalization.Calendar calendar = DateTimeFormatInfo.CurrentInfo.Calendar;
        public List<Setting> Setting;
        private bool chang;
        private bool skipValueChanged;
        private List<Classes.Group> list_group = new List<Classes.Group>();

        public List<Events> AllEventsList;
        public class Events
        {
            public Account Account { get; set; }
            public string ID { get; set; }
            public string Summary { get; set; }
            public DateTime? EventDate { get; set; }
        }
        public MainWindow()
        {

            Calendars.Accounts = new System.Collections.ObjectModel.ObservableCollection<Account>();
            string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SerializedAccounts.bin").ToString();
            if (File.Exists(filePath))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                if (stream.Length != 0)
                {
                    Calendars.AccountsForSerialization = (AccountsForSerialization)formatter.Deserialize(stream);
                    Calendars.Accounts = Calendars.AccountsForSerialization.Accounts;
                }
            }
            InitializeComponent();
            TaskGrid.DataContext = new ToDoTask();
            this.DataContext = this;

            SetVisible();
           

            var cmd = SqlClient.CreateCommand("select * from groups");
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list_group.Add(new Classes.Group()
                {
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
                TaskGrid.ItemsSource = list_group.Find(w => (w.GroupId == GroupCode) && (w.task_list != null)).task_list.Where(w => cb == 1 ? !w.Done : true);
            }
            else
            {
                TaskGrid.ItemsSource = new List<ToDoTask>();
                //TaskListBox.ItemsSource = new List<ToDoTask>();
            }


            chang = false;
            TaskGrid.Items.Refresh();
            chang = true;
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


            var cmd = SqlClient.CreateCommand(@"insert into Tasks (taskname, groupid, done, description, priority) 
                                                    values(@taskname,@groupid,@done,@description,@priority);
                                                select last_insert_rowid();
                                                    ");
            cmd.Parameters.Add(new SQLiteParameter("taskname", TaskTextBox.Text));
            cmd.Parameters.Add(new SQLiteParameter("groupid", group.GroupId));
            cmd.Parameters.Add(new SQLiteParameter("done", "-"));
            cmd.Parameters.Add(new SQLiteParameter("description", ""));
            cmd.Parameters.Add(new SQLiteParameter("priority", "Unimportant"));
            var newTaskId = (int)(long)cmd.ExecuteScalar();
            list_group.Find(w => w.GroupId == group.GroupId).task_list.Add(new ToDoTask()
            {
                TaskName = TaskTextBox.Text,
                TaskId = newTaskId,
                Deadline = null,
                Files_names = new List<string>(),
                _priority = "Unimportant"
            });
            SetTasksGrid(group.GroupId);
            GroupListBox.Items.Refresh();
            TaskTextBox.Text = string.Empty;
            RefreshCalendarTab();
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
            chang = false;
            TaskGrid.Items.Refresh();
            chang = true;
            TaskTextBox.IsEnabled = true;
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
            chang = false;
            TaskGrid.Items.Refresh();
            chang = true;
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
            AddRemoveButton.Content = "Add";
            EventButton.Visibility = Visibility.Hidden;
            if (sT.EventId != null && sT.EventId != string.Empty)
            {
                var Event = Calendars.GetEventById(sT.EventId);
                EventButton.Content = Event.Summary;
                EventButton.Visibility = Visibility.Visible;
                AddRemoveButton.Content = "Remove";
            }

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
            Event_Label.Visibility = visible;
            AddRemoveButton.Visibility = visible;
            EventButton.Visibility = visible;

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
            RefreshCalendarTab();

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
            for (int i = 1; i < 7; i++)
            {
                ListBox listBox = new ListBox();
                listBox.MouseDoubleClick += ListBox_MouseDoubleClick;
                ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);
                listBox.Name = "DayListBox" + i + 1;
                Grid.SetRow(listBox, i);
                Grid.SetColumn(listBox, 1);
                RegisterName(listBox.Name, listBox);
                Daily.Children.Add(listBox);
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
                    if (i != 0 && j != 0)
                    {
                        ListBox listBox = new ListBox();
                        listBox.MouseDoubleClick += ListBox_MouseDoubleClick;
                        ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);
                        listBox.Name = "WeekListBox" + i + j;
                        Grid.SetRow(listBox, i);
                        Grid.SetColumn(listBox, j);
                        RegisterName(listBox.Name, listBox);
                        Weekly.Children.Add(listBox);
                    }
                }
            }
            #endregion
            #region MonthGrid

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
                    if (i != 0)
                    {
                        ListBox listBox = new ListBox();
                        listBox.MouseDoubleClick += ListBox_MouseDoubleClick;
                        ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Disabled);
                        listBox.Name = "MonthListBox" + i + j;
                        //listBox.Items.Add("");
                        Grid.SetRow(listBox, i);
                        Grid.SetColumn(listBox, j);
                        RegisterName(listBox.Name, listBox);
                        Monthly.Children.Add(listBox);
                    }
                }
            }
            dateTime = Calendars.SelectedDateTime;
            int currentMonth = dateTime.Month;
            while (dateTime.Day != 1)
            {
                dateTime = dateTime.AddDays(-1);
            }
            int row = 1, col = (int)dateTime.DayOfWeek;
            while (dateTime.Month == currentMonth)
            {
                ListBox listBox = (ListBox)FindName("MonthListBox" + row + col);
                col++;
                if (col == 7)
                {
                    col = 0;
                    row++;
                }
                var t = new ListBoxItem();
                t.IsEnabled = false;
                t.Content = dateTime.ToShortDateString();
                listBox.Items.Add(t);
                dateTime = dateTime.AddDays(1);
            }
            #endregion

            Calendars.RunJob();
            LoadAccounts();
            SetEventList();

            PopulateDaylyEvents();
            PopulateWeeklyEvents();
            PopulateMonthlyEvents();

            PopulateDaylyTasks();
            PopulateWeeklyTasks();
            PopulateMonthlyTasks();
        }

        private void RefreshCalendarTab()
        {
            CleanDaylyLists();
            CleanMonthlyLists();
            CleanWeeklyLists();

            RecreateMonthlyLabels();
            RecreateWeeklyLabels();

            PopulateDaylyEvents();
            PopulateWeeklyEvents();
            PopulateMonthlyEvents();

            PopulateDaylyTasks();
            PopulateWeeklyTasks();
            PopulateMonthlyTasks();
        }
        private void RecreateWeeklyLabels()
        {
            List<Label> removeList = new List<Label>();
            foreach (var item in Weekly.Children)
            {
                if (item.GetType() == typeof(Label))
                    removeList.Add((Label)item);
            }
            foreach (var item in removeList)
            {
                Weekly.Children.Remove(item);
            }

            DateTime dateTime = Calendars.SelectedDateTime;
            for (int i = 1; i < 8; i++)
            {
                Label label = new Label();
                label.Content = dateTime.ToString("ddd") + ' ' + dateTime.ToShortDateString();
                dateTime = dateTime.AddDays(1);
                Grid.SetColumn(label, i);
                Weekly.Children.Add(label);
            }
            Label l0 = new Label();
            l0.Content = "Time";
            Grid.SetRow(l0, 0);
            Weekly.Children.Add(l0);
            Label l1 = new Label();
            l1.Content = "00:00";
            Grid.SetRow(l1, 1);
            Weekly.Children.Add(l1);
            Label l2 = new Label();
            l2.Content = "04:00";
            Grid.SetRow(l2, 2);
            Weekly.Children.Add(l2);
            Label l3 = new Label();
            l3.Content = "08:00";
            Grid.SetRow(l3, 3);
            Weekly.Children.Add(l3);
            Label l4 = new Label();
            l4.Content = "12:00";
            Grid.SetRow(l4, 4);
            Weekly.Children.Add(l4);
            Label l5 = new Label();
            l5.Content = "16:00";
            Grid.SetRow(l5, 5);
            Weekly.Children.Add(l5);
            Label l6 = new Label();
            l6.Content = "20:00";
            Grid.SetRow(l6, 6);
            Weekly.Children.Add(l6);
        }
        private void RecreateMonthlyLabels()
        {
            List<Label> removeList = new List<Label>();
            foreach (var item in Monthly.Children)
            {
                if (item.GetType() == typeof(Label))
                    removeList.Add((Label)item);
            }
            foreach (var item in removeList)
                Monthly.Children.Remove(item);

            DateTime dateTime = Calendars.SelectedDateTime;
            int currentMonth = dateTime.Month;
            while (dateTime.Day != 1)
            {
                dateTime = dateTime.AddDays(-1);
            }
            int row = 1, col = (int)dateTime.DayOfWeek;
            while (dateTime.Month == currentMonth)
            {
                ListBox listBox = (ListBox)FindName("MonthListBox" + row + col);
                col++;
                if (col == 7)
                {
                    col = 0;
                    row++;
                }
                var t = new ListBoxItem();
                t.IsEnabled = false;
                t.Content = dateTime.ToShortDateString();
                listBox.Items.Add(t);
                dateTime = dateTime.AddDays(1);
            }
            Label l0 = new Label();
            l0.Content = "Sun";
            Grid.SetColumn(l0, 0);
            Monthly.Children.Add(l0);
            Label l1 = new Label();
            l1.Content = "Mon";
            Grid.SetColumn(l1, 1);
            Monthly.Children.Add(l1);
            Label l2 = new Label();
            l2.Content = "Tue";
            Grid.SetColumn(l2, 2);
            Monthly.Children.Add(l2);
            Label l3 = new Label();
            l3.Content = "Wed";
            Grid.SetColumn(l3, 3);
            Monthly.Children.Add(l3);
            Label l4 = new Label();
            l4.Content = "Thu";
            Grid.SetColumn(l4, 4);
            Monthly.Children.Add(l4);
            Label l5 = new Label();
            l5.Content = "Fri";
            Grid.SetColumn(l5, 5);
            Monthly.Children.Add(l5);
            Label l6 = new Label();
            l6.Content = "Sat";
            Grid.SetColumn(l6, 6);
            Monthly.Children.Add(l6);
        }

        private void CleanDaylyLists()
        {
            for (int i = 1; i < 7; i++)
            {
                ListBox listBox = (ListBox)FindName("DayListBox" + i + 1);
                listBox.Items.Clear();
            }
        }
        private void CleanWeeklyLists()
        {
            for (int i = 1; i < 7; i++)
            {
                for (int j = 1; j < 8; j++)
                {
                    ListBox listBox = (ListBox)FindName("WeekListBox" + i + j);
                    listBox.Items.Clear();
                }
            }
        }
        private void CleanMonthlyLists()
        {
            for (int i = 1; i < 7; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    ListBox listBox = (ListBox)FindName("MonthListBox" + i + j);
                    listBox.Items.Clear();
                }
            }
        }

        private void PopulateDaylyEvents()
        {
            DateTime dateTime;
            foreach (var account in Calendars.Accounts)
            {
                if (account.Enabled && account.Events.Items != null && account.Events.Items.Count > 0)
                {
                    foreach (var eventItem in account.Events.Items)
                    {
                        DateTime? when = eventItem.Start.DateTime;
                        if (when == null)
                        {
                            dateTime = DateTime.ParseExact(eventItem.Start.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        }
                        else { dateTime = when.Value; }
                        if (dateTime.DayOfYear != Calendars.SelectedDateTime.DayOfYear || Calendars.SelectedDateTime.Year != dateTime.Year)
                        {
                            continue;
                        }
                        int row1 = dateTime.Hour / 4 + 1, col1 = 1;
                        ListBox listBox = (ListBox)FindName("DayListBox" + row1 + col1);
                        var TextBlock = new TextBlock()
                        {
                            Text = eventItem.Summary,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(account.Color), //todo
                            Tag = eventItem
                        };
                        
                        listBox.Items.Add(TextBlock);
                       // listBox.Items.Add(eventItem.Summary);
                    }
                }
            }
        }
        private void PopulateWeeklyEvents()
        {
            DateTime dateTime;
            foreach (var account in Calendars.Accounts)
            {
                if (account.Enabled && account.Events.Items != null && account.Events.Items.Count > 0)
                {
                    foreach (var eventItem in account.Events.Items)
                    {
                        DateTime? when = eventItem.Start.DateTime;
                        if (when == null)
                        {
                            dateTime = DateTime.ParseExact(eventItem.Start.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        }
                        else { dateTime = when.Value; }
                        if (calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) != calendar.GetWeekOfYear(Calendars.SelectedDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) || Calendars.SelectedDateTime.Year != dateTime.Year)
                        {
                            continue;
                        }
                        int row1 = dateTime.Hour / 4 + 1, col1 = (int)dateTime.DayOfWeek + 1;
                        ListBox listBox = (ListBox)FindName("WeekListBox" + row1 + col1);
                        var TextBlock = new TextBlock()
                        {
                            Text = eventItem.Summary,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(account.Color), //todo
                            Tag = eventItem
                        };
                        //listBox.MouseDoubleClick += ListBox_MouseDoubleClick;
                        listBox.Items.Add(TextBlock);
                        //listBox.Items.Add(eventItem.Summary);
                    }
                }
            }
        }
        private void PopulateMonthlyEvents()
        {
            DateTime dateTime;
            foreach (var account in Calendars.Accounts)
            {
                if (account.Enabled && account.Events.Items != null && account.Events.Items.Count > 0)
                {
                    foreach (var eventItem in account.Events.Items)
                    {
                        DateTime? when = eventItem.Start.DateTime;
                        if (when == null)
                        {
                            dateTime = DateTime.ParseExact(eventItem.Start.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        }
                        else { dateTime = when.Value; }
                       
                        if (Calendars.SelectedDateTime.Month != dateTime.Month || Calendars.SelectedDateTime.Year != dateTime.Year)
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
                        ListBox listBox = (ListBox)FindName("MonthListBox" + row1 + col1);
                        var TextBlock = new TextBlock()
                        {
                            Text = eventItem.Summary,
                            FontWeight = FontWeights.Bold,
                            Foreground = new SolidColorBrush(account.Color), //todo
                            Tag = eventItem
                        };
                        //listBox.MouseDoubleClick += ListBox_MouseDoubleClick;
                        listBox.Items.Add(TextBlock);
                        //listBox.Items.Add(eventItem.Summary);
                    }
                }
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            
            
            var list = (ListBox)sender;
            
            var item = (TextBlock)list.SelectedItem;

            if (item.Tag is ItemList)
            {
                return;
            }
            var Event = (Google.Apis.Calendar.v3.Data.Event)(item.Tag);
            
            if (JustVievCalendar)
            {
                //todo den41c
                var task = SelectedTask();
                task.OldDeadline = task.Deadline;
                task.EventId = Event.Id;
                task.Deadline = Event.Start.DateTime;
                
                var cmd = SqlClient.CreateCommand(@"update tasks set EventId = @EventId  
                                                    where taskid=@taskid ");
                cmd.Parameters.Add(new SQLiteParameter("EventId", task.EventId));
                cmd.Parameters.Add(new SQLiteParameter("taskid", task.TaskId));
                cmd.ExecuteNonQuery();

                cmd = SqlClient.CreateCommand(@"update tasks set OldDeadline = @OldDeadline  
                                                    where taskid=@taskid ");
                cmd.Parameters.Add(new SQLiteParameter("OldDeadline", task.OldDeadline));
                cmd.Parameters.Add(new SQLiteParameter("taskid", task.TaskId));
                cmd.ExecuteNonQuery();

                cmd = SqlClient.CreateCommand(@"update tasks set Deadline = @Deadline  
                                                    where taskid=@taskid ");
                cmd.Parameters.Add(new SQLiteParameter("Deadline", task.Deadline));
                cmd.Parameters.Add(new SQLiteParameter("taskid", task.TaskId));
                cmd.ExecuteNonQuery();

                SetTaskInfo();
                AddAccount.IsEnabled = true;
                DeleteAccount.IsEnabled = true;
                Tabcontrol.SelectedItem = Tasks;
            }
            else
            {
                var eventForm = new Event(Event);
                eventForm.Show();
            }
            JustVievCalendar = false;


        }

        private void PopulateDaylyTasks()
        {
            DateTime dateTime;
            foreach (var group in list_group)
            {
                if (true/*account.Enabled && account.Events.Items != null && account.Events.Items.Count > 0*/)
                {
                    foreach (var task in group.task_list)
                    {
                        DateTime? when = task.Deadline;
                        if (when == null) { continue; }
                        dateTime = when.Value;
                        if (dateTime.DayOfYear != Calendars.SelectedDateTime.DayOfYear || Calendars.SelectedDateTime.Year != dateTime.Year)
                        {
                            continue;
                        }
                        int row1 = dateTime.Hour / 4 + 1, col1 = 1;
                        ListBox listBox = (ListBox)FindName("DayListBox" + row1 + col1);

                        var TextBlock = new TextBlock()
                        {
                            Text = task.TaskName,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Green,
                            Tag = new ItemList { ID = task.TaskId.ToString(), Type = TypeOfItem.ToDoTask }
                        };

                        listBox.Items.Add(TextBlock);
                    }
                }
            }
        }

        

        private void PopulateWeeklyTasks()
        {
            DateTime dateTime;
            foreach (var group in list_group)
            {
                if (true/*account.Enabled && account.Events.Items != null && account.Events.Items.Count > 0*/)
                {
                    foreach (var task in group.task_list)
                    {
                        DateTime? when = task.Deadline;
                        if (when == null) { continue; }
                        dateTime = when.Value;
                        if (calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) != calendar.GetWeekOfYear(Calendars.SelectedDateTime, CalendarWeekRule.FirstDay, DayOfWeek.Sunday) || Calendars.SelectedDateTime.Year != dateTime.Year)
                        {
                            continue;
                        }
                        int row1 = dateTime.Hour / 4 + 1, col1 = (int)dateTime.DayOfWeek + 1;
                        ListBox listBox = (ListBox)FindName("WeekListBox" + row1 + col1);
                        var TextBlock = new TextBlock()
                        {
                            Text = task.TaskName,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Green,
                            Tag = new ItemList { ID = task.TaskId.ToString(), Type = TypeOfItem.ToDoTask }
                        };
                        
                        listBox.Items.Add(TextBlock);
                        
                    }
                }
            }
        }
        private void PopulateMonthlyTasks()
        {
            DateTime dateTime;
            foreach (var group in list_group)
            {
                if (true/*account.Enabled && account.Events.Items != null && account.Events.Items.Count > 0*/)
                {
                    foreach (var task in group.task_list)
                    {
                        DateTime? when = task.Deadline;
                        if (when == null) { continue; }
                        dateTime = when.Value;
                        if (Calendars.SelectedDateTime.Month != dateTime.Month || Calendars.SelectedDateTime.Year != dateTime.Year)
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
                        ListBox listBox = (ListBox)FindName("MonthListBox" + row1 + col1);
                        var TextBlock = new TextBlock()
                        {
                            Text = task.TaskName,
                            FontWeight = FontWeights.Bold,
                            Foreground = Brushes.Green,
                            Tag = new ItemList { ID = task.TaskId.ToString(), Type = TypeOfItem.ToDoTask }
                        };
                       

                        listBox.Items.Add(TextBlock);
                    }
                }
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           


        }

        private void TaskGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == "TaskId" ||
                e.Column.Header.ToString() == "Priority" ||
                e.Column.Header.ToString() == "Deadline" ||
                e.Column.Header.ToString() == "Color" ||
                e.Column.Header.ToString() == "Done" ||
                e.Column.Header.ToString() == "TaskName" ||
                e.Column.Header.ToString() == "EventId" ||
                e.Column.Header.ToString() == "OldDeadline"
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
            SetEventList();
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
            list_group.ForEach(w => w.All = ComboBoxDone.SelectedIndex == 0);
            if (SelectedGroup().GroupId != 0)
                SetTasksGrid(SelectedGroup().GroupId);
            GroupListBox.Items.Refresh();

        }



        private void Deadline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!skipValueChanged)
            {
                list_group.Find(w => w.GroupId == SelectedGroup().GroupId).
               task_list.Find(w => w.TaskId == SelectedTask().TaskId).Deadline = (DateTime?)Deadline.Value;
                SaveChanges();
            }
            RefreshCalendarTab();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (chang)
            {
                SetTasksGrid(SelectedGroup().GroupId); chang = false;
            }
            GroupListBox.Items.Refresh();
        }

        private void CheckBox_GotFocus(object sender, RoutedEventArgs e)
        {
            chang = true;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var set = new Edit(this);
            set.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            set.Show();
            Setting = set.Setting;
        }      

        private void Window_Closed(object sender, EventArgs e)
        {
            Calendars.AccountsForSerialization.Accounts = Calendars.Accounts;
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "SerializedAccounts.bin").ToString(), FileMode.Create, FileAccess.Write);
            formatter.Serialize(stream, Calendars.AccountsForSerialization);
            stream.Close();
        }

        private void LeftArrow_Click(object sender, RoutedEventArgs e)
        {
            switch (comboBoxCalendarView.SelectedIndex)
            {
                case 0:
                    Calendars.SelectedDateTime = Calendars.SelectedDateTime.AddDays(-1);
                    break;
                case 1:
                    Calendars.SelectedDateTime = Calendars.SelectedDateTime.AddDays(-7);
                    break;
                case 2:
                    Calendars.SelectedDateTime = Calendars.SelectedDateTime.AddMonths(-1);
                    break;
                default:
                    break;
            }
            RefreshCalendarTab();
        }

        private void RightArrow_Click(object sender, RoutedEventArgs e)
        {
            switch (comboBoxCalendarView.SelectedIndex)
            {
                case 0:
                    Calendars.SelectedDateTime = Calendars.SelectedDateTime.AddDays(1);
                    break;
                case 1:
                    Calendars.SelectedDateTime = Calendars.SelectedDateTime.AddDays(7);
                    break;
                case 2:
                    Calendars.SelectedDateTime = Calendars.SelectedDateTime.AddMonths(1);
                    break;
                default:
                    break;
            }
            RefreshCalendarTab();
        }

        public void SetEventList()
        {
            AllEventsList = new List<Events>();

            foreach (var acc in Calendars.Accounts.Where(w=>w.ToDoEnable))
            {
                foreach (var evt in acc.Events.Items)
                {

                    AllEventsList.Add(new Events()
                    {
                        Account = acc,
                        ID = evt.Id,
                        Summary = evt.Summary,
                        EventDate = evt.Start.DateTime
                    });
                }
            }
            EventsListBox.Items.Clear();
            AllEventsList.OrderBy(w => w.EventDate).Where(w => w.EventDate > DateTime.Now).
                Select(w => w).Take(10).ToList().ForEach(w => EventsListBox.Items.Add(w));

        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListView.SelectedIndex >= 0)
            {
                DirectoryInfo di = new DirectoryInfo(Calendars.Accounts[AccountListView.SelectedIndex].PathToCredentials);
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                Calendars.Accounts.Remove(Calendars.Accounts[AccountListView.SelectedIndex]);
            }
           
            SetEventList();
        }
        private ToDoTask DropTask;
        
        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var group = SelectedGroup();

            DropTask = group.task_list.Where(w => w.TaskName == ((TextBox)sender).Text).First();
            object item = DropTask;
            if (item != null)
                DragDrop.DoDragDrop(TaskGrid, item, DragDropEffects.Move);

        }

        private void Control_Drop(object sender, DragEventArgs e)
        {
            var group_id = (int)((Label)sender).Tag;
            var group_to_remove_task = SelectedGroup();
            group_to_remove_task.task_list.Remove(DropTask);
            list_group.Where(w => w.GroupId == group_id).First().task_list.Add(DropTask);
            GroupListBox.Items.Refresh();

            TaskGrid.Items.Refresh();
            GroupListBox.SelectedItem = list_group.Find(w => (w.GroupId == group_id));

            var cmd = SqlClient.CreateCommand(@"update tasks set groupid = @groupid  
                                                    where taskid=@taskid ");
            cmd.Parameters.Add(new SQLiteParameter("groupid", group_id));
            cmd.Parameters.Add(new SQLiteParameter("taskid", DropTask.TaskId));
            cmd.ExecuteNonQuery();

        }

        private void EventButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTask().EventId == null || SelectedTask().EventId == string.Empty) { return; }
            var Event = Calendars.GetEventById(SelectedTask().EventId);
            var eventForm = new Event(Event);
            eventForm.Show();
        }
        bool JustVievCalendar = false;
        private void AddRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddRemoveButton.Content.ToString() == "Remove")
            {
                var task = SelectedTask();
                task.EventId = string.Empty;
                task.Deadline = task.OldDeadline;

                var cmd = SqlClient.CreateCommand(@"update tasks set EventId = @EventId  
                                                    where taskid=@taskid ");
                cmd.Parameters.Add(new SQLiteParameter("EventId", task.EventId));
                cmd.Parameters.Add(new SQLiteParameter("taskid", task.TaskId));
                cmd.ExecuteNonQuery();

                cmd = SqlClient.CreateCommand(@"update tasks set Deadline = @Deadline  
                                                    where taskid=@taskid ");
                cmd.Parameters.Add(new SQLiteParameter("Deadline", task.Deadline));
                cmd.Parameters.Add(new SQLiteParameter("taskid", task.TaskId));
                cmd.ExecuteNonQuery();
                SetTaskInfo();
                RefreshCalendarTab();
                return;
            }
            //AddAccount.IsEnabled = false;
            //DeleteAccount.IsEnabled = false;
            JustVievCalendar = true;
            Tabcontrol.SelectedItem = Calendar;


        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            RefreshCalendarTab();
        }

        private void EventsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = (ListBox)sender;

            var item = (Events)list.SelectedItem;
            if (item == null) { return; }

            var Event = Calendars.GetEventById(item.ID);
            var eventForm = new Event(Event);
            eventForm.Show();

        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            RefreshCalendarTab();
        }

        private void Calendar_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            RefreshCalendarTab();
        }

    }
}
