using Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Library.Utilities
{
    public class Utils
    {
        public static List<string> ProcessLineFromCSVFile(string line)
        {
            //Declare all the items we will take from the line
            string id = "";
            string location = "";

            //First Item is the ID of the Sample
            int pos = line.IndexOf(",");
            //Need to -1 to not include comma
            id = line.Remove(pos);
            line = line.Remove(0, pos+1);

            //Get the location
            pos = line.IndexOf(",");
            location = line.Remove(pos);
            line = line.Remove(0, pos + 1);

            //From here the items are seperated using ",
            List<string> features = line.Split("\",").ToList();

            features = features.Select(f => f.Replace("\"", "")).ToList();
            

            //Slot these two features at the front
            features = features.Prepend(location).ToList();
            features = features.Prepend(id).ToList();

            return features;
        }

        /// <summary>
        /// Used to create records from the pre-processed output of "ProcessLineFromCSV()"
        /// </summary>
        /// <param name="useableFeatures"></param>
        /// <returns></returns>
        internal static Record CreateRecordFromCSVFeatures(List<string> useableFeatures)
        {
            //First element is an ID
            string id = useableFeatures.ElementAt(0);

            //Second element is a location
            string location = useableFeatures.ElementAt(1);

            //Last element is the target
            double target = Double.Parse(useableFeatures.Last(), CultureInfo.InvariantCulture);

            //The rest must be processed 
            List<List<double>> features = new List<List<double>>();

            for (int i = 2; i < useableFeatures.Count - 1; i++)
            {
                List<string> featureTimeSeries = useableFeatures.ElementAt(i).Split(",").ToList();

                //Need to replace nans and convert digits to doubles
                List<double> featureTimeSeriesConverted = Utils.ConvertStringListToDoubles(featureTimeSeries);

                features.Add(featureTimeSeriesConverted);
            }

            //Now we can create a Record ,precip,rel_humidity,wind_dir,wind_spd,atmos_press,target
            Record record = new Record()
            {
                ID = id,
                Location = location,
                Target = target,
                TemperatureReadings = features.ElementAt(0),
                PrecipitationReadings = features.ElementAt(1),
                RelativeHumidityReadings = features.ElementAt(2),
                WindDirectionReadings = features.ElementAt(3),
                WindSpeedReadings = features.ElementAt(4),
                AtmosphericPressureReadings = features.ElementAt(5)
            };

            return record;
        }

        private static List<double> ConvertStringListToDoubles(List<string> featureTimeSeries)
        {
            List<double> result = new List<double>();

            foreach (string featureAtTimeStamp in featureTimeSeries)
            {
                if (featureAtTimeStamp == "nan")
                    result.Add(Double.NaN);
                else
                    result.Add(Double.Parse(featureAtTimeStamp, CultureInfo.InvariantCulture));
            }

            return result;
        }

        private const string FILE_PATH = "C:\\Users\\jarro\\Desktop\\University\\Cos711\\Assignments\\A3\\A3 Data\\";
        private const string FILE_LOCATION = FILE_PATH+ "Train.csv";


        /// <summary>
        /// Reads the csv file and creates a dataset
        /// </summary>
        /// <param name="dataSet"></param>
        /// <returns></returns>
        public static void GetDataSetFromCSVFile(ref DataSet dataSet)
        {
            //Read in the raw CSV file
            using var reader = new StreamReader(FILE_LOCATION);

            List<string> FeatureNames = reader.ReadLine().Split(",").ToList();
            dataSet.FeatureNames = FeatureNames;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                List<string> useableFeatures = Utils.ProcessLineFromCSVFile(line);

                //Create record from line
                Record record = Utils.CreateRecordFromCSVFeatures(useableFeatures);

                //Add record to the dataset
                dataSet.Records.Add(record);
            }
        }

        private static string ListTostring(List<double> list)
        {
            string result = "";
            list.ForEach(r => result += r.ToString() + ";");
            //strip trailing comma
            result = result.Remove(result.Length -1,1);

            //Decimals are using , notation. Need to convert to .
            result = result.Replace(",",".");
            result = result.Replace(";",",");
            return result;
        }

        public static void WriteDataSetToFile(DataSet dataset,string fileName)
        {
            using var stream = new StreamWriter(FILE_PATH + fileName);
            //Write header line
            string line = "";
            dataset.FeatureNames.ForEach(name => line += name + ",");
            line = line.Remove(line.Length - 1, 1);

            stream.WriteLine(line);
            stream.Flush();

            string recordFormat = "{0},{1},\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",{8}";
            for (int i = 0; i < dataset.NumberOfRecords; i++)
            {
                Record currentRecord = dataset.GetRecord(i);
                string Id = currentRecord.ID;
                string location = currentRecord.Location;
                string target = currentRecord.Target.ToString();
                target = target.Replace(",", ".");
                string temp = ListTostring(currentRecord.TemperatureReadings);
                string precip = ListTostring(currentRecord.PrecipitationReadings);
                string rel_hum = ListTostring(currentRecord.RelativeHumidityReadings);
                string wind_dir = ListTostring(currentRecord.WindDirectionReadings);
                string wind_spd = ListTostring(currentRecord.WindSpeedReadings);
                string atmos_press = ListTostring(currentRecord.AtmosphericPressureReadings);

                line = string.Format(recordFormat, Id, location, temp, precip, rel_hum, wind_dir, wind_spd, atmos_press, target);
                stream.WriteLine(line);
                stream.Flush();
            }
        }

        public static List<NetworkSummary> ReadSummaries()
        {
            string path = "C:\\Users\\jarro\\Desktop\\University\\Cos711\\Assignments\\A3\\Code\\Main\\";
            string standard = path+"NetworkSummary.json";
            string lstm = path + "LSTM.json";
            string cnn = path + "CNN.json";
            using StreamReader r = new StreamReader(standard);
            string json = r.ReadToEnd();
            List<NetworkSummary> items = JsonConvert.DeserializeObject<List<NetworkSummary>>(json);
            return items;

        }

    }
}
