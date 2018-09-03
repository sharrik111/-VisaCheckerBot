using Checkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace VisaCheckerBotService
{
    [ServiceContract]
    public interface IVisaCheckerBotService
    {
        [OperationContract]
        bool Subscribe(string embassy, long id);

        [OperationContract]
        bool Unsubscribe(string embassy, long id);

        [OperationContract]
        List<string> GetBusyDates(string embassy);

        [OperationContract]
        List<string> GetFreeDates(string embassy);

        [OperationContract]
        List<string> GetRegisteredEmbassies();

        [OperationContract]
        void SaveAll();

        [OperationContract]
        void Save(string embassy);

        [OperationContract]
        void LoadAll();

        [OperationContract]
        void Load(string embassy);

        [OperationContract]
        long GetTimeout(string embassy);

        [OperationContract]
        void SetTimeout(string embassy, long value);

        // It's useless to set Schedule as a data contract because we're already not using WCF service as needed.
        [OperationContract]
        Schedule GetSchedule(string embassy);

        [OperationContract]
        void SetSchedule(string embassy, Schedule value);

        [OperationContract]
        DateTime GetLastUpdate(string embassy);
    }
}
