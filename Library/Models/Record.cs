using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Models
{
    /// <summary>
    /// A Record symbolises a data entrypublic  complete with input values and an expected output
    /// </summary>
    public class Record
    {
        //Identifiers
        public string ID { get; set; }

        public string Location { get; set; }

        //Data Fields
        public List<double> TemperatureReadings { get; set; }
        public List<double> PrecipitationReadings { get; set; }
        public List<double> RelativeHumidityReadings { get; set; }
        public List<double> WindDirectionReadings { get; set; }
        public List<double> WindSpeedReadings { get; set; }
        public List<double> AtmosphericPressureReadings { get; set; }

        //Combine Features into a single list
        public List<List<double>> Features
        {
            get => new List<List<double>>()
                {
                    this.TemperatureReadings,
                    this.PrecipitationReadings,
                    this.RelativeHumidityReadings,
                    this.WindDirectionReadings,
                    this.WindSpeedReadings,
                    this.AtmosphericPressureReadings
                };
        }

        public void SetFeature(int index,List<double> newValue)
        {
            switch (index)
            {
                case 0:
                    this.TemperatureReadings = newValue;
                    break;

                case 1:
                    this.PrecipitationReadings = newValue;
                    break;
                case 2:
                    this.RelativeHumidityReadings = newValue;
                    break;
                case 3:
                    this.WindDirectionReadings = newValue;
                    break;
                case 4:
                    this.WindSpeedReadings = newValue;
                    break;
                case 5:
                    this.AtmosphericPressureReadings = newValue;
                    break;

                default:
                    break;
            }
            

        }

        //Expected Output
        public double Target { get; set; }  

        //Functions
        public bool IsValid()
        {
            foreach (List<double> feature in this.Features)
            {
                if (feature.Count != 121)
                    return false;
            }

            return true;
        }

        public bool Equals(Record other)
        {
            for (int i = 0; i < Features.Count; i++)
            {
                List<double> otherRecordsFeatureSet = other.Features[i];
                List<double> thisRecordsFeatureSet = Features[i];
                for (int x = 0; x < 121; x++)
                {
                    if (thisRecordsFeatureSet[x] != otherRecordsFeatureSet[x])
                        return false;
                }
            }

            return true;
        }

    }
}
