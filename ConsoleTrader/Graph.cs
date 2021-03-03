using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ConsoleTrader
{
    [Serializable]
    public class Graph
    {
        public int Heigth { get; private set; }         // Size of graph
        public int Length { get; private set; }         // Size of graph
        public Candle Candle { get; private set; }
        public List<string> GraphImage { get; private set; }     // list of strings to display graph
        int candleCount;
        List<double[]> listOfCandlePricePairs;
        List<double> topsAndBottomsOfAllCandles;
        List<int[]> listOfCandleValuePairs;
        List<bool[]> BoolMatrix;
        double upperBound;
        double lowerBound;
        bool boundChanged;


        public Graph()
        {

        }

        public Graph(int heigth, int length, double startPrice)
        {
            listOfCandlePricePairs = new List<double[]>();
            topsAndBottomsOfAllCandles = new List<double>();
            listOfCandleValuePairs = new List<int[]>();
            BoolMatrix = new List<bool[]>();
            GraphImage = new List<string>();
            Heigth = heigth;
            Length = length;

            // Set Bounds 
            upperBound = startPrice * 1.1;           
            lowerBound = startPrice / 1.1;
            boundChanged = false;

            // Initialize
            for (int i = 0; i < Length; i++)
            {
                listOfCandlePricePairs.Add(new double[2]);   //{ 0, 0}
                listOfCandleValuePairs.Add(new int[2]);
                BoolMatrix.Add(new bool[Heigth+1]);
            }
            for (int i = 0; i < length * 2; i++)
            {
                topsAndBottomsOfAllCandles.Add(startPrice);
            }

            // Set first element 
            Candle = new Candle();
            candleCount = 1;
            Candle.ElementsCount = 1;
            listOfCandlePricePairs[0][1] = startPrice;
            listOfCandleValuePairs[0][1] = GetCandleValue(startPrice, lowerBound, upperBound);
        }

        public void BuildGraph(double price)
        {
            boundChanged = ExpandBoundsCheck(price);
            listOfCandleValuePairs[0][0] = GetCandleValue(price, lowerBound, upperBound);
            listOfCandlePricePairs[0][0] = price;
            if (boundChanged)
            {
                RescaleGraph();
            }
            BoolMatrix[0] = MakeBoolArray(0);
            GraphImage = MakeListOfStrings(price);

            if (Candle.ElementsCount == Candle.Capacity)        // then graph moves to the next candle
            {
                ShiftValues();
            }
            Candle.ElementsCount++;
        }


        private int GetCandleValue(double price, double lowerBound, double upperBound)
        {
            int index = (int)Math.Round((price - lowerBound) * Heigth / (upperBound - lowerBound));
            if (index < 0)
            {
                return 0;
            }
            return index;
        }

        private bool ExpandBoundsCheck(double price)
        {
            bool result = false;
            if (price * 1.05 > upperBound)
            {
                upperBound = price * 1.1;
                result = true;
            }
            if (price / 1.05 < lowerBound)
            {
                lowerBound = price / 1.1;
                result = true;
            }
            return result;
        }
          
        private bool NarrowBoundsCheck()
        {
            double max = topsAndBottomsOfAllCandles.Max();
            double min = topsAndBottomsOfAllCandles.Min();
            bool result = false;
            if (upperBound > max * 1.15)
            {
                upperBound = max * 1.1;
                result = true;
            }
            if (lowerBound < min / 1.15)
            {
                lowerBound = min / 1.1;
                result = true;
            }
            return result;
        }

        private void RescaleGraph()
        {
            int candlesToRescale;
            if (candleCount < Length)
            {
                candlesToRescale = candleCount;
            }
            else
            {
                candlesToRescale = listOfCandlePricePairs.Count;
            }
            for (int w = 0; w < candlesToRescale; w++)
            {
                listOfCandleValuePairs[w][0] = GetCandleValue(listOfCandlePricePairs[w][0], lowerBound, upperBound);
                listOfCandleValuePairs[w][1] = GetCandleValue(listOfCandlePricePairs[w][1], lowerBound, upperBound);
            }
            for (int i = 0; i < candlesToRescale; i++)
            {
                BoolMatrix[i] = MakeBoolArray(i);
            }
        }

        private bool[] MakeBoolArray(int index)
        {
            bool[] arr = new bool[Heigth + 1];
            Candle.Bottom = listOfCandleValuePairs[index].Min();
            Candle.Top = listOfCandleValuePairs[index].Max();
            for (int j = Candle.Bottom; j <= Candle.Top; j++)
            {
                arr[j] = true;
            }
            return arr;
        }

        private List<string> MakeListOfStrings(double price)
        {
            List<string> list = new List<string>();
            for (int i = BoolMatrix[0].Length - 1; i >= 0; i--)
            {
                StringBuilder sb = new StringBuilder();
                for (int j = BoolMatrix.Count - 1; j >= 0; j--)
                {
                    if (BoolMatrix[j][i])
                    {
                        sb.Append('\u2022');
                    }
                    else
                    {
                        sb.Append(' ');
                    }
                }
                list.Add(sb.ToString());
            }
            for (int i = 0; i < list.Count; i++)
            {
                list[i] += '|';                      // right bound of the graph
            }
            list[0] += Math.Round(upperBound).ToString();                                  // first string contains upperBound price value
            list[Heigth] += Math.Round(lowerBound).ToString();                             // last string contains lowerBound price value
            list[Heigth - listOfCandleValuePairs[0][0]] += Math.Round(price, 2).ToString();   // a string where current price should be displayed

            // Fill the Console buffer from the last output with spaces
            for (int i = 0; i < list.Count; i++)
            {
                list[i] += new string(' ', 30);
            }

            string str = new string('_', Length);        
            list.Insert(0, str);                         // top bounds of the graph
            list.Add(str + "|  ");                       // bottom bounds of the graph
            return list;
        }

        private void ShiftValues()
        {
            topsAndBottomsOfAllCandles.Insert(0, listOfCandlePricePairs[0][0]);
            topsAndBottomsOfAllCandles.Insert(0, listOfCandlePricePairs[0][1]);
            topsAndBottomsOfAllCandles.RemoveRange(Length * 2, 2);

            boundChanged = NarrowBoundsCheck();

            listOfCandlePricePairs.Insert(0, new double[2] { listOfCandlePricePairs[0][0], listOfCandlePricePairs[0][0] });
            listOfCandlePricePairs.RemoveAt(Length);

            listOfCandleValuePairs.Insert(0, new int[2] { listOfCandleValuePairs[0][0], listOfCandleValuePairs[0][0] });
            listOfCandleValuePairs.RemoveAt(Length);

            BoolMatrix.Insert(0, BoolMatrix[0]);
            BoolMatrix.RemoveAt(Length);

            Candle = new Candle();
            if (candleCount < Length)
            {
                candleCount++;
            }
        }

    }

    public class Candle
    {
        public static int Capacity = 15;          // amount of values each candle gets before graph moves to the next candle
        public int ElementsCount { get; set; }      // amount of values in a candle
        public int Bottom { get; set; }             // bottom of the candle
        public int Top { get; set; }                // top of the candle

    }

}
