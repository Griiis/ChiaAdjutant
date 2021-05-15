using System;
using System.Collections.Generic;
using System.Text;

namespace ChiaAdjutant.Data
{
    public class PlotPassEventData
    {
        private DateTime _dateTime;
        private int _plotsPassed;
        private int _proofAmount;
        private double _passTime;

        public DateTime DateTime { get => _dateTime; }
        public int FilterPassed { get => _plotsPassed;}
        public int ProofAmount { get => _proofAmount;}
        public double PassTime { get => _passTime;}

        public bool IsAnyPass
        {
            get { return _plotsPassed > 0; }
        }

        public bool IsWinPass
        {
            get { return _proofAmount > 0; }
        }

        public PlotPassEventData()
        {
        }

        public PlotPassEventData(DateTime dateTime, int filterPassed, int proofAmount, double timePassTime)
        {
            _dateTime = dateTime;
            _plotsPassed = filterPassed;
            _proofAmount = proofAmount;
            _passTime = timePassTime;
        }

        public override string ToString()
        {
            return string.Format("{0} попытка пройти. {1} признано действительным(-и). {2} доказательств найдено. {3} время затрачено;",DateTime.ToString(),FilterPassed,ProofAmount,PassTime);
        }
    }
}
