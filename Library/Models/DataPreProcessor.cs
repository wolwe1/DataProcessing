using Library.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Library.Models
{
    class DataPreProcessor
    {
        private const double UNFIT_PERCENTAGE_MISSING = 25;
        public static List<int> FindUnfitRecords(DataSet dataSet)
        {
            List<int> unfitIndexes = new List<int>();

            for (int i = 0; i < dataSet.NumberOfRecords; i++)
            {
                if (Statistics.GetPercentageMissing(dataSet.GetRecord(i)) >= UNFIT_PERCENTAGE_MISSING)
                {
                    //Record is unfit, add its index to the list
                    unfitIndexes.Add(i);
                }   
            }
            return unfitIndexes;
        }

        public static List<int> FindRecordsWithUnfitFeature(DataSet dataSet)
        {
            List<int> unfitIndexes = new List<int>();


            for (int i = 0; i < dataSet.NumberOfRecords; i++)
            {
                Record record = dataSet.GetRecord(i);

                foreach (List<double> featureSet in record.Features)
                {
                    if (Statistics.GetPercentageMissing(featureSet) >= UNFIT_PERCENTAGE_MISSING)
                    {
                        unfitIndexes.Add(i);
                        break;
                    }
                }
            }
            return unfitIndexes;
        }



        //internal static List<List<double>> RemoveOutliers(List<List<double>> dataSet)
        //{
        //    List<List<double>> normalisedDataSet = new List<List<double>>();
        //    //For each feature
        //    for (int i = 0; i < dataSet.Count; i++)
        //    {
        //        List<double> featureData = dataSet.ElementAt(i);

        //        List<double> normalisedFeatureData = new List<double>();

        //        //Get the mean
        //        double sum = featureData.Sum();
        //        double mean = sum / (double)featureData.Count;

        //        //Get the standard deviation
        //        double stdDeviation = Statistics.GetStandardDeviation(featureData);

        //        //Normalise each value and place it in the normalised list
        //        featureData.ForEach(entry => normalisedFeatureData.Add(((entry - mean) / stdDeviation)));

        //        //Add normalised feature set to normalised data set
        //        normalisedDataSet.Add(normalisedFeatureData);

        //    }

        //    return normalisedDataSet;
        //}

        /// <summary>
        /// Returns a list containing the normalised training and testing sets, in concatenated feature form
        /// </summary>
        /// <param name="trainingData">Concatenated feature form</param>
        /// <param name="testingData">Concatenated feature form</param>
        /// <returns></returns>
        internal static List<List<List<double>>> NormaliseData(List<List<double>> trainingData, List<List<double>> testingData)
        {
            //Prepare values
            List<List<double>> normalisedTrainingDataSet = new List<List<double>>();
            List<List<double>> normalisedTestingDataSet = new List<List<double>>();

            int numFeatures = trainingData.Count;//Both should be equal

            //For each feature
            for (int i = 0; i < numFeatures; i++)
            {
                //Get testing and training feature set
                List<double> trainingFeatureData = trainingData.ElementAt(i);
                List<double> testingFeatureData = testingData.ElementAt(i);

                //Get training mean and standard deviation
                double trainingMean = Statistics.GetMean(trainingFeatureData);
                double trainingStdDeviation = Statistics.GetStandardDeviation(trainingFeatureData);

                //Normalise each value and place it in the normalised list

                normalisedTrainingDataSet.Add( trainingFeatureData.Select(entry => ((entry - trainingMean) / trainingStdDeviation) ).ToList() );
                normalisedTestingDataSet.Add( testingFeatureData.Select(entry => ((entry - trainingMean) / trainingStdDeviation) ).ToList() );

            }

            return new List<List<List<double>>>()
            {
                normalisedTrainingDataSet,
                normalisedTestingDataSet
            };
        }

        internal static DataSet ReplaceNan(DataSet dataSet)
        {
            //For each record in the dataset
            for (int i = 0; i < dataSet.NumberOfRecords; i++)
            {
                Record currentRecord = dataSet.GetRecord(i);
                //Pass off to record level replace
                dataSet.SetRecord(i, ReplaceNan(currentRecord));
            }
            return dataSet;
        }

        internal static Record ReplaceNan(Record record)
        {
            //For each feature set
            for (int i = 0; i < record.Features.Count; i++)
            {
                List<double> featureSet = record.Features.ElementAt(i);

                List<double> noNanFeatureSet = ReplaceNan(featureSet);

                //Set the records feature set to the new no Nan version
                record.Features[i].Clear();
                record.Features[i].AddRange(noNanFeatureSet);
            }

            return record;
        }

        internal static List<double> ReplaceNan(List<double> featureSet)
        {
            //!TODO NEED TO HANDLE CASE WHERE NAN IS FIRST ELEMENT OR LAST ELEMENT
            List<double> noNanFeatureSet = new List<double>();

            //Loop through the feature set
            for (int i = 0; i < featureSet.Count; i++)
            {
                double entry = featureSet.ElementAt(i);

                //Check if entry is Nan
                if (Double.IsNaN(entry))
                {
                    //If nan, keep searching until finding the next non Nan value
                    int x = i + 1;
        
                    while(x < featureSet.Count && Double.IsNaN(featureSet.ElementAt(x)) )
                    {
                        x++;
                    }
                    //Non Nan entry found, start infering entry values by taking known value i,Nan...,x
                    List<double> seriesToInfer;
                    List<double> inferredSeries;

                    //Special cases
                    if (i == 0 || x == featureSet.Count)//Missing start value or end value
                    {
                        seriesToInfer = featureSet.GetRange(i, (x - i));
                        seriesToInfer.ForEach(x => noNanFeatureSet.Add(0)); //Add 0 for every missing value
                        //To prevent skipping 
                        x -= 1;

                    }
                    else
                    {
                        //-1 to get last known value, +2 because last index not included and then another for next known value
                        seriesToInfer = featureSet.GetRange(i-1, (x+2 - i));
                        inferredSeries = Statistics.RegressionImputate(seriesToInfer);

                        //Entries i to x now have inferred values, add the values to the new list
                        inferredSeries.ForEach(x => noNanFeatureSet.Add(x));
                    }
                    
                    //jump i to the next value
                    i = x;
                }
                else
                {
                    noNanFeatureSet.Add(entry);
                }
            }

            return noNanFeatureSet;
        }
    }
}
