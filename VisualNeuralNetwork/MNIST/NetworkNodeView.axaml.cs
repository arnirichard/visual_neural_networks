using Avalonia.Controls;
using Avalonia.Interactivity;

namespace VisualNeuralNetwork.MNIST
{
    public partial class NetworkNodeView : UserControl
    {
        public NetworkNodeView()
        {
            InitializeComponent();
        }

        public void findFailed_Click(object? sender, RoutedEventArgs args)
        {
            if (DataContext is NetworkNode vm)
            {
                vm.FindFailed();
            }
        }
    }
}
