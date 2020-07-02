using Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library.Utilities
{
    public class Statistics
    {
        public static double GetPercentageMissing(DataSet dataSet)
        {
            double totalMissingPercentage = 0;

            foreach (Record record in dataSet.GetRecords())
            {
                totalMissingPercentage += GetPercentageMissing(record);
            }

            return Math.Round( (totalMissingPercentage/dataSet.NumberOfRecords), 2 );
        }
        public static double GetPercentageMissing(Record record)
        {
            double totalPossibleValues = 121 * record.Features.Count;
            double totalMissing = 0;

            //Count missing values
            record.Features.ForEach(f => totalMissing += GetMissingValueCount(f));

            //No divide by zero error here
            if (totalMissing == 0)
                return 0;

            double rawMissingPercentage = (totalMissing / totalPossibleValues)*100;
            return Math.Round(rawMissingPercentage, 2);
        }

        public static double GetPercentageMissing(List<double> timeSeries)
        {
            double totalPossibleValues = timeSeries.Count;
            double totalMissing = GetMissingValueCount(timeSeries);

            double rawMissingPercentage = (totalMissing / totalPossibleValues) * 100;
            return Math.Round(rawMissingPercentage, 2);
        }

        public static int GetMissingValueCount(List<double> values)
        {
            int missing = 0;

            foreach (double val in values)
            {
                if (Double.IsNaN(val))
                    missing++;
            }

            return missing;
        }

        public static double GetStandardDeviation(List<double> set)
        {
            double N = set.Count;
            double mean = set.Sum() / N;

            //Sigma (xi - u)^2
            double sum = 0;

            set.ForEach(x => sum += Math.Pow(x - mean, 2));

            return Math.Sqrt((1 / N) * sum);
        }

        /// <summary>
        /// Returns a list of all indexes above Z-Score threshhold
        /// </summary>
        /// <param name="set">The set to find outliers in</param>
        /// <param name="threshHold">The Z-score threshold to compare against</param>
        /// <returns>List of indexes of outliers</returns>
        public static List<int> FindOutliersUsingZScore(List<double> set,double threshHold)
        {
            List<int> indexesOfOutliers = new List<int>();

            double stdDeviation = GetStandardDeviation(set);
            double mean = set.Sum() / (double)set.Count;

            for (int i = 0; i < set.Count; i++)
            {
                double zScore = (set.ElementAt(i) - mean) / stdDeviation;

                if (zScore >= threshHold)
                    indexesOfOutliers.Add(i);
            }

            return indexesOfOutliers;
        }

        internal static List<double> RegressionImputate(List<double> seriesToInfer)
        {
            List<double> inferredSeries = new List<double>();

            double firstKnownValue = seriesToInfer.First();
            double lastKnownValue = seriesToInfer.Last();

            //Get the total difference between the two points
            double totalDifference = lastKnownValue - firstKnownValue;

            //Get the estimate difference between each entry, -1 to account for the known values
            double averageDifference = totalDifference / (seriesToInfer.Count - 1);

            //Loop through each missing value
            double lastValue = firstKnownValue;
            for (int i = 1; i < seriesToInfer.Count -1; i++)
            {
                lastValue += averageDifference;
                inferredSeries.Add(lastValue);
            }

            //Add last known value
            inferredSeries.Add(lastKnownValue);

            return inferredSeries;

        }

        /// <summary>
        /// Returns the mean of a list containing no Nan values
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        internal static double GetMean(List<double> list)
        {
            double sum = list.Sum();
            double mean = sum / (double)list.Count;

            return mean;
        }

        public static double GetZscore(List<double> set,int index)
        {
            double stdDeviation = GetStandardDeviation(set);
            double mean = set.Sum() / (double)set.Count;

            return (set.ElementAt(index) - mean) / stdDeviation;
        }

    }
}
