﻿using System;
using System.Collections.Generic;

namespace NinjaTrader_Client.Trader.Indicators
{
    class TestIndicator : WalkerIndicator
    {
        private double lastSeenValue = double.NaN;

        public TestIndicator()
        {

        }

        public override void setNextData(long _timestamp, double _value)
        {
            lastSeenValue = _value;
        }

        public override double getIndicator()
        {
            return lastSeenValue * 2;
        }

        public override string getName()
        {
            return "TestIndiator";
        }

        public override bool isValid(long timestamp)
        {
            return double.IsNaN(lastSeenValue) == false;
        }
    }
}