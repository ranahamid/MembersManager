using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.MMLogger
{
    public interface ILogger
    {
        void Error(Exception exception, string message = null);
        void Warning(string message);
        void Debug(string message);
    }
}
