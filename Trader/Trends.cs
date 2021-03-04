using System.Collections.Generic;


namespace Trader
{
    public abstract class Trend
    {
        public List<ValueProbability> ListOfPriceChangeProbabilities { get;  set; }
        protected string Name { get; set; }
        protected double Influence { get; set; }
        public string ServerLoad { get; set; }
        public abstract void MakeInfluenceToPriceChange(ref double minValue, ref double maxValue);
    }
    public class FlatTrend : Trend
    {
        public FlatTrend()
        {
            Name = TrendName.Flat.ToString();
            Influence = 1.03;
            ServerLoad = ServerLoadState.Normal.ToString();
            ListOfPriceChangeProbabilities = new List<ValueProbability>
            {
                new ValueProbability(1, 4, 0.0015),     // 3% chance to get up to 1.5% change
                new ValueProbability(4, 11, 0.005),     // 7% chance to get up to 0.5% change
                new ValueProbability(11, 36, 0.003),    // 25% chance to get up to 0.3% change
                new ValueProbability(36, 101, 0.0015)   // 65% chance to get up to 0.15% change
            };
        }
        public override void MakeInfluenceToPriceChange(ref double minPercent, ref double maxPercent)
        {
            maxPercent *= Influence;
        }
    }
    public class DescendingFlatTrend : Trend
    {
        public DescendingFlatTrend()
        {
            Name = TrendName.DescendingFlat.ToString();
            Influence = 0.98;
            ServerLoad = ServerLoadState.Normal.ToString();
            ListOfPriceChangeProbabilities = new List<ValueProbability>
            {
                new ValueProbability(1, 4, 0.0015),     // 3% chance to get up to 1.5% change
                new ValueProbability(4, 11, 0.005),     // 7% chance to get up to 0.5% change
                new ValueProbability(11, 36, 0.003),    // 25% chance to get up to 0.3% change
                new ValueProbability(36, 101, 0.0015)   // 65% chance to get up to 0.15% change
            };
        }
        public override void MakeInfluenceToPriceChange(ref double minPercent, ref double maxPercent)
        {
            maxPercent *= Influence;
        }
    }
    public class AscendingFlatTrend : Trend
    {
        public AscendingFlatTrend()
        {
            Name = TrendName.AscendingFlat.ToString();
            Influence = 1.08;
            ServerLoad = ServerLoadState.Normal.ToString();
            ListOfPriceChangeProbabilities = new List<ValueProbability>
            {
                new ValueProbability(1, 4, 0.0015),     // 3% chance to get up to 1.5% change
                new ValueProbability(4, 11, 0.005),     // 7% chance to get up to 0.5% change
                new ValueProbability(11, 36, 0.003),    // 25% chance to get up to 0.3% change
                new ValueProbability(36, 101, 0.0015)   // 65% chance to get up to 0.15% change
            };
        }
        public override void MakeInfluenceToPriceChange(ref double minPercent, ref double maxPercent)
        {
            maxPercent *= Influence;
        }
    }

    public class BullMarketTrend : Trend
    {
        public BullMarketTrend()
        {
            Name = TrendName.BullMarket.ToString();
            Influence = 1.13;
            ServerLoad = ServerLoadState.High.ToString();
            ListOfPriceChangeProbabilities = new List<ValueProbability>
            {
                new ValueProbability(1, 5, 0.015),     // 4% chance to get up to 1.5% change
                new ValueProbability(5, 13, 0.01),     // 8% chance to get up to 1.0% change
                new ValueProbability(13, 30, 0.005),   // 17% chance to get up to 0.5% change
                new ValueProbability(30, 61, 0.003),   // 31% chance to get up to 0.3% change
                new ValueProbability(61, 101, 0.0015)  // 40% chance to get up to 0.15% change
            };
        }
        public override void MakeInfluenceToPriceChange(ref double minPercent, ref double maxPercent)
        {
            if (minPercent < -0.01)
            {
                minPercent = -0.01;
            }
            maxPercent *= Influence;
            minPercent /= Influence;
        }
    }
    public class BearMarketTrend : Trend
    {
        public BearMarketTrend()
        {
            Name = TrendName.BearMarket.ToString();
            Influence = 0.95;
            ServerLoad = ServerLoadState.High.ToString();
            ListOfPriceChangeProbabilities = new List<ValueProbability>
            {
                new ValueProbability(1, 5, 0.015),     // 4% chance to get up to 1.5% change
                new ValueProbability(5, 13, 0.01),     // 8% chance to get up to 1.0% change
                new ValueProbability(13, 30, 0.005),   // 17% chance to get up to 0.5% change
                new ValueProbability(30, 61, 0.003),   // 31% chance to get up to 0.3% change
                new ValueProbability(61, 101, 0.0015)  // 40% chance to get up to 0.15% change
            };
        }
        public override void MakeInfluenceToPriceChange(ref double minPercent, ref double maxPercent)
        {
            if (maxPercent > 0.01)
            {
                maxPercent = 0.01;
            }
            maxPercent *= Influence;
            minPercent /= Influence;

        }
    }
}
