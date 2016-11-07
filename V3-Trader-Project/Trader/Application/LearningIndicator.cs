﻿using NinjaTrader_Client.Trader.Indicators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V3_Trader_Project.Trader.Visualizers;

namespace V3_Trader_Project.Trader.Application
{
    public enum LearningIndicatorPredictionIndecies{
        BuyCodeProbability = 0, SellCodeProbability = 1, AvgOutcomeMax = 2, AvgOutcomeMin = 3, AvgOutcomeActual = 4
    };

    public class LearningIndicator
    {
        private double[] predictivePower;

        //Not known is base distribution of outcomeCodes and timeframe
        //Plug ML in here?
        
        private double[][] outcomeCodeSamplingTable;
        private double[][] outcomeSamplingTable;
        
        //Not used
        private long timeframe;
        private double targetPercent;
        private double meanBuyDist, meanSellDist;

        private double usedValues;

        private WalkerIndicator indicator;

        public LearningIndicator(WalkerIndicator indicator, double[][] prices, bool[][] outcomeCodes, double[][] outcomes, long timeframe, double meanBuyDist, double meanSellDist, double targetPercent)
        {
            this.meanBuyDist = meanBuyDist;
            this.meanSellDist = meanSellDist;
            this.targetPercent = targetPercent;
            this.timeframe = timeframe;

            double validRatio;
            double[] values = IndicatorRunner.getIndicatorValues(prices, indicator.Clone(), out validRatio);
            if (validRatio < 0.5)
                throw new TooLittleValidDataException("Not enough valid values: " + validRatio);

            //May be does not work properly... todo:
            double min, max, usedValuesRatio;
            //DistributionHelper.getMinMax(values, 4, out min, out max);
            DistributionHelper.getMinMax(values, out min, out max);

            outcomeCodeSamplingTable = IndicatorSampler.sampleValuesOutcomeCode(values, outcomeCodes, min, max, 40, out usedValuesRatio);
            if (usedValuesRatio < 0.5)
                throw new TooLittleValidDataException("Not enough sampling for outcomeCode: " + usedValuesRatio);

            outcomeSamplingTable = IndicatorSampler.sampleValuesOutcome(values, prices, outcomes, min, max, out usedValuesRatio, 40);
            if (usedValuesRatio < 0.5)
                throw new TooLittleValidDataException("Not enough sampling for outcome: " + usedValuesRatio);

            this.usedValues = usedValuesRatio;

            //Predictive power calculation
            predictivePower = new double[24];
            IndicatorSampler.getStatisticsOutcomeCodes(values, outcomeCodes, out predictivePower[0], out predictivePower[1], out predictivePower[2], out predictivePower[3]);
            IndicatorSampler.getStatisticsOutcomes(values, prices, outcomes, out predictivePower[4], out predictivePower[5], out predictivePower[6], out predictivePower[7], out predictivePower[8], out predictivePower[9]);

            DistributionHelper.getSampleOutcomeCodesBuyMaxSellMax(outcomeCodeSamplingTable, 0.5, out predictivePower[10], out predictivePower[11], out predictivePower[12],  out predictivePower[13]);
            DistributionHelper.getSampleOutcomesMinMax(outcomeSamplingTable, 0.5, out predictivePower[14], out predictivePower[15], out predictivePower[16], out predictivePower[17], out predictivePower[18], out predictivePower[19], out predictivePower[20], out predictivePower[21], out predictivePower[22], out predictivePower[23]);
            //End predictive power calculation

            this.indicator = indicator;
        }
        
        public string getName()
        {
            return indicator.getName();
        }

        public long getTimeframe()
        {
            return timeframe;
        }

        public double getUsedValues()
        {
            return usedValues;
        }

        public double getPercent()
        {
            return targetPercent;
        }

        public double[] getPredictivePowerArray()
        {
            return predictivePower;
        }

        public static string getPredictivePowerArrayHeader()
        {
            return "spBuy;spSell;pBuy;pSell;spMin;spMax;spActual;pMin;pMax;pActual;maxBuyCode;maxBuyCodeCount;maxSellCode;maxSellCodeCount;maxMaxGain%;maxMaxGain%Count;minMinFall%;minMinFall%Count;maxMinMaxDistance%;maxMinMaxDistance%Count;maxActualGain%;maxActualGain%Count;minActualFall%;minActualFall%Count;";
        }

        public double getPredictivePowerScore()
        {
            double output = 1;
            foreach (double d in predictivePower)
                if (double.IsNaN(d) == false)
                    output += Math.Abs(d);

            return output;
        }

        public void setNewPrice(double[] prices)
        {
            double mid = (prices[(int)PriceDataIndeces.Ask] + prices[(int)PriceDataIndeces.Bid]) / 2d;
            indicator.setNextData(Convert.ToInt64(prices[(int)PriceDataIndeces.Date]), mid);
        }

        public double[] getPrediction(long timestamp)
        {
            if (indicator.isValid(timestamp) == false)
                return new double[] { double.NaN, double.NaN, double.NaN, double.NaN, double.NaN };
            else
            {
                double buyRatio = double.NaN, sellRatio = double.NaN, max = double.NaN, min = double.NaN, actual = double.NaN;
                
                //Search in outcomeCodeSamplingTable
                double v = indicator.getIndicator();
                for(int i = 0; i < outcomeCodeSamplingTable.Length; i++)
                {
                    if (v >= outcomeCodeSamplingTable[i][(int)SampleValuesOutcomeCodesIndices.Start] 
                        && (i == outcomeCodeSamplingTable.Length - 1 || outcomeCodeSamplingTable[i + 1] == null || outcomeCodeSamplingTable[i + 1][(int)SampleValuesOutcomeCodesIndices.Start] > v))
                    {
                        buyRatio = outcomeCodeSamplingTable[i][(int)SampleValuesOutcomeCodesIndices.BuyRatio];
                        sellRatio = outcomeCodeSamplingTable[i][(int)SampleValuesOutcomeCodesIndices.SellRatio];
                        break;
                    }
                }

                //Search in outcomeSamplingTable
                for (int i = 0; i < outcomeSamplingTable.Length; i++)
                {
                    if (v >= outcomeSamplingTable[i][(int)SampleValuesOutcomeIndices.Start] 
                        && (i == outcomeSamplingTable.Length - 1 || outcomeSamplingTable[i + 1] == null || outcomeSamplingTable[i + 1][(int)SampleValuesOutcomeIndices.Start] > v))
                    {
                        min = outcomeSamplingTable[i][(int)SampleValuesOutcomeIndices.MinAvg];
                        max = outcomeSamplingTable[i][(int)SampleValuesOutcomeIndices.MaxAvg];
                        actual = outcomeSamplingTable[i][(int)SampleValuesOutcomeIndices.ActualAvg];
                        break;
                    }
                }

                return new double[]{ buyRatio, sellRatio, min, max, actual };
            }
        }

        public Image visualizeIndicatorValues(int widht, int height, double[][] prices)
        {
            double validRatio;
            double[] values = IndicatorRunner.getIndicatorValues(prices, indicator.Clone(), out validRatio);
            return ArrayVisualizer.visualizeArray(values, widht, height, 15);
        }

        public Image visualizeTables(int width, int height, bool showState = false)
        {
            double currentIndicatorValue = indicator.getIndicator();
            Image outcomeImg = OutcomeSamplingVisualizer.visualizeOutcomeSamplingTable(outcomeSamplingTable, width, height / 2, showState ? currentIndicatorValue : double.NaN);
            Image outcomeCodeImg = OutcomeSamplingVisualizer.visualizeOutcomeCodeSamplingTable(outcomeCodeSamplingTable, width, height / 2, showState ? currentIndicatorValue : double.NaN);

            Image o = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(o);
            g.Clear(Color.White);
            g.DrawImage(outcomeImg, 0, 0);
            g.DrawImage(outcomeCodeImg, 0, outcomeImg.Height);
            g.DrawLine(new Pen(Color.Blue, 3), 0, outcomeImg.Height, outcomeImg.Width, outcomeImg.Height);

            return o;
        }
    }
}
