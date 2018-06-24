using OpenNeuralNetworkGen3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace OpenNeuralNetworkGen3Tests
{
    [TestClass]
    public class AnyNeuronTest
    {
        [TestMethod]
        public void AllNeuronTestBasicTest()
        {
            Network network = new Network();
            //var inLayer = new Layer();
            var inLayer = network.NewLayer();
            inLayer.Add(2, typeof(AnyNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(3, typeof(AnyNeuron));

            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);

            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, false);

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, false);

            network.ClearStates();

            inLayer.Neurons[0].State = 0;
            inLayer.Neurons[1].State = 1;

            network.Tick(0.1);

            Assert.IsTrue(outLayer.Neurons[0].State > Neuron.VEGETATIVE_STATE);
            Assert.AreEqual(0.1, outLayer.Neurons[0].FrequencyState);
            Assert.IsTrue(outLayer.Neurons[1].State > Neuron.VEGETATIVE_STATE);
            Assert.AreEqual(0.1, outLayer.Neurons[1].FrequencyState);
            Assert.IsTrue(outLayer.Neurons[2].State <= Neuron.VEGETATIVE_STATE);


        }
    }
}
