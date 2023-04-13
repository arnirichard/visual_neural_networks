# visual_neural_networks
Neural network implemented from scratch with visual effect in .Net and Avalonia

Using the MNIST dataset (over 100.000 images of size 28x28px grayscale), users can make side-by-side comparison of simple neural networks consisting of simple dense layers, varying the image pre-processing, learning rate, and cost functions.

The two networks have identical random weights at the start. Minibatch size can be adjusted.

Each epoch is one pass of training with a single minibatch.

Users can see the prediction for every image and include it in the training batch.

"Find failed" button will find the next image in the class that has incorrect classification. By including that image in the training it is possible to train the model faster and eventually classify all images correctly (If images are strecthed).

I can't guarantee that all the calculations are correct, but it is possible to optimize the tensor calculations further.

![Alt text](screenshot.png?raw=true)
