using System;

namespace Trader
{
    [Serializable]
    public class ValueProbability   // probability to get a value
    {
        public int Begin { get; private set; }  // beginning of the range of probability
        public int End { get; private set; }    // end of the range of probability
        public double Value { get; private set; }
        public ValueProbability(int begin, int end, double value)
        {
            Begin = begin;
            End = end;
            Value = value;
        }
    }
}
