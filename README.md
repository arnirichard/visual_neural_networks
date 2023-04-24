# visual_neural_networks
Neural network implemented from scratch with visual effect in .Net and Avalonia

Using the MNIST dataset (over 100.000 images of size 28x28px grayscale), users can make side-by-side comparison of two simple neural networks consisting of two simple dense layers with 20 hidden neurons, varying the image pre-processing, learning rate, and cost functions.

The two networks have identical random weights at the start. Minibatch size can be adjusted. The weights are shown as images, which show interesting patterns after training. 

Training is performed on 90% of the data, and test is performed on the remaining 10%

![Alt text](screenshot.png?raw=true)
