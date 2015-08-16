using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMToSCT2.Helpers
{
    public class ConversionException : Exception
    {
        public ConversionException()
        {
        }

        public ConversionException(String message)
            : base(message)
        {
        }

        public ConversionException(String message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
