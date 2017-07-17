using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using Checkers.Storage;

namespace Checkers.Lithuania
{
    public class LithuaniaChecker : VisaChecker, IDisposable
    {
        #region Ctors

        public LithuaniaChecker(NotifySubscriberCallback callback, long timeout = 300000) : base(callback, timeout)
        {
            Timer = new Timer(Request, null, 0L, timeout);
        }

        #endregion

        #region Constants

        public const string Identifier = "LT";

        private const string RequestBaseAddress = "https://evas2.urm.lt/calendar/json?";

        private const string RequestFreeDates = RequestBaseAddress + "_d=&_aby=3&_cry=6&_c=1&_t=";

        private const string RequestBusyDates = RequestBaseAddress + "_d=&_aby=3&_cry=6&_c=1&_b=1";

        #endregion

        #region Properties

        private Timer Timer { get; set; }

        #endregion

        #region Logic

        public override bool Subscribe(long id)
        {
            bool result = subscribers.Add(id);
            if (result)
            {
                if(FreeDates.Count != 0)
                {
                    Callback?.Invoke(new NotifySubscriberCallbackArgs(id, Identifier));
                }
            }
            return result;
        }

        public override bool Unsubscribe(long id)
        {
            return subscribers.Remove(id);
        }

        public override void Save(IStorageService service)
        {
            StringBuilder stringToSave = new StringBuilder();
            subscribers.ToList().ForEach(id => stringToSave.Append(id + "\n"));
            service.Save(stringToSave.ToString());
        }

        public override void Load(IStorageService service)
        {
            string subscribersList = service.Load();
            foreach(string id in subscribersList.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                subscribers.Add(Convert.ToInt64(id));
            }
        }

        private void Request(object state)
        {
            using (var client = new WebClient())
            {
                // client.Proxy = new WebProxy("46.251.49.21", 8080) { BypassProxyOnLocal = false };
                client.Headers.Add("X-Requested-With", "XMLHttpRequest");
                try
                {
                    var response = client.DownloadString(RequestFreeDates);
                    var freeDatesResponse = JsonConvert.DeserializeObject<List<string>>(response);
                    freeDatesResponse.RemoveEmptyItems();
                    FreeDates = freeDatesResponse;
                    OnFreeDatesNotify(Identifier);
                    LastUpdate = DateTime.Now;
                }
                catch (Exception e)
                {
                    OnErrorOccured("Fetching free dates error: " + e.Message);
                }
                try
                {
                    var response = client.DownloadString(RequestBusyDates);
                    var busyDatesResponse = JsonConvert.DeserializeObject<List<string>>(response);
                    busyDatesResponse.RemoveEmptyItems();
                    BusyDates = busyDatesResponse;
                }
                catch (Exception e)
                {
                    // BusyDates.Add("Fetching busy dates error: " + e.Message);
                    OnErrorOccured("Fetching busy dates error: " + e.Message);
                }
            }
            Timer?.Change(Timeout, Timeout);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Timer?.Dispose();
            Timer = null;
        }

        #endregion

        #region Basic overrides

        public override string ToString()
        {
            return Identifier + " : " + base.ToString();
        }

        #endregion
    }
}
