using System;
using System.Linq;


namespace Trader
{
    public class Coin
    {
        public double Price { get; set; }
        public Trend Trend { get; set; }
        
        private double GetPriceChangePercent(Random randomizer)
        {
            int rangeValue = randomizer.Next(1, 101);
            double percent = Trend.ListOfPriceChangeProbabilities.Single((listItem) => rangeValue >= listItem.Begin && rangeValue < listItem.End).Value;
            return percent;
        }
        public double GetNextPrice(Random randomizer)
        {
            double maxPercent = GetPriceChangePercent(randomizer);
            double minPercent = -maxPercent;
            Trend.MakeInfluenceToPriceChange(ref minPercent, ref maxPercent);

            double temp = randomizer.NextDouble();

            double thePercent = temp * (maxPercent - minPercent) + minPercent;
            Price += thePercent*Price;
            return Price;
        }



    }

}
