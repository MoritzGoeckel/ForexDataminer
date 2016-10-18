﻿using System;
using System.Collections.Generic;
using V3_Trader_Project.Trader;

namespace NinjaTrader_Client.Trader.Indicators
{
    class TimeOpeningHoursIndicator : WalkerIndicator
    {
        public TimeOpeningHoursIndicator()
        {
            
        }

        long currentTime = 0;

        public override double getIndicator()
        {
            DateTime dt = Timestamp.getDate(currentTime);

            if (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                return 0; //Kein trading

            if (dt.DayOfWeek == DayOfWeek.Friday && dt.Hour >= 21)
                return 0; //Kein trading

            if (dt.DayOfWeek == DayOfWeek.Friday && dt.Hour >= 19)
                return 0.5; //Do not open positions

            return 1; //Happy trading :)
            // Trading: 0 = No, 0.5 = Dont open, 1 = Yes
        }

        public override void setNextData(long timestamp, double value)
        {
            currentTime = timestamp;
        }

        public override string getName()
        {
            return "TradingTime";
        }

        public override bool isValid(long timestamp)
        {
            return true;
        }
    }
}
