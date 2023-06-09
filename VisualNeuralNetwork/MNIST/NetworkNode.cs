using Avalonia.Threading;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VisualNeuralNetwork.NeuralNetwork;

namespace VisualNeuralNetwork.MNIST
{
    enum ImageProcessing
    {
        None,
        Center,
        Stretch
    }

    class NetworkNode : ViewModelBase
    {
        public const int ImageDimension = 28;
        public const int NumberOfNeurons = 20;
        public const int ImageSize = ImageDimension * ImageDimension;

        public Network Network { get; private set; }
        public List<ImageClass> Classes { get; set; }
        public int TotalImages => Classes.Sum(c => c.NumberOfSamples);
        public ArraySegment<byte>? CurrentImage { get; set; }
        public Classification? CurrentImageClassification { get; set; }
        public double learningRate { get; set; } = 1;
        public double LearningRate
        {
            get => learningRate;
            set
            {
                learningRate = value;
                this.RaisePropertyChanged(nameof(LearningRate));
            }
        }
        public ImageClass? SelectedImageClass => ClassNumber < Classes.Count ? Classes[ClassNumber] : null;
        int classNumber = 0;
        public int ClassNumber
        {
            get => classNumber;
            set
            {
                classNumber = value;
                ImageNumber = 1;
                UpdateImage();
                this.RaisePropertyChanged(nameof(ClassNumber));
                this.RaisePropertyChanged(nameof(SelectedImageClass));
            }
        }
        int imageNumber = 1;
        public int ImageNumber
        {
            get => imageNumber;
            set
            {
                imageNumber = value;
                UpdateImage();
                this.RaisePropertyChanged(nameof(ImageNumber));
            }
        }
        public bool IncludeImageInTraining { get; set; } = true;
        static List<LossFunction> LF = new List<LossFunction> { new MSE(), new SoftmaxLoss(), new CrossEntropyLoss() };
        public List<LossFunction> LossFunctionOptions => LF;
        LossFunction lossFunction = LF[2];
        public LossFunction LossFunction
        {
            get => lossFunction;
            set
            {
                lossFunction = value;
                Network.SetLossFunction(lossFunction);
            }
        }
        ActivationFunction activationFunction = new Sigmoid();
        public ActivationFunction ActivationFunction
        {
            get => activationFunction;
            set
            {
                activationFunction = value;
                SetNewNetwork();
            }
        }
        public IEnumerable<ImageProcessing> ImageProcessingOptions => Enum.GetValues(typeof(ImageProcessing)).Cast<ImageProcessing>();

        ImageProcessing imageProcessing = ImageProcessing.None;
        public ImageProcessing ImageProcessing
        {
            get => imageProcessing;
            set
            {
                imageProcessing = value;
                tensorData = tensors[imageProcessing];
                UpdateImage();
                CalculatePerformance();
            }
        }
        public double PerformancePerc { get; set; }
        public double Epochs { get; set; }
        
        TensorData tensorData;
        Dictionary<ImageProcessing, TensorData> tensors;
        int trainingIndex;
        int trainingClass;
        public NetworkNode(List<ImageClass> classes, Dictionary<ImageProcessing, TensorData> tensors)
        {
            Classes = classes;
            this.tensors = tensors;
            tensorData = tensors[imageProcessing];
            Network = GetNewNetwork();
            UpdateImage();
            CalculatePerformance();
        }

        void UpdateImage()
        {
            CurrentImage = SelectedImageClass?.GetImage(ImageNumber-1, ImageDimension, imageProcessing);
            CurrentImageClassification = GetClassification(CurrentImage);
            this.RaisePropertyChanged(nameof(CurrentImage));
            this.RaisePropertyChanged(nameof(CurrentImageClassification));
        }
        Classification? GetClassification(ArraySegment<byte>? segment)
        {
            if (segment != null)
            {
                double[] output = Network.FeedForward(segment.Value.Select(b => b / 255.0).ToArray());
                int resultingClass = output.MaxIndex();
                return new Classification()
                {
                    PredictedClass = resultingClass,
                    Score = output[resultingClass]
                };
            }
            return null;
        }

        public void SetNewNetwork()
        {
            Network = GetNewNetwork();
            PerformancePerc = 0;
            Epochs = 0;
            this.RaisePropertyChanged(nameof(Network));
            this.RaisePropertyChanged(nameof(PerformancePerc));
            this.RaisePropertyChanged(nameof(Epochs));
            UpdateImage();
            CalculatePerformance();
        }

        Network GetNewNetwork()
        {
            return new Network(lossFunction,
                new List<Layer>()
                {
                    new DenseLayer(ImageSize, NumberOfNeurons, new Sigmoid()),
                    new DenseLayer(NumberOfNeurons, 10, new Sigmoid()),
                });
        }

        internal void Train(int batchSize, bool calcPerformance)
        {
            int[] indexes = GetTrainingIndexes(batchSize);
            Tensor input = tensorData.Input.GetSubTensor(indexes);
            Tensor target = tensorData.Target.GetSubTensor(indexes);
            Network.Train(input, target, learningRate);
            Epochs += batchSize / (double)tensorData.NumberOfTrainingSamples;
            Dispatcher.UIThread.Post(() =>
            {
                this.RaisePropertyChanged(nameof(Epochs));
            });
            if (calcPerformance)
            {
                CalculatePerformance();
                UpdateImage();
            }
        }

        int[] GetTrainingIndexes(int batchSize)
        {
            List<int> result = new();

            if (IncludeImageInTraining)
                result.Add(GetCurrentImageIndex());

            while (result.Count < batchSize)
            {
                var cl = Classes[trainingClass++ % Classes.Count];
                if (trainingClass == Classes.Count)
                {
                    trainingClass = 0;
                    trainingIndex++;
                }

                result.Add(trainingIndex % tensorData.NumberOfTrainingSamples);
                trainingIndex += cl.NumberOfTrainingSamples;
            }

            trainingIndex = trainingIndex % tensorData.NumberOfTrainingSamples;

            return result.ToArray();
        }

        internal void OnWeightsChanged()
        {
            UpdateImage();
        }

        private int GetCurrentImageIndex()
        {
            var imageClass = Classes[ClassNumber];
            int imageIndex = ImageNumber - 1;
            int result = imageIndex >= imageClass.NumberOfTrainingSamples
                ? Classes.Sum(c => c.NumberOfTrainingSamples) + imageIndex - imageClass.NumberOfTrainingSamples
                : Classes.Where(c => c.Class < ClassNumber).Sum(c => c.NumberOfTrainingSamples) + imageIndex;
            return result;
        }

        void CalculatePerformance()
        {
            TensorData td = tensorData;

            ThreadPool.QueueUserWorkItem(delegate
            {
                Tensor input = td.Input.GetSubTensor(
                    Enumerable.Range(td.NumberOfTrainingSamples, td.Input.Shape[0]- td.NumberOfTrainingSamples).ToArray());
                Tensor output = Network.FeedForward(input);

                int[] tested = new int[Classes.Count];
                int[] success = new int[Classes.Count];

                int currentClassIndex = 0;
                ImageClass c = Classes[currentClassIndex];
                int baseIndex = 0;
                int outputIndex = 0;

                for(int i = 0; i < input.Shape[0]; i++)
                {
                    if(i == baseIndex+c.NumberOfTestSamples)
                    {
                        baseIndex = i;
                        currentClassIndex++;
                        c = Classes[currentClassIndex];
                    }

                    int predictedClass = output.Data.MaxIndex(outputIndex, Classes.Count) - outputIndex;

                    tested[currentClassIndex]++;
                    if (predictedClass == c.Class)
                        success[currentClassIndex]++;

                    outputIndex += Classes.Count;
                }
                
                PerformancePerc = 100* success.Sum()/(double)input.Shape[0];
                for(int i = 0; i < tested.Length; i++)
                {
                    Classes[i].SetPerformance(new Performance()
                    {
                        Total = tested[i],
                        CorrectTotal = success[i]
                    });
                }
                Dispatcher.UIThread.Post(() =>
                {
                    this.RaisePropertyChanged(nameof(PerformancePerc));
                });
            });
        }

        internal void FindFailed()
        {          
            var c = SelectedImageClass;
            var startIndex = ImageNumber;
            var nw = Network;

            if (c != null)
                ThreadPool.QueueUserWorkItem(delegate
                {
                    int outputClass;

                    while (startIndex >= 0)
                    {
                        for (int i = startIndex; i < c.NumberOfSamples; i++)
                        {
                            var output = nw.FeedForward(c.GetImage(i, ImageDimension, imageProcessing));
                            outputClass = output.MaxIndex();

                            if (outputClass != c.Class)
                            {
                                if (Network == nw && SelectedImageClass == c)
                                {
                                    ImageNumber = i + 1;
                                }
                                startIndex = -1;
                                break;
                            }
                        }
                        startIndex = startIndex > 0
                            ? 0
                            : -1;
                    }
                });
        }
    }   
}
