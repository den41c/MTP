﻿using System;
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

        public Event(Google.Apis.Calendar.v3.Data.Event ViewEvent)
        {
            InitializeComponent();

            //ViewEvent.
        }

        public Event()
        {
            InitializeComponent();
        }
    }
}
