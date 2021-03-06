﻿using Checkers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VisaCheckerBotService;

namespace DesktopManagerApplication.ViewModels.MainWindow
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region Life Cycle

        public MainWindowViewModel(IVisaCheckerBotService service)
        {
            this.service = service ?? throw new ArgumentNullException("service");
            // Initialization process...
            RegisteredEmbassies = service.GetRegisteredEmbassies();
            BusyDatesCommand = new ParameterlessDelegateCommand(UpdateBusyDates);
            SubscribersCountCommand = new ParameterlessDelegateCommand(UpdateSubscribersCount);
            FreeDatesCommand = new ParameterlessDelegateCommand(UpdateFreeDates);
            LastUpdateCommand = new ParameterlessDelegateCommand(UpdateLastUpdateTime);
            SaveCommand = new ParameterlessDelegateCommand(SaveCallback);
            LoadCommand = new ParameterlessDelegateCommand(LoadCallback);
            UpdateCommand = new ParameterlessDelegateCommand(Update);
            if (RegisteredEmbassies.Count != 0)
            {
                SelectedEmbassy = RegisteredEmbassies[0];
            }
        }

        #endregion

        #region Instance variables & properties

        /// <summary>
        /// The model analog.
        /// </summary>
        private IVisaCheckerBotService service;

        public List<string> RegisteredEmbassies { get; private set; }

        private string selectedEmbassy = null;

        public string SelectedEmbassy
        {
            get => selectedEmbassy;
            set
            {
                selectedEmbassy = value;
                OnPropertyChanged();
                // Update timeout and schedule on the view.
                OnPropertyChanged(nameof(Timeout));
                OnPropertyChanged(nameof(FromHours));
                OnPropertyChanged(nameof(ToHours));
                Update();
            }
        }

        private List<string> busyDates = null;

        public List<string> BusyDates
        {
            get => busyDates;
            private set
            {
                busyDates = value;
                OnPropertyChanged();
            }
        }

        public ParameterlessDelegateCommand BusyDatesCommand { get; private set; }

        public ParameterlessDelegateCommand SubscribersCountCommand { get; private set; }

        private List<string> freeDates = null;

        public List<string> FreeDates
        {
            get => freeDates;
            private set
            {
                freeDates = value;
                OnPropertyChanged();
            }
        }

        public int SubscribersCount
        {
            get
            {
                if (SelectedEmbassy == null)
                    return 0;
                return service.GetSubscribersCount(SelectedEmbassy);
            }
        }

        public ParameterlessDelegateCommand FreeDatesCommand { get; private set; }

        private DateTime lastUpdate;

        public DateTime LastUpdate
        {
            get => lastUpdate;
            private set
            {
                lastUpdate = value;
                OnPropertyChanged();
            }
        }

        public ParameterlessDelegateCommand LastUpdateCommand { get; private set; }

        public long Timeout
        {
            get
            {
                if(SelectedEmbassy == null)
                {
                    return 0;
                }
                return service.GetTimeout(SelectedEmbassy);
            }
            set
            {
                try
                {
                    Error = string.Empty;
                    if (SelectedEmbassy == null)
                    {
                        Error = "Embassy is not selected.";
                    }
                    else
                    {
                        service.SetTimeout(SelectedEmbassy, value);
                    }
                }
                catch(Exception ex)
                {
                    Error = ex.Message;
                }
                OnPropertyChanged();
            }
        }

        
        public int FromHours
        {
            get
            {
                if (SelectedEmbassy == null)
                    return 0;
                return service.GetSchedule(SelectedEmbassy).StartHour;
            }
            set
            {
                Error = string.Empty;
                if (SelectedEmbassy == null)
                    Error = "Embassy is not selected.";
                else
                {
                    try
                    {
                        var schedule = new Schedule(value, ToHours);
                        service.SetSchedule(SelectedEmbassy, schedule);
                        OnPropertyChanged();
                    }
                    catch (Exception e)
                    {
                        Error = "Error: " + e.Message;
                    }
                }
            }
        }

        
        public int ToHours
        {
            get
            {
                if (SelectedEmbassy == null)
                    return 0;
                return service.GetSchedule(SelectedEmbassy).FinishHour;
            }
            set
            {
                Error = string.Empty;
                if (SelectedEmbassy == null)
                    Error = "Embassy is not selected.";
                else
                {
                    try
                    {
                        var schedule = new Schedule(FromHours, value);
                        service.SetSchedule(SelectedEmbassy, schedule);
                        OnPropertyChanged();
                    }
                    catch (Exception e)
                    {
                        Error = "Error: " + e.Message;
                    }
                }
            }
        }

        private string error = string.Empty;

        public string Error
        {
            get => error;
            private set
            {
                error = value;
                OnPropertyChanged();
            }
        }

        public ParameterlessDelegateCommand SaveCommand { get; private set; }

        public ParameterlessDelegateCommand LoadCommand { get; private set; }

        public ParameterlessDelegateCommand UpdateCommand { get; private set; }
        
        #endregion

        #region Command callbacks

        private void Update()
        {
            Error = string.Empty;
            UpdateBusyDates();
            UpdateFreeDates();
            UpdateLastUpdateTime();
            UpdateSubscribersCount();
        }

        private void UpdateBusyDates()
        {
            if (SelectedEmbassy != null)
            {
                BusyDates = service.GetBusyDates(SelectedEmbassy);
            }
        }

        private void UpdateFreeDates()
        {
            if(SelectedEmbassy != null)
            {
                FreeDates = service.GetFreeDates(SelectedEmbassy);
            }
        }

        private void UpdateLastUpdateTime()
        {
            if(SelectedEmbassy != null)
            {
                LastUpdate = service.GetLastUpdate(SelectedEmbassy);
            }
        }

        private void UpdateSubscribersCount()
        {
            OnPropertyChanged(nameof(SubscribersCount));
        }

        private void SaveCallback()
        {
            service.SaveAll();
        }

        private void LoadCallback()
        {
            service.LoadAll();
        }

        #endregion

        #region INotifyPropertyChanged

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
