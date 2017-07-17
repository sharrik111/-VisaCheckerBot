using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Reflection;

namespace VisaCheckerBotService
{
    enum MessageTypes
    {
        [Description("/help")]
        Help = 0,
        [Description("/subscribe")]
        Subscribe,
        [Description("/unsubscribe")]
        Unsubscribe,
        [Description("/busydates")]
        BusyDates,
        [Description("/freedates")]
        FreeDates,
        [Description("/availablevisas")]
        AvailableVisas,
        [Description("/lastupdate")]
        LastUpdate
    }

    static class EnumExtensions
    {
        public static string GetDescription(this Enum en)
        {
            Type type = en.GetType();

            MemberInfo[] info = type.GetMember(en.ToString());
            if(info != null && info.Length != 0)
            {
                object[] attrs = info[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length != 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }
    }
}