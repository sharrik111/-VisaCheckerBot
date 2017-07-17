using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);

    public class ErrorEventArgs : EventArgs
    {
        public string Error { get; private set; }

        public DateTime Timestamp { get; private set; }

        public ErrorEventArgs(string error = null) : base()
        {
            Error = error;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return Timestamp.ToString() + " : " + Error;
        }
    }
}
