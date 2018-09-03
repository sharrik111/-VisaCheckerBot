using Checkers;
using Checkers.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading;
using Telegram.Bot;

namespace VisaCheckerBotService
{
    public class VisaCheckerBotService : IVisaCheckerBotService
    {
        #region Life cycle

        public VisaCheckerBotService()
        {
            Bot = new TelegramBotClient(TelegramBotToken);
            Bot.OnMessage += MessageReceived;
            EmbassiesCheckers = new Dictionary<string, VisaChecker>()
            {
                {
                    Checkers.Lithuania.LithuaniaChecker.Identifier, new Checkers.Lithuania.LithuaniaChecker(Callback, 300000) { Schedule = new Schedule(8, 20) }
                }
            };
            foreach (string id in EmbassiesCheckers.Keys)
            {
                EmbassiesCheckers[id].ErrorOccured += ErrorOccured;
            }
            try
            {
                LoadAll();
            }
            catch { }
            Bot.StartReceiving();
        }

        ~VisaCheckerBotService()
        {
            Bot.StopReceiving();
            try
            {
                SaveAll();
            }
            catch { }
            foreach (string id in EmbassiesCheckers.Keys)
            {
                var disposable = EmbassiesCheckers[id] as IDisposable;
                disposable?.Dispose();
            }
        }

        #endregion

        #region Telegram bot logic
        
        private const string TelegramBotToken = "";

        private static readonly string SubscribeMessageDescription = MessageTypes.Subscribe.GetDescription();

        private static readonly string UnsubscribeMessageDescription = MessageTypes.Unsubscribe.GetDescription();

        private static readonly string FreeDatesMessageDescription = MessageTypes.FreeDates.GetDescription();

        private static readonly string BusyDatesMessageDescription = MessageTypes.BusyDates.GetDescription();

        private static readonly string HelpMessageDescription = MessageTypes.Help.GetDescription();

        private static readonly string LastUpdateMessageDescription = MessageTypes.LastUpdate.GetDescription();

        private static readonly string AvailableVisasMessageDescription = MessageTypes.AvailableVisas.GetDescription();

        private const string IncorrectQueryResponse = "Unable to determine your query. Use '/help' to get more information.";

        private static readonly string HelpMessageResponse = string.Format("Commands available:\n" +
            "{0} <embassyIdentifier> - subscribes to free dates notifications to the specified embassy.\n" +
            "Example (subscribe to Lithuania embassy): {0} LT\n\n" +
            "{1} <embassyIdentifier> - unsubscribes from notifications above.\n" +
            "Example (unsubscribe from Lithuania embassy): {1} LT\n\n" +
            "{2} <embassyIdentifier> - checkout free dates when you're able to register to. Could be unavailable for some embassies.\n\n" +
            "{3} <embassyIdentifier> - checkout busy dates when all of the tickets were booked. Could be unavailable for some embassies.\n\n" +
            "{4} <embassyIdentifier> - gets last update time for the specified embassy.\n\n" +
            "{5} - gets identifiers for available embassies to check.", SubscribeMessageDescription, UnsubscribeMessageDescription,
                FreeDatesMessageDescription, BusyDatesMessageDescription, LastUpdateMessageDescription, AvailableVisasMessageDescription);

        private TelegramBotClient Bot { get; set; }

        private async void MessageReceived(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type != Telegram.Bot.Types.Enums.MessageType.TextMessage) return;
            string response;
            long chatID = e.Message.Chat.Id;
            string message = e.Message.Text.ToLower();
            if (message.StartsWith(SubscribeMessageDescription))
            {
                message = message.Replace(SubscribeMessageDescription + " ", "").ToUpper();
                try
                {
                    if (Subscribe(message, chatID))
                    {
                        response = "You were successfully subscribed.";
                    }
                    else
                    {
                        response = "Seems you're subscribed already.";
                    }
                }
                catch
                {
                    response = IncorrectQueryResponse;
                }
            }
            else if (message.StartsWith(UnsubscribeMessageDescription))
            {
                message = message.Replace(UnsubscribeMessageDescription + " ", "").ToUpper();
                try
                {
                    if (Unsubscribe(message, chatID))
                    {
                        response = "You were successfully unsubscribed.";
                    }
                    else
                    {
                        response = "Seems you weren't subscribed before.";
                    }
                }
                catch
                {
                    response = IncorrectQueryResponse;
                }
            }
            else if (message.StartsWith(FreeDatesMessageDescription))
            {
                message = message.Replace(FreeDatesMessageDescription + " ", "").ToUpper();
                try
                {
                    List<string> freeDates = GetFreeDates(message);
                    if (freeDates.Count != 0)
                    {
                        response = string.Format("Free dates for {0} embassy:\n{1}", message, freeDates.BuildString());
                    }
                    else
                    {
                        response = string.Format("There are no free dates for {0} embassy.", message);
                    }
                }
                catch (KeyNotFoundException)
                {
                    response = IncorrectQueryResponse;
                }
                catch (Exception ex)
                {
                    response = "Unable to fetch free dates. Reason: " + ex.Message;
                }
            }
            else if (message.StartsWith(BusyDatesMessageDescription))
            {
                message = message.Replace(BusyDatesMessageDescription + " ", "").ToUpper();
                try
                {
                    List<string> busyDates = GetBusyDates(message);
                    if (busyDates.Count != 0)
                    {
                        response = string.Format("Busy dates for {0} embassy:\n{1}", message, busyDates.BuildString());
                    }
                    else
                    {
                        response = string.Format("Unable to find busy dates for {0} embassy.", message);
                    }
                }
                catch (KeyNotFoundException)
                {
                    response = IncorrectQueryResponse;
                }
                catch (Exception ex)
                {
                    response = "Unable to fetch busy dates. Reason: " + ex.Message;
                }
            }
            else if (message.StartsWith(LastUpdateMessageDescription))
            {
                message = message.Replace(LastUpdateMessageDescription + " ", "").ToUpper();
                try
                {
                    response = GetLastUpdate(message).ToString();
                }
                catch
                {
                    response = IncorrectQueryResponse;
                }
            }
            else if (message == HelpMessageDescription)
            {
                response = HelpMessageResponse;
            }
            else if (message == AvailableVisasMessageDescription)
            {
                List<string> registered = GetRegisteredEmbassies();
                if (registered.Count != 0)
                {
                    response = "Available embassies:\n" + registered.BuildString();
                }
                else
                {
                    response = "There are no available embassies to check.";
                }
            }
            else
            {
                response = IncorrectQueryResponse;
            }
            await Bot.SendTextMessageAsync(chatID, response);
        }

        #endregion

        #region Logic

        private Dictionary<string, VisaChecker> EmbassiesCheckers;

        private List<string> Errors = new List<string>();

        private async void Callback(NotifySubscriberCallbackArgs args)
        {
            try
            {
                await Bot.SendTextMessageAsync(args.ID, string.Format("There are free dates to {0} embassy. Hurry up to book the ticket!", args.CountryIdentifier));
            }
            catch (Exception ex)
            {
                Errors.Add("Unable to send text message. Reason: " + ex.Message);
            }
        }

        private void ErrorOccured(object sender, ErrorEventArgs e)
        {
            Errors.Add(string.Format("{0}. Sender: {1}", e, sender));
            SaveErrors();
        }

        private void SaveErrors()
        {
            try
            {
                new FileStreamStorageService("Errors.txt", true).Save(Errors.BuildString());
            }
            catch { }
        }

        #endregion

        #region IVisaCheckerBotService

        public List<string> GetBusyDates(string embassy)
        {
            return EmbassiesCheckers[embassy].GetBusyDates();
        }

        public List<string> GetFreeDates(string embassy)
        {
            return EmbassiesCheckers[embassy].GetFreeDates();
        }

        public int GetSubscribersCount(string embassy)
        {
            if (!EmbassiesCheckers.ContainsKey(embassy))
                return 0;
            return EmbassiesCheckers[embassy].SubscribersCount;
        }

        public List<string> GetRegisteredEmbassies()
        {
            return EmbassiesCheckers.Keys.ToList();
        }

        public long GetTimeout(string embassy)
        {
            if (EmbassiesCheckers.ContainsKey(embassy))
            {
                return EmbassiesCheckers[embassy].Timeout;
            }
            return -1;
        }

        public void LoadAll()
        {
            foreach (string id in EmbassiesCheckers.Keys)
            {
                EmbassiesCheckers[id].Load(new FileStreamStorageService(id));
            }
        }

        public void Load(string embassy)
        {
            if (EmbassiesCheckers.ContainsKey(embassy))
            {
                EmbassiesCheckers[embassy].Load(new FileStreamStorageService(embassy));
            }
        }

        public void SaveAll()
        {
            foreach (string id in EmbassiesCheckers.Keys)
            {
                EmbassiesCheckers[id].Save(new FileStreamStorageService(id));
            }
        }

        public void Save(string embassy)
        {
            if (EmbassiesCheckers.ContainsKey(embassy))
            {
                EmbassiesCheckers[embassy].Save(new FileStreamStorageService(embassy));
            }
        }

        public void SetTimeout(string embassy, long value)
        {
            if (EmbassiesCheckers.ContainsKey(embassy))
            {
                EmbassiesCheckers[embassy].Timeout = value;
            }
        }

        public Schedule GetSchedule(string embassy)
        {
            if (EmbassiesCheckers.ContainsKey(embassy))
                return EmbassiesCheckers[embassy].Schedule;
            return new Schedule(0, 0);
        }

        public void SetSchedule(string embassy, Schedule value)
        {
            if (EmbassiesCheckers.ContainsKey(embassy))
                EmbassiesCheckers[embassy].Schedule = value;
        }

        public bool Subscribe(string embassy, long id)
        {
            return EmbassiesCheckers[embassy].Subscribe(id);
        }

        public bool Unsubscribe(string embassy, long id)
        {
            return EmbassiesCheckers[embassy].Unsubscribe(id);
        }

        public DateTime GetLastUpdate(string embassy)
        {
            return EmbassiesCheckers[embassy].LastUpdate;
        }

        #endregion
    }
}
