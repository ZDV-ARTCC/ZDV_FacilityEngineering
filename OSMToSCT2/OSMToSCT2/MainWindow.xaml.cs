using OSMToSCT2.ViewModels;
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

namespace OSMToSCT2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel mViewModel;

        public MainWindow()
        {
            mViewModel = new MainViewModel();
            InitializeFromUserSettings();
            DataContext = mViewModel;

            InitializeComponent();

            Closing += MainWindow_Closing;
        }

        protected void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveUserSettings();
        }

        /// <summary>
        /// Initializes the ViewModel from user Settings
        /// </summary>
        protected void InitializeFromUserSettings()
        {
            mViewModel.InputOSMFilePath = Properties.Settings.Default.InputOSMFilePath;
            mViewModel.InputSCTFilePath = Properties.Settings.Default.InputSCTFilePath;
            mViewModel.OutputDirectoryPath = Properties.Settings.Default.OutputDirectory;

            mViewModel.SettingsVM.ColorApron = Properties.Settings.Default.ColorApron;
            mViewModel.SettingsVM.ColorHangar = Properties.Settings.Default.ColorHangar;
            mViewModel.SettingsVM.ColorOther1 = Properties.Settings.Default.ColorOther1;
            mViewModel.SettingsVM.ColorRunway = Properties.Settings.Default.ColorRunway;
            mViewModel.SettingsVM.ColorTaxiway = Properties.Settings.Default.ColorTaxiway;
            mViewModel.SettingsVM.ColorTerminal = Properties.Settings.Default.ColorTerminal;
        }

        /// <summary>
        /// Saves the user's settings
        /// </summary>
        protected void SaveUserSettings()
        {
            Properties.Settings.Default.InputOSMFilePath = mViewModel.InputOSMFilePath;
            Properties.Settings.Default.InputSCTFilePath = mViewModel.InputSCTFilePath;
            Properties.Settings.Default.OutputDirectory = mViewModel.OutputDirectoryPath;

            Properties.Settings.Default.ColorApron = mViewModel.SettingsVM.ColorApron;
            Properties.Settings.Default.ColorHangar = mViewModel.SettingsVM.ColorHangar;
            Properties.Settings.Default.ColorOther1 = mViewModel.SettingsVM.ColorOther1;
            Properties.Settings.Default.ColorRunway = mViewModel.SettingsVM.ColorRunway;
            Properties.Settings.Default.ColorTaxiway = mViewModel.SettingsVM.ColorTaxiway;
            Properties.Settings.Default.ColorTerminal = mViewModel.SettingsVM.ColorTerminal;

            Properties.Settings.Default.Save();
        }
    }
}
