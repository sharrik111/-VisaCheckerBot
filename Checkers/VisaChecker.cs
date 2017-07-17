using Checkers.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    public abstract class VisaChecker
    {
        #region Ctors

        public VisaChecker(NotifySubscriberCallback callback, long timeout = 300000)
        {
            Callback = callback;
            Timeout = timeout;
        }

        #endregion

        #region Properties

        protected long timeout;

        public virtual long Timeout
        {
            get => timeout;
            set
            {
                if(value < 5000)
                {
                    throw new ArgumentException("Too low value for timeout.", "Timeout");
                }
                timeout = value;
            }
        }

        public DateTime LastUpdate { get; protected set; }

        protected List<string> BusyDates { get; set; } = new List<string>();

        protected List<string> FreeDates { get; set; } = new List<string>();

        protected NotifySubscriberCallback Callback { get; set; }

        protected HashSet<long> subscribers = new HashSet<long>();

        public event ErrorEventHandler ErrorOccured;

        #endregion

        #region Logic

        protected virtual void OnErrorOccured(string error)
        {
            var handler = ErrorOccured;
            handler?.Invoke(this, new ErrorEventArgs(error));
        }

        protected virtual void OnFreeDatesNotify(string identifier)
        {
            if (FreeDates.Count != 0 && subscribers.Count != 0)
            {
                // We need to create the copy of subscribers collection because of possible desynchronizing.
                subscribers.ToList().ForEach(id => Callback?.Invoke(new NotifySubscriberCallbackArgs(id, identifier)));
            }
        }

        public abstract bool Subscribe(long id);

        public abstract bool Unsubscribe(long id);

        public abstract void Save(IStorageService service);

        public abstract void Load(IStorageService service);

        public virtual List<string> GetBusyDates()
        {
            return BusyDates.ToList();
        }

        public virtual List<string> GetFreeDates()
        {
            return FreeDates.ToList();
        }

        #endregion

        #region Basic overrides

        public override string ToString()
        {
            return string.Format("Timeout: {0}. Subscribers: {1}. ", Timeout, subscribers.Count);
        }

        #endregion
    }
}
