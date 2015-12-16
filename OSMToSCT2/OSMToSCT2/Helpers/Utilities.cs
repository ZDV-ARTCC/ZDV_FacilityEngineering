using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMToSCT2.Helpers
{
    public static class Utilities
    {
        /// <summary>
        /// Parses an SCT-formatted Latitude string to a Decimal
        /// </summary>
        /// <param name="strLatitude">SCT-formatted Latitude string</param>
        /// <returns>Decimal Latitude</returns>
        public static decimal DecimalLatitudeFromString(String strLatitude)
        {
            decimal decLatitude;
            decimal decSeconds;
            decimal decMinutes;
            decimal sign;
            string[] parts;

            if (strLatitude[0] == 'N')
            {
                sign = 1;
            }
            else
            {
                sign = -1;
            }

            parts = strLatitude.Substring(1).Split('.');

            decSeconds = Decimal.Parse(parts[2] + "." + parts[3]) / 3600;
            decMinutes = Decimal.Parse(parts[1]) / 60;
            decLatitude = sign * (Decimal.Parse(parts[0]) + decMinutes + decSeconds);

            return decLatitude;
        }


        /// <summary>
        /// Parses an SCT-formatted Longitude string to a Decimal
        /// </summary>
        /// <param name="strLongitude">SCT-formatted Longitude string</param>
        /// <returns>Decimal Longitude</returns>
        public static decimal DecimalLongitudeFromString(String strLongitude)
        {
            decimal decLongitude;
            decimal decSeconds;
            decimal decMinutes;
            decimal sign;
            string[] parts;

            if (strLongitude[0] == 'E')
            {
                sign = 1;
            }
            else
            {
                sign = -1;
            }

            parts = strLongitude.Substring(1).Split('.');

            decSeconds = Decimal.Parse(parts[2] + "." + parts[3]) / 3600;
            decMinutes = Decimal.Parse(parts[1]) / 60;
            decLongitude = sign * (Decimal.Parse(parts[0]) + decMinutes + decSeconds);

            return decLongitude;
        }


        /// <summary>
        /// Formats a decimal latitude value in the SCT DMS format
        /// </summary>
        /// <param name="latitudeDecimal">Decimal Latitude value</param>
        /// <returns>SCT-formatted latitude value (ex. N039.13.25.089)</returns>
        public static String LatitudeDecimalToDMS(decimal latitudeDecimal)
        {
            String latitudeDMS;
            decimal latitudeM;
            decimal latitudeS;
            decimal latitudeSRemainder;

            latitudeDMS = "";

            if (latitudeDecimal >= 0)
            {
                latitudeDMS += "N";
            }
            else
            {
                latitudeDecimal = -latitudeDecimal;
                latitudeDMS += "S";
            }

            latitudeM = (latitudeDecimal - Math.Floor(latitudeDecimal)) * 60;
            latitudeS = Math.Round((latitudeM - Math.Floor(latitudeM)) * 60, 3);
            latitudeSRemainder = Math.Round((latitudeS - Math.Floor(latitudeS)) * 1000);

            latitudeDMS += String.Format("{0:000}.{1:00}.{2:00}.{3:000}",
                                         (int)latitudeDecimal,
                                         (int)latitudeM,
                                         (int)latitudeS,
                                         (int)latitudeSRemainder);

            return latitudeDMS;
        }

        /// <summary>
        /// Formats a decimal longitude value in the SCT DMS format
        /// </summary>
        /// <param name="longitudeDecimal">Decimal Longitude value</param>
        /// <returns>SCT-formatted longitude value (ex. W106.52.00.570)</returns>
        public static String LongitudeDecimalToDMS(decimal longitudeDecimal)
        {
            String longitudeDMS;
            decimal longitudeM;
            decimal longitudeS;
            decimal longitudeSRemainder;

            longitudeDMS = "";

            if (longitudeDecimal >= 0)
            {
                longitudeDMS += "E";
            }
            else
            {
                longitudeDecimal = -longitudeDecimal;
                longitudeDMS += "W";
            }

            longitudeM = (longitudeDecimal - Math.Floor(longitudeDecimal)) * 60;
            longitudeS = Math.Round((longitudeM - Math.Floor(longitudeM)) * 60, 3);
            longitudeSRemainder = Math.Round((longitudeS - Math.Floor(longitudeS)) * 1000);


            longitudeDMS += String.Format("{0:000}.{1:00}.{2:00}.{3:000}",
                                         (int)longitudeDecimal,
                                         (int)longitudeM,
                                         (int)longitudeS,
                                         (int)longitudeSRemainder);

            return longitudeDMS;
        }
    }
}
