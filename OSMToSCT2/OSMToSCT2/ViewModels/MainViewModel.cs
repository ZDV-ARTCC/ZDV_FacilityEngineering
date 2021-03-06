﻿using OSMToSCT2.Helpers;
using OSMToSCT2.TextConverters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OSMToSCT2.ViewModels
{
    /// <summary>
    /// ViewModel for the main window
    /// </summary>
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

        /// <summary>
        /// Converts the OSM file(s) from InputOSMFilePath to SCT file(s)
        /// and saves them to OutputDirectoryPath
        /// </summary>
        /// <param name="param">Not Used</param>
        public void ConvertOSMToSCT(object param)
        {
            FileInfo inputFileInfo;
            FileInfo outputFileInfo;
            StreamReader streamReader;
            StreamWriter streamWriter;
            String osmText;
            String sctText;
            String outputDirectoryPath;
            DirectoryInfo outputDirInfo;
            OSMToSCTTextConverter converter;

            OutputConsoleText = "";
            OutputConsoleText += "Converting OSM to SCT...";

            inputFileInfo = new FileInfo(mInputOSMFilePath);

            if (!inputFileInfo.Exists)
            {
                OutputConsoleText += "Error. File not found.";
                return;
            }

            // Read in the file contents
            using (FileStream fileStream = inputFileInfo.OpenRead())
            {
                streamReader = new StreamReader(fileStream);

                osmText = streamReader.ReadToEnd();

                fileStream.Flush();
                fileStream.Close();
            }

            // Initialize the converter utility
            converter = new OSMToSCTTextConverter(mSettingsVM.ColorRunway,
                                                  mSettingsVM.ColorTaxiway,
                                                  mSettingsVM.ColorApron,
                                                  mSettingsVM.ColorTerminal,
                                                  mSettingsVM.ColorHangar,
                                                  mSettingsVM.ColorOther1);

            // Perform the conversion
            sctText = converter.Convert(osmText);

            outputDirectoryPath = Environment.ExpandEnvironmentVariables(mOutputDirectoryPath);
            outputDirInfo = new DirectoryInfo(outputDirectoryPath);

            if (!outputDirInfo.Exists)
                outputDirInfo.Create();

            outputFileInfo = new FileInfo(Path.Combine(outputDirectoryPath, Path.ChangeExtension(inputFileInfo.Name, "sct2")));
            outputFileInfo.Delete();

            // Write the SCT file(s)
            using (FileStream fileStream = outputFileInfo.OpenWrite())
            {
                streamWriter = new StreamWriter(fileStream);

                streamWriter.Write(sctText);
                streamWriter.Flush();

                fileStream.Close();

                OutputConsoleText += "SCT File Created Successfully.";
            }
        }

        /// <summary>
        /// Converts the SCT file(s) from InputSCTFilePath to OSM file(s)
        /// and saves them to OutputDirectoryPath
        /// </summary>
        /// <param name="param">Not Used</param>
        public void ConvertSCTToOSM(object param)
        {
            SCTToOSMTextConverter diagramConverter;
            SCTToOSMVideoMapConverter videomapConverter;
            StreamReader streamReader;
            StreamWriter streamWriter;
            FileInfo inputFileInfo;
            DirectoryInfo outputDirInfo;
            FileInfo outputFileInfo;
            String sctText;
            String osmText;
            String outputDirectoryPath;
            String inputFilePath;

            OutputConsoleText = "";
            OutputConsoleText += "Converting SCT to OSM...";

            inputFilePath = Environment.ExpandEnvironmentVariables(mInputSCTFilePath);
            inputFileInfo = new FileInfo(inputFilePath);

            if (!inputFileInfo.Exists)
            {
                OutputConsoleText += "Error. File not found.";
                return;
            }

            // Read in the file contents
            using (FileStream fileStream = inputFileInfo.OpenRead())
            {
                streamReader = new StreamReader(fileStream);

                sctText = streamReader.ReadToEnd();

                fileStream.Flush();
                fileStream.Close();
            }

            if (mSettingsVM.IsDiagramMode)
            {
                // Initialize the converter utility
                diagramConverter = new SCTToOSMTextConverter(mSettingsVM.ColorRunway,
                                                      mSettingsVM.ColorTaxiway,
                                                      mSettingsVM.ColorApron,
                                                      mSettingsVM.ColorTerminal,
                                                      mSettingsVM.ColorHangar,
                                                      mSettingsVM.ColorOther1);
                // Perform the conversion
                osmText = diagramConverter.Convert(sctText);
            }
            else
            {
                // Initialize the converter utility
                videomapConverter = new SCTToOSMVideoMapConverter();

                // Perform the conversion
                osmText = videomapConverter.Convert(sctText);
            }

            outputDirectoryPath = Environment.ExpandEnvironmentVariables(mOutputDirectoryPath);
            outputDirInfo = new DirectoryInfo(outputDirectoryPath);
            
            if(!outputDirInfo.Exists)
                outputDirInfo.Create();

            outputFileInfo = new FileInfo(Path.Combine(outputDirectoryPath, Path.ChangeExtension(inputFileInfo.Name, "osm")));

            if (outputFileInfo.Exists)
                outputFileInfo.Delete();

            // Write the OSM file(s)
            using (FileStream fileStream = outputFileInfo.OpenWrite())
            {
                streamWriter = new StreamWriter(fileStream, Encoding.UTF8);

                streamWriter.Write(osmText);
                streamWriter.Flush();

                fileStream.Close();

                OutputConsoleText += "OSM File Created Successfully.";
            }

        }
    }
}
