﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V3_Trader_Project.Trader
{
    public enum PriceDataIndeces : int
    {
        Date = 0, Bid = 1, Ask = 2, Volume = 3
    }

    public class DataLoader
    {
        private List<string> filenames;
        private string rootPath;

        public DataLoader(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            filenames = new List<string>();
            foreach(FileInfo file in dir.GetFiles())
                filenames.Add(file.Name);

            filenames.Sort();
            rootPath = dir.FullName + "/";
        }

        public List<string> getFiles()
        {
            List<string> output = new List<string>();
            output.AddRange(filenames);
            return output;
        }

        public double[][] getArray()
        {
            List<double[]> rows = new List<double[]>();
            foreach(string file in filenames)
            {
                foreach(string line in File.ReadAllLines(rootPath + file))
                {
                    string[] values = line.Split(',');
                    rows.Add(new double[] { Timestamp.getUTCMillisecondsDate(values[(int)PriceDataIndeces.Date]) , double.Parse(values[(int)PriceDataIndeces.Bid].Replace(".", ",")), double.Parse(values[(int)PriceDataIndeces.Ask].Replace(".", ",")), double.Parse(values[(int)PriceDataIndeces.Volume].Replace(".", ",")) });
                }
            }

            return rows.ToArray();
        }
    }
}
