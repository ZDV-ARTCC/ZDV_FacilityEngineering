using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMToSCT2.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private String mColorRunway;
        private String mColorTaxiway;
        private String mColorApron;
        private String mColorTerminal;
        private String mColorHangar;
        private String mColorOther1;

        public SettingsViewModel()
        {

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public String ColorRunway
        {
            get
            {
                return mColorRunway;
            }
            set
            {
                if (mColorRunway != value)
                {
                    mColorRunway = value;
                    OnPropertyChanged("ColorRunway");
                }
            }
        }

        public String ColorTaxiway
        {
            get
            {
                return mColorTaxiway;
            }
            set
            {
                if (mColorTaxiway != value)
                {
                    mColorTaxiway = value;
                    OnPropertyChanged("ColorTaxiway");
                }
            }
        }

        public String ColorApron
        {
            get
            {
                return mColorApron;
            }
            set
            {
                if (mColorApron != value)
                {
                    mColorApron = value;
                    OnPropertyChanged("ColorApron");
                }
            }
        }

        public String ColorTerminal
        {
            get
            {
                return mColorTerminal;
            }
            set
            {

                if (mColorTerminal != value)
                {
                    mColorTerminal = value;
                    OnPropertyChanged("ColorTerminal");
                }
            }
        }

        public String ColorHangar
        {
            get
            {
                return mColorHangar;
            }
            set
            {

                if (mColorHangar != value)
                {
                    mColorHangar = value;
                    OnPropertyChanged("ColorHangar");
                }
            }
        }

        public String ColorOther1
        {
            get
            {
                return mColorOther1;
            }
            set
            {
                if (mColorOther1 != value)
                {
                    mColorOther1 = value;
                    OnPropertyChanged("ColorOther1");
                }
            }
        }
    }
}
