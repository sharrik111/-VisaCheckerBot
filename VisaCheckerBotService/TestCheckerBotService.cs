using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Checkers;

namespace VisaCheckerBotService
{
    public class TestCheckerBotService : IVisaCheckerBotService
    {
        #region Instance variables & properties

        private long timeout = 5000;

        #endregion

        #region IVisaCheckerBotService implementation

        public List<string> GetBusyDates(string embassy)
        {
            return new List<string> { "date1", "date2", "date3" };
        }

        public List<string> GetFreeDates(string embassy)
        {
            return new List<string> { "free1", "free2", "free3" };
        }

        public DateTime GetLastUpdate(string embassy)
        {
            return DateTime.Now;
        }

        public List<string> GetRegisteredEmbassies()
        {
            return new List<string> { "Embassy1", "Embassy2" };
        }

        public Schedule GetSchedule(string embassy)
        {
            throw new NotImplementedException();
        }

        public int GetSubscribersCount(string embassy)
        {
            throw new NotImplementedException();
        }

        public long GetTimeout(string embassy)
        {
            return timeout;
        }

        public void Load(string embassy)
        {
            return;
        }

        public void LoadAll()
        {
            return;
        }

        public void Save(string embassy)
        {
            return;
        }

        public void SaveAll()
        {
            return;
        }

        public void SetSchedule(string embassy, Schedule value)
        {
            throw new NotImplementedException();
        }

        public void SetTimeout(string embassy, long value)
        {
            timeout = value;
        }

        public bool Subscribe(string embassy, long id)
        {
            return true;
        }

        public bool Unsubscribe(string embassy, long id)
        {
            return true;
        }

        #endregion
    }
}