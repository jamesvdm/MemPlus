﻿using System;
using System.Windows;
using System.Windows.Media;
using MemPlus.Classes;
using MemPlus.Classes.LOG;
using MemPlus.Classes.RAM;

namespace MemPlus.Windows
{
    /// <inheritdoc cref="Syncfusion.Windows.Shared.ChromelessWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private readonly RamController _ramController;
        private readonly LogController _logController;

        public MainWindow()
        {
            _logController = new LogController();
            _logController.AddLog(new ApplicationLog("Initializing MainWindow"));

            InitializeComponent();
            ChangeVisualStyle();

            _ramController = new RamController(Dispatcher, CgRamUsage,LblTotalPhysicalMemory, LblAvailablePhysicalMemory, 1000, _logController);
            _ramController.EnableMonitor();

            Application app = Application.Current;
            app.Activated += Active;
            app.Deactivated += Passive;

            _logController.AddLog(new ApplicationLog("Done initializing MainWindow"));
        }

        private void Active(object sender, EventArgs args)
        {
           _ramController.EnableMonitor();
            Overlay.Visibility = Visibility.Collapsed;
        }

        private void Passive(object sender, EventArgs args)
        {
            _ramController.DisableMonitor();
            Overlay.Visibility = Visibility.Visible;
        }
    

        internal void ChangeVisualStyle()
        {
            _logController.AddLog(new ApplicationLog("Changing MainWindow theme style"));

            StyleManager.ChangeStyle(this);
            CgRamUsage.Scales[0].Ranges[0].Stroke = new SolidColorBrush(Properties.Settings.Default.MetroColor);

            _logController.AddLog(new ApplicationLog("Done changing MainWindow theme style"));
        }

        private async void BtnClearMemory_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.AddLog(new ApplicationLog("Clearing RAM Memory"));

            try
            {
                BtnClearMemory.IsEnabled = false;

                await _ramController.ClearMemory(true);
                double ramSavings = _ramController.RamSavings / 1024 / 1024;
                if (ramSavings < 0)
                {
                    MessageBox.Show("Looks like your RAM usage has increased with " + Math.Abs(ramSavings).ToString("F2") + " MB!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("You saved " + ramSavings.ToString("F2") + " MB of RAM!", "MemPlus", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                BtnClearMemory.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MemPlus", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            _logController.AddLog(new ApplicationLog("Done clearing RAM memory"));
        }

        private void ClearLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _logController.ClearLogs();
        }

        private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ApplicationLogsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            new ApplicationLogWindow(_logController).ShowDialog();
        }
    }
}