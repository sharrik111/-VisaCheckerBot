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

        [OperationContract]
        DateTime GetLastUpdate(string embassy);
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
