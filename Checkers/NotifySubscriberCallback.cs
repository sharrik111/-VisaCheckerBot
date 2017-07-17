using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    public delegate void NotifySubscriberCallback (NotifySubscriberCallbackArgs e);

    public class NotifySubscriberCallbackArgs
    {
        public long ID { get; private set; }

        public string CountryIdentifier { get; private set; }

        public NotifySubscriberCallbackArgs(long id, string countryIdentifier)
        {
            ID = id;
            CountryIdentifier = countryIdentifier;
        }
    }
}
