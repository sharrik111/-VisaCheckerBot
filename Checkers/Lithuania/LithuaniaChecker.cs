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
            modifiedTimeout = timeout;
        }

        #endregion

        #region Constants

        public const string Identifier = "LT";

        private const string RequestBaseAddress = "https://evas2.urm.lt/calendar/json?";

        private const string RequestFreeDates = RequestBaseAddress + "_d=&_aby=3&_cry=6&_c=1&_t=";

        private const string RequestBusyDates = RequestBaseAddress + "_d=&_aby=3&_cry=6&_c=1&_b=1";

        private const int BlockMultiplier = 2;

        #endregion

        #region Properties

        private long modifiedTimeout;

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

        private void Request(object state)
        {
            bool blocked = false;
            // Add 'From' < 'To' and check the DateTime.Now with the gate. If outside of the gate - return.
            if (Schedule.StartHour == Schedule.FinishHour || (DateTime.Now.Hour >= Schedule.StartHour && DateTime.Now.Hour < Schedule.FinishHour))
            {
                using (var client = new WebClient())
                {
                    // Using proxy is a great idea but the list of Belarusian proxies are not so wide.
                    // Scanning free proxies lists on the Internet will make the code a bit harder so it
                    // is an idea for the future.
                    //client.Proxy = new WebProxy("86.57.135.124", 41258)
                    //{
                    //    BypassProxyOnLocal = false
                    //};
                    // Currently the next scheme is used (based on user experience):
                    // 1) In case of blocking - Increase timeout with multiplier.
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
                        blocked = true;
                        OnErrorOccured("Fetching free dates error: " + e.Message);
                    }
                    if (!blocked)
                    {
                        // If the bot is blocked there is no sense to make an additional request.
                        try
                        {
                            var response = client.DownloadString(RequestBusyDates);
                            var busyDatesResponse = JsonConvert.DeserializeObject<List<string>>(response);
                            busyDatesResponse.RemoveEmptyItems();
                            BusyDates = busyDatesResponse;
                        }
                        catch (Exception e)
                        {
                            OnErrorOccured("Fetching busy dates error: " + e.Message);
                        }
                    }
                }
            }
            modifiedTimeout = blocked ? modifiedTimeout  * BlockMultiplier : Timeout;
            Timer?.Change(modifiedTimeout, modifiedTimeout);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                Timer?.Dispose();
                Timer = null;
            }
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
