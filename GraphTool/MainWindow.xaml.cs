using System;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using Library.Manager;
using Library.Models;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq;
using Library.Utilities;

namespace GraphTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UserControl
    {
        public MainWindow()
        {
            InitializeComponent();

            List<NetworkSummary> summaries = Utils.ReadSummaries();

            //GraphTrainingTime(summaries);
            GraphNetworkResultMetric(summaries, "mse");
        }

        public void GraphTrainingTime(List<NetworkSummary> summaries)
        {
            SeriesCollection = new SeriesCollection();
            List<double> times = new List<double>();
            List<string> networkNames = new List<string>();

            foreach (NetworkSummary summary in summaries)
            {
                networkNames.Add(summary.Name);

                SeriesCollection.Add(
                    new ColumnSeries()
                    {
                        Title = summary.Name,
                        Values = new ChartValues<double> { Math.Round(summary.TimeToTrain,2) }
                    });
            }

            YFormatter = value => Math.Round(value,2) + " seconds";
            Labels = networkNames.ToArray();

            DataContext = this;
        }

        public void GraphNetworkResultMetric(List<NetworkSummary> summaries,string metric)
        {
            SeriesCollection = new SeriesCollection();
            foreach (NetworkSummary summary in summaries)
            {
                SeriesCollection.Add(
                                new LineSeries()
                                {
                                    Title = metric + " of " + summary.Name + " network",
                                    Values = new ChartValues<double>(summary.Evaluations.Where(e => e.Metric == metric).FirstOrDefault().Performance)
                                });
            }

            int maxEpochs = summaries.Max(s => s.Epochs.Max());
            List<int> epochs = Enumerable.Range(0, maxEpochs).ToList();
            Labels = epochs.Select(x => x.ToString()).ToArray();
            YFormatter = value => value.ToString();

            //Set Titles
            XTitle = "Epochs";
            YTitle = metric;
            DataContext = this;
        }
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public string XTitle { get; set; }
        public string YTitle { get; set; }
    }
}
