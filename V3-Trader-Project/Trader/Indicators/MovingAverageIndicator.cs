﻿using System;
using System.Collections.Generic;

namespace NinjaTrader_Client.Trader.Indicators
{
    class MovingAverageIndicator : WalkerIndicator
    {
        private long timeframe;
        private List<TimestampValuePair> history = new List<TimestampValuePair>();

        public const string Name = "MovingAverageIndicator";
        
        public MovingAverageIndicator(long timeframe)
        {
            this.timeframe = timeframe;
        }

        double valueNow;
        long timestampNow;
        public override void setNextData(long _timestamp, double _value)
        {
            if (_timestamp < timestampNow)
                throw new Exception("Cant add older data here!");

            if (_timestamp == timestampNow && _value != valueNow)
                throw new Exception("Same timestamp different value!");

            if (_timestamp == timestampNow && _value == valueNow)
                return;

            history.Add(new TimestampValuePair { timestamp = _timestamp, value = _value });
            timestampNow = _timestamp;
            valueNow = _value;

            sum += _value;
        }

        double sum = 0;
        public override double getIndicator()
        {
            while (history.Count != 0 && history[0].timestamp < timestampNow - timeframe)
            {
                sum -= history[0].value;
                history.RemoveAt(0);
            }

            double count = history.Count;

            double output = Convert.ToDouble(sum) / Convert.ToDouble(count);
            if (count == 0)
                output = valueNow;

            return output; 
        }

        public override string getName()
        {
            return Name + "_" + timeframe;
        }

        public override bool isValid(long timestamp)
        {
            if (history.Count == 0)
                return false;

            TimestampValuePair pair = history[0];
            if (timestamp - pair.timestamp > timeframe - (timeframe * 0.2)) //Ältester Datensatz ist älter als die (timeframe - 10% Timeframe) 
                return true;
            else
                return false;
        }

        public override WalkerIndicator Clone()
        {
            return new MovingAverageIndicator(timeframe);
        }
    }
}
