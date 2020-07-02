using Library.Models;
using Library.Manager;
using Library.Utilities;
using Microsoft.VisualBasic.CompilerServices;

namespace DataProcessor
{

    class Program
    {
        static void Main()
        {
            DataSetManager dataSetManager = new DataSetManager();

            dataSetManager.PrepareDataSet();

            DataSet trainingDataset = dataSetManager.GetTrainingSet();

            Library.Utilities.Utils.WriteDataSetToFile(trainingDataset, "Prepared-TrainingSet.csv");

            DataSet testingDataset = dataSetManager.GetTestingSet();

            Library.Utilities.Utils.WriteDataSetToFile(testingDataset, "Prepared-TestingSet.csv");


        }
    }
}
