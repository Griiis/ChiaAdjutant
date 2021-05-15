using ChiaAdjutant.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Globalization;

namespace ChiaAdjutant
{
    public class AsyncDebugReader
    {
        private const string _pathToFile = @"%USERPROFILE%\.chia\mainnet\log\debug.log";
        private Regex _basicRegExp = new Regex(@"harvester chia.harvester.harvester: INFO");

        private List<PlotPassEventData> _plotEvents = new List<PlotPassEventData>();

        public AsyncDebugReader()
        {
            ReadData();
        }

        public async void ReadData()
        {
            await Task.Run(()=> ReadDataBlock());
        }

        public async void ReadDataBlock()
        {
            Stream fileStream = new FileStream(Environment.ExpandEnvironmentVariables(_pathToFile), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader streamReader = new StreamReader(fileStream);

            string lineBuffer = null;
            while(true)
            {
                lineBuffer = streamReader.ReadLine();
                if(lineBuffer != null && _basicRegExp.IsMatch(lineBuffer))
                {
                    GeneratePlotData(lineBuffer);
                }
                else if(lineBuffer == null)
                {
                    await Task.Delay(3000);
                }
            }
        }

        private void GeneratePlotData(string data)
        {
            Regex dateTimeReg = new Regex(@"^(\d+-\d+-\d+T\d+:\d+:\d+.\d+)");
            Regex cleaningReg = new Regex(@"\D+");

            string dateTimeS = dateTimeReg.Match(data).Value;
            dateTimeS = cleaningReg.Replace(dateTimeS, " ");
            string[] splitedData = dateTimeS.Split(' ');
            int[] dateElement = new int[7];
            for(int i = 0;i< dateElement.Length;i++)
            {
                dateElement[i] = int.Parse(splitedData[i]);
            }
            DateTime dateTime = new DateTime(dateElement[0], dateElement[1], dateElement[2], 
                dateElement[3], dateElement[4], dateElement[5], dateElement[6]);

            Regex eligableRef = new Regex(@"(\d+\splots were eligible)");
            int plotsPassed = int.Parse(eligableRef.Match(data).Value.Split(' ')[0]);

            Regex proofRef = new Regex(@"(\d+\sproofs)");
            int proofPassed = int.Parse(proofRef.Match(data).Value.Split(' ')[0]);

            Regex passTimeRef = new Regex(@"(Time: \S+)");
            IFormatProvider formatter = new NumberFormatInfo { NumberDecimalSeparator = "." };
            double passTime = double.Parse(passTimeRef.Match(data).Value.Split(' ')[1], formatter);

            PlotPassEventData passEventData = new PlotPassEventData(dateTime, plotsPassed, proofPassed, passTime);
            _plotEvents.Add(passEventData);
        }

        public PlotPassEventData GetNextPassEvent()
        {
            if(_plotEvents.Count == 0)
            {
                return null;
            }
            PlotPassEventData passEventData = _plotEvents[0];
            _plotEvents.RemoveAt(0);

            return passEventData;
        }
    }
}
