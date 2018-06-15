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

namespace stp
{
    /// <summary>
    /// Interaction logic for Event.xaml
    /// </summary>
    public partial class Event : Window
    {
        //public Google.Apis.Calendar.v3.Data.Event ViewEvent;
        public Google.Apis.Calendar.v3.Data.Event TargetEvent;

        public Event()
        {
            InitializeComponent();
        }
        public Event(Google.Apis.Calendar.v3.Data.Event targetEvent)
        {
            InitializeComponent();

            TargetEvent = targetEvent;

            DateField.Value = TargetEvent.Start.DateTime;
            NameField.Text = TargetEvent.Summary;
            PlaceField.Text = TargetEvent.Location;
            DescrioptionField.Text = TargetEvent.Description;
            DurationField.Text = (TargetEvent.End.DateTime - TargetEvent.Start.DateTime).ToString();
            NotifyBeforeField.Text = "00:30:00";
            GuestsListView.ItemsSource = TargetEvent.Attendees;
        }
    }
}
