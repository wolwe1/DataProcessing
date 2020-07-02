using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Models
{
    public class NetworkSummary
    {
        public string Name { get; set; }
        public List<int> Epochs { get; set; }
        public double TimeToTrain { get; set; }
        public List<Evaluations> Evaluations { get; set; }

    }

    public class Evaluations
    {
        public string Metric { get; set; }
        public List<double> Performance { get; set; }

    }
}
