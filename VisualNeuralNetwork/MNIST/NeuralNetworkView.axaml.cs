using Avalonia.Controls;
using Avalonia.Interactivity;

namespace VisualNeuralNetwork.MNIST
{
    public partial class NeuralNetworkView : UserControl
    {
        public NeuralNetworkView()
        {
            InitializeComponent();
            DataContext = new NeuralNetworkViewModel();
        }

        public void NextEpoch_Click(object? sender, RoutedEventArgs args)
        {
            if (DataContext is NeuralNetworkViewModel vm)
            {
                vm.MakeEpoch(1);
            }
        }

        public void MultiEpoch_Click(object? sender, RoutedEventArgs args)
        {
            if(DataContext is NeuralNetworkViewModel vm)
            {
                vm.MakeEpoch(vm.Epochs);
            }
        }

        public void Restart_Click(object? sender, RoutedEventArgs args)
        {
            if (DataContext is NeuralNetworkViewModel vm)
            {
                vm.Restart();
            }
        }
    }
}
