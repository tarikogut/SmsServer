using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmsLibrary
{
    public class UnknownSmsTypeException: Exception
    {
        public UnknownSmsTypeException(byte pduType) : 
			base(string.Format("Unknow SMS type. PDU type binary: {0}.", Convert.ToString(pduType, 2))) { }
    }
}
