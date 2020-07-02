using Library.Models;
using Library.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Library.Manager
{
    public class DataSetManager
    {
        private DataSet _dataSet;
        private DataSet _trainingDataSet;
        private DataSet _testingDataSet;
        public DataSetManager()
        {
            _dataSet = new DataSet();
            _trainingDataSet = new DataSet();
            _testingDataSet = new DataSet();
        }

        public void LoadDataset()
        {
            //Load the dataset
            Utils.GetDataSetFromCSVFile(ref this._dataSet);
        }

        public void RemoveUnfitRecords()
        {
            //Remove unfit records
            List<int> unfitRecordIndexes = DataPreProcessor.FindUnfitRecords(this._dataSet);
            this._dataSet.RemoveRecords(unfitRecordIndexes);

            //Check for feature based unfitness
            List<int> recordsWithUnfitFeature = DataPreProcessor.FindRecordsWithUnfitFeature(this._dataSet);
            this._dataSet.RemoveRecords(recordsWithUnfitFeature);
            Console.WriteLine("Fully sanatised data set containts " + this._dataSet.NumberOfRecords + " records");
            Console.WriteLine("Percentage of missing values is now " + Statistics.GetPercentageMissing(this._dataSet));
        }

        public void ReplaceMissingValues()
        {
            //Replace Nan values using regression inference
            this._dataSet = DataPreProcessor.ReplaceNan(this._dataSet);
        }

        public void Normalise()
        {
            //Get feature base orientation of the dataset so that features are normalised across all records
            List<List<double>> trainingData = this._trainingDataSet.GetConcatenatedFeatureSets();
            List<List<double>> testingData = this._testingDataSet.GetConcatenatedFeatureSets();

            //Normalise the matrix
            List<List<List<double>>> normalisedSets = DataPreProcessor.NormaliseData(trainingData, testingData);

            this._trainingDataSet.Rebuild(normalisedSets.ElementAt(0));
            this._testingDataSet.Rebuild(normalisedSets.ElementAt(1));
        }

        /// <summary>
        /// Randomly shuffles the dataset
        /// </summary>
        public void Shuffle()
        {
            var rnd = new Random();
            this._dataSet.Records.Shuffle(rnd);
        }
        /// <summary>
        /// Split the dataset into training and testing data sets based on split percentage
        /// </summary>
        /// <param name="splitPercentage">The percentage of elements to go into the training set</param>
        /// <returns>List<DataSet> 0 - Training, 1 - Testing</returns>
        public List<DataSet> SplitDataset(double splitPercentage)
        {
            DataSet trainingDataSet = new DataSet();
            DataSet testingDataSet = new DataSet();

            double realPercentage = (double)splitPercentage / 100;
            int numRecordsInTrainingSet = (int)Math.Round(realPercentage * this._dataSet.NumberOfRecords);

            //Ensure dataset is shuffled
            Shuffle();

            //Place records in training set
            for (int i = 0; i < numRecordsInTrainingSet; i++)
            {
                trainingDataSet.Records.Add(this._dataSet.GetRecord(i));
            }

            //Place all remaining records in the testing set
            for (int i = numRecordsInTrainingSet; i < this._dataSet.NumberOfRecords; i++)
            {
                testingDataSet.Records.Add(this._dataSet.GetRecord(i));
            }

            //Return the new datasets
            return new List<DataSet>()
            {
                trainingDataSet,
                testingDataSet
            };
        }

        public void PrepareDataSet()
        {
            LoadDataset();
            RemoveUnfitRecords();
            ReplaceMissingValues();

            //Create testing and training datasets
            List<DataSet> trainAndTest = SplitDataset(70);
            this._trainingDataSet = trainAndTest.ElementAt(0);
            this._testingDataSet = trainAndTest.ElementAt(1);

            this._trainingDataSet.FeatureNames = this._dataSet.FeatureNames;
            this._testingDataSet.FeatureNames = this._dataSet.FeatureNames;

            //Check validity
            Normalise();

        }

        public DataSet GetDataSet()
        {
            return this._dataSet;
        }

        public DataSet GetTrainingSet()
        {
            return this._trainingDataSet;
        }

        public DataSet GetTestingSet()
        {
            return this._testingDataSet;
        }

        public const string STARLINE = "******************************************************************";

        public void PrintDataSetStatistics()
        {
            Console.WriteLine("DataSet Statistics\n");
            Console.WriteLine(STARLINE + "\n");
            Console.WriteLine("Total Records:" + this._dataSet.NumberOfRecords + "\n");
            Console.WriteLine("Dataset valid state:" + this._dataSet.IsValid());

            //Percentage of missing values
            double percentMissing = Statistics.GetPercentageMissing(this._dataSet);
            Console.WriteLine("Percentage of missing values:" + percentMissing + "%");

            //Number of unfit records
            int numUnfit = DataPreProcessor.FindUnfitRecords(this._dataSet).Count;
            Console.WriteLine("Number of unfit records:" + numUnfit);

        }
    }
}
