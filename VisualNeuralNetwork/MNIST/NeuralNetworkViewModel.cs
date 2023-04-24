using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VisualNeuralNetwork.NeuralNetwork;

namespace VisualNeuralNetwork.MNIST
{
    class TensorData
    {
        public readonly Tensor Input;
        public readonly Tensor Target;
        public readonly int NumberOfTrainingSamples;
        public TensorData(Tensor input, Tensor target, int numberOfTrainingSamples)
        {
            Input = input;
            Target = target;
            NumberOfTrainingSamples = numberOfTrainingSamples;
        }
    }

    class NeuralNetworkViewModel : ViewModelBase
    {
        public const int ImageDimension = 28;
        public const int ImageSize = ImageDimension * ImageDimension;

        Dictionary<int, ImageClass> Data = new();
        int numberOfTrainingSamples;
        public NetworkNode? Network1 { get; set; }
        public NetworkNode? Network2 { get; set; }
        public bool IsLoading { get; private set; } = true;
        public int BatchSize { get; set; } = 200;
        public int NumberOfBatches { get; set; } = 1000;
        bool isTraining;

        public NeuralNetworkViewModel()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                LoadData();
            });
        }

        internal void Restart()
        {
            Network1?.SetNewNetwork();
            Network2?.SetNewNetwork();
            Network1?.Network.SyncWeights(Network2!.Network);
            Network1?.OnWeightsChanged();
        }

        public void MakeEpoch(int count)
        {
            if (isTraining)
                return;
            isTraining = true;

            ThreadPool.QueueUserWorkItem(delegate
            {
                if (numberOfTrainingSamples > 0)
                    for (int i = 0; i < count; i++)
                    {
                        Network1?.Train(BatchSize, i == count - 1);
                        Network2?.Train(BatchSize, i == count - 1);
                    }
                isTraining = false;
            });
        }

        internal void LoadData()
        {
            int bytesPerSample = ImageSize;
            string folder = @".\Assets\MNIST";

            List<ImageClass> classes = new();
            List<ImageClass> classes2 = new();

            foreach (var file in Directory.GetFiles(folder))
            {
                int c;
                if (int.TryParse(Path.GetFileNameWithoutExtension(file), out c))
                {
                    byte[] data = File.ReadAllBytes(file);
                    Data[c] = new ImageClass(c, bytesPerSample, data);
                    classes2.Add(new ImageClass(c, bytesPerSample, data));
                }
            }

            classes.AddRange(Data.Values.OrderBy(c => c.Class));
            classes2 = classes2.OrderBy(c => c.Class).ToList();
            numberOfTrainingSamples = classes.Sum(c => c.NumberOfSamples);

            Dictionary<ImageProcessing, TensorData> tensorData = new()
            {
                { ImageProcessing.None, CreateInput(classes, ImageProcessing.None) },
                { ImageProcessing.Center, CreateInput(classes, ImageProcessing.Center) },
                { ImageProcessing.Stretch, CreateInput(classes, ImageProcessing.Stretch) },
            };

            Network1 = new NetworkNode(classes, tensorData);
            Network2 = new NetworkNode(classes2, tensorData);
            Network1?.Network.SyncWeights(Network2!.Network);
            Network1?.OnWeightsChanged();
            IsLoading = false;
            this.RaisePropertyChanged("Network1");
            this.RaisePropertyChanged("Network2");
            this.RaisePropertyChanged("IsLoading");
        }

        TensorData CreateInput(List<ImageClass> Classes, ImageProcessing imageProcessing)
        {
            int totalImages = Classes.Sum(c => c.NumberOfSamples);
            double[] inputData = new double[totalImages * ImageSize];
            double[] targetData = new double[totalImages * Classes.Count];
            Tensor input = new Tensor(inputData, new int[] { totalImages, ImageSize });
            Tensor target = new Tensor(targetData, new int[] { totalImages, Classes.Count });
            int inputIndex = 0, targetIndex = 0;

            ArraySegment<double> image;
            foreach (var c in Classes)
            {
                for (int i = 0; i < c.NumberOfTrainingSamples; i++)
                {
                    image = c.GetImage(i, ImageDimension, imageProcessing).Select(b => b/255.0).ToArray();
                    Array.Copy(image.Array!, image.Offset, inputData, inputIndex, image.Count);
                    inputIndex += image.Count;
                    targetData[targetIndex + c.Class] = 1;
                    targetIndex += Classes.Count;
                }
            }
            foreach (var c in Classes)
            {
                for (int i = c.NumberOfTrainingSamples; i < c.NumberOfSamples; i++)
                {
                    image = c.GetImage(i, ImageDimension, imageProcessing).Select(b => b / 255.0).ToArray();
                    Array.Copy(image.Array!, image.Offset, inputData, inputIndex, image.Count);
                    inputIndex += image.Count;
                    targetData[targetIndex + c.Class] = 1;
                    targetIndex += Classes.Count;
                }
            }
            return new TensorData(input, target, Classes.Sum(c => c.NumberOfTrainingSamples));
        }
    }
}