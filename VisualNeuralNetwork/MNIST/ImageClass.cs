using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualNeuralNetwork.MNIST
{
    class Performance
    {
        public int Total { get; set; }
        public int CorrectTotal { get; set; }
        public int FailedTotal => Total - CorrectTotal;
        public double SuccessRatePerc => CorrectTotal / (double)Total * 100;

        public override string ToString()
        {
            return string.Format("{0}/{1} {2}%",
                CorrectTotal,
                Total,
                SuccessRatePerc.ToString("0.00"));
        }
    }

    class Classification
    {
        public int PredictedClass { get; set; }
        public double Score { get; set; }

        public override string ToString()
        {
            return string.Format("Predicted {0} Score {1}",
                PredictedClass,
                Score.ToString("0.####"));
        }
    }

    class ImageClass : ViewModelBase
    {
        public const double TrainingRatio = 0.9;

        public int Class { get; set; }
        public int BytesPerSample { get; set; }
        public byte[] Data { get; set; }
        public int NumberOfSamples => Data.Length / BytesPerSample;
        public int NumberOfTrainingSamples => (int)(NumberOfSamples * TrainingRatio);
        public int NumberOfTestSamples => NumberOfSamples-NumberOfTrainingSamples;
        public Performance? Performance { get; private set; }

        public ImageClass(int index, int bytesPerSample, byte[] data)
        {
            Class = index;
            BytesPerSample = bytesPerSample;
            Data = data;
        }

        public void SetPerformance(Performance performance)
        {
            Performance = performance;
            this.RaisePropertyChanged("Performance");
        }

        public ArraySegment<byte> GetImage(int index)
        {
            return new ArraySegment<byte>(Data, index * BytesPerSample, BytesPerSample);
        }
    }
}
