using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    public struct Schedule
    {
        private int startHour;

        private int finishHour;

        public int StartHour => startHour;

        public int FinishHour => finishHour;

        public Schedule(int startHour, int finishHour)
        {
            if (startHour < 0 || startHour > 24 || finishHour < 0 || finishHour > 24)
                throw new ArgumentException("Incorrect values: should be from 0 to 24.");
            this.startHour = startHour % 24;
            this.finishHour = finishHour % 24;
            if (this.startHour > this.finishHour)
                throw new ArgumentException("Start hour should be less or equal than finish hour.");
        }
    }
}
