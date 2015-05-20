using OSMToSCT2.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OSMToSCT2.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private String mInputOSMFilePath;
        private String mInputSCTFilePath;
        private String mOutputDirectoryPath;

        private SettingsViewModel mSettingsVM;

        private String mOutputConsoleText;

        private RelayCommand mConvertOSMToSCTCommand;
        private RelayCommand mConvertSCTToOSMCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            mInputOSMFilePath = "";
            mInputSCTFilePath = "";
            mOutputDirectoryPath = "";

            mSettingsVM = new SettingsViewModel();

            mOutputConsoleText = "";

            mConvertOSMToSCTCommand = new RelayCommand(ConvertOSMToSCT, CanConvertOSMToSCT);
            mConvertSCTToOSMCommand = new RelayCommand(ConvertSCTToOSM, CanConvertSCTToOSM);
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public String InputOSMFilePath
        {
            get
            {
                return mInputOSMFilePath;
            }
            set
            {
                if (mInputOSMFilePath != value)
                {
                    mInputOSMFilePath = value;
                    OnPropertyChanged("InputOSMFilePath");
                }
            }
        }

        public String InputSCTFilePath
        {
            get
            {
                return mInputSCTFilePath;
            }
            set
            {
                if (mInputSCTFilePath != value)
                {
                    mInputSCTFilePath = value;
                    OnPropertyChanged("InputSCTFilePath");
                }
            }
        }

        public String OutputDirectoryPath
        {
            get
            {
                return mOutputDirectoryPath;
            }
            set
            {
                if (mOutputDirectoryPath != value)
                {
                    mOutputDirectoryPath = value;
                    OnPropertyChanged("OutputDirectoryPath");
                }
            }
        }

        public SettingsViewModel SettingsVM
        {
            get
            {
                return mSettingsVM;
            }
        }

        public String OutputConsoleText
        {
            get
            {
                return mOutputConsoleText;
            }
            set
            {
                if (mOutputConsoleText != value)
                {
                    mOutputConsoleText = value;
                    OnPropertyChanged("OutputConsoleText");
                }
            }
        }

        public ICommand ConvertOSMToSCTCommand
        {
            get
            {
                return mConvertOSMToSCTCommand;
            }
        }

        public ICommand ConvertSCTToOSMCommand
        {
            get
            {
                return mConvertSCTToOSMCommand;
            }
        }

        public bool CanConvertOSMToSCT(object param)
        {
            return true;
        }

        public bool CanConvertSCTToOSM(object param)
        {
            return true;
        }

        public void ConvertOSMToSCT(object param)
        {
            OutputConsoleText = "";
            OutputConsoleText += "Converting OSM to SCT...";
        }

        public void ConvertSCTToOSM(object param)
        {
            OutputConsoleText = "";
            OutputConsoleText += "Converting SCT to OSM...";
        }
    }
}
