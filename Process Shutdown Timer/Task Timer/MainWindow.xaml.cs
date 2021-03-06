﻿using System;
using System.Collections;
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
using System.Windows.Threading;
using System.Diagnostics;
using System.ComponentModel;
using toolkit = Xceed.Wpf.Toolkit;

namespace ProcessShutdownTimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ProcessManager Manager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Manager = new ProcessManager();
            Manager.RefreshProcessList();

            TimePickerBox.Value = DateTime.Now;
            ScheduledBox.ItemsSource = Manager.ScheduledList;
            ProcessBox.ItemsSource = Manager.ProcessList;
            
            CollectionView processView = (CollectionView)CollectionViewSource.GetDefaultView(ProcessBox.ItemsSource);
            CollectionView scheduledView = (CollectionView)CollectionViewSource.GetDefaultView(ScheduledBox.ItemsSource);
            processView.Filter = ProcessFilter;

            Manager.ScheduledView = scheduledView;
            Manager.ProcessView = processView;
        }

        private bool ProcessFilter(object item)
        {
            ProcessContainer thisItem = item as ProcessContainer;
            if (thisItem.IsScheduled == true)
                return false;

            if (String.IsNullOrEmpty(FilterInput.Text))
            {
                return true;
            }
            else
            {
                return (thisItem.ProcessName.IndexOf(FilterInput.Text, StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IList selected = ProcessBox.SelectedItems;
            DateTime finalTime = new DateTime();
            // TODO: Implement proper WPF-style validation
            bool valid = true;
            if (RadioTimePicker.IsChecked == true && selected.Count > 0)
            {
                // Time Picker Box path
                if (TimePickerBox.Value != null)
                {
                    finalTime = TimePickerBox.Value ?? default(DateTime);

                    if (TimePickerBox.Value < DateTime.Now)
                    {
                        finalTime += TimeSpan.FromHours(24d);
                        MessageBox.Show("Changed time: " + finalTime.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid time.", "Slow down, partner.");
                    valid = false;
                }
            }
            else if (RadioNumberInput.IsChecked == true && selected.Count > 0)
            {
                int hours;
                int minutes;
                int seconds;
                hours = int.TryParse(Hours.Text, out hours) ? hours : 0;
                minutes = int.TryParse(Minutes.Text, out minutes) ? minutes : 0;
                seconds = int.TryParse(Seconds.Text, out seconds) ? seconds : 0;
                finalTime = DateTime.Now + new TimeSpan(hours, minutes, seconds);
                MessageBox.Show(finalTime.ToString());
            }
            else
            {
                if (selected.Count == 0)
                {
                    MessageBox.Show("Please choose a process.", "Slow down, partner.");
                    valid = false;
                }
            }
            if (valid)
            {
                Manager.ScheduleShutdown(selected, finalTime);
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(ProcessBox.ItemsSource).Refresh();
        }

        private void Time_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            bool allow = false;
            int input;
            if (int.TryParse(e.Text, out input))
            {
                if (input >= 0)
                {
                    allow = true;
                }
            }
            e.Handled = !allow;
        }
    }
}