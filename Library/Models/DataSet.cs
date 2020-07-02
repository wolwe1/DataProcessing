using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library.Models
{
    public class DataSet
    {
        public List<Record> Records { get; set; }

        public List<string> FeatureNames { get; set; }

        public int NumberOfRecords { get { return this.Records.Count; }}

        public DataSet()
        {
            Records = new List<Record>();
        }



        //Functions

        /// <summary>
        /// Returns all records in the dataset
        /// </summary>
        /// <returns></returns>
        public List<Record> GetRecords()
        {
            return this.Records;
        }

        /// <summary>
        /// Returns a single record at index
        /// </summary>
        /// <param name="index">The index of the record</param>
        /// <returns></returns>
        public Record GetRecord(int index)
        {
            if (index >= this.NumberOfRecords)
                throw new IndexOutOfRangeException();

            return this.Records.ElementAt(index);
        }

        /// <summary>
        /// Checks if the database is in a valid state
        /// </summary>
        /// <returns>True or false</returns>
        public bool IsValid()
        {
            foreach (Record record in Records)
            {
                if (!record.IsValid())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Removes all records at indexes provided
        /// </summary>
        /// <param name="recordIndexes">List of indexes to remove</param>
        internal void RemoveRecords(List<int> recordIndexes)
        {
            //Sort list and remove from the back to prevent index issues
            //recordIndexes.Sort();
            this.Records.RemoveAll(x => recordIndexes.Contains(this.Records.IndexOf(x)));

        }

        public List<List<double>> GetConcatenatedFeatureSets()
        {
            List<List<double>> rawDataSet = new List<List<double>>();

            int numFeaturesInRecord = this.Records.ElementAt(0).Features.Count;

            //For each feature 
            for (int i = 0; i < numFeaturesInRecord; i++)
            {
                //Create a set for every records set of that feature
                List<double> featureSet = new List<double>();

                //For every record
                for (int x = 0; x < this.NumberOfRecords; x++)
                {
                    Record currentRecord = this.Records.ElementAt(x);
                    //Add each timeseries entry of the feature to the featureSet
                    currentRecord.Features.ElementAt(i).ForEach(entry => featureSet.Add(entry));
                }

                rawDataSet.Add(featureSet);
            }

            return rawDataSet;
        }

        /// <summary>
        /// Rebuilds the dataset records from a concatenated feature set 
        /// </summary>
        /// <param name="list">Concatenated feature set</param>
        public void Rebuild(List<List<double>> concatenatedFeatureList)
        {
            //All features should have the same count, 121 items per feature
            int numRecords = concatenatedFeatureList.ElementAt(0).Count/121;
            int numFeatures = this.Records.ElementAt(0).Features.Count;

            List<Record> newRecords = new List<Record>();

            //For each to-be record
            for (int i = 0; i < numRecords; i++)
            {
                Record oldRecord = GetRecord(i);
                Record currentRecord = new Record();
                //Set non feature values for record
                currentRecord.ID = oldRecord.ID;
                currentRecord.Location = oldRecord.Location;
                currentRecord.Target = oldRecord.Target;
                //For each feature
                for (int x = 0; x < numFeatures; x++)
                {
                    List<double> feature = concatenatedFeatureList.ElementAt(x);
                    List<double> featureValuesForCurrentRecord = feature.Skip((i * 121)).Take(121).ToList();
                    currentRecord.SetFeature(x,featureValuesForCurrentRecord);
                }
                newRecords.Add(currentRecord);
            }

            //Clear old records
            this.Records.Clear();
            newRecords.ForEach(record => this.Records.Add(record));
        }

        internal void SetRecord(int index, Record newRecord)
        {
            if (index >= this.NumberOfRecords)
                throw new IndexOutOfRangeException("Trying to set index that doesnt exist");

            this.Records[index] = newRecord;
        }

        public bool Equals(DataSet other)
        {
            for (int i = 0; i < NumberOfRecords; i++)
            {
                Record currentRecord = GetRecord(i);

                if (!currentRecord.Equals(other.GetRecord(i)))
                    return false;
            }

            return true;
        }
    }
}
