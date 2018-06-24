using OpenNeuralNetworkGen3;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace OpenNeuralNetworkGen3Tests
{
    [TestClass]
    public class AllWithBlockingNeuronTest
    {
        [TestMethod]
        public void AllWithBlockingNeuronBasicTest()
        {
            Network network = new Network();
            //var inLayer = new Layer();
            var inLayer = network.NewLayer();
            inLayer.Add(2, typeof(AllWithBlockingNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(6, typeof(AllWithBlockingNeuron));

            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);

            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, false);


            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, false);

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[3], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[3], Neuron.VEGETATIVE_STATE, true);

            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[4], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[4], Neuron.VEGETATIVE_STATE, false);

            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[5], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[5], Neuron.VEGETATIVE_STATE, false);
            /**/
            //Assert.AreEqual(12, network.Synapses.Count);

            network.ClearStates();

            inLayer.Neurons[0].State = 0;
            inLayer.Neurons[1].State = 1;

            network.Tick(0.1);

            // test 1 if neg is not active and pos is active, it should be active
            Assert.IsTrue(outLayer.Neurons[0].State > Neuron.VEGETATIVE_STATE);
            Assert.AreEqual(0.1, outLayer.Neurons[0].FrequencyState);

            // test 2 if neg is active and pos is active, result should not be active
            Assert.IsTrue(outLayer.Neurons[1].State <= Neuron.VEGETATIVE_STATE);

            // test 3 if neg is not active and pos is not active, it should not be active
            Assert.IsTrue(outLayer.Neurons[2].State <= Neuron.VEGETATIVE_STATE);

            // test 4 if neg is active and pos is not active, it should not be active
            Assert.IsTrue(outLayer.Neurons[3].State <= Neuron.VEGETATIVE_STATE);

            // test 5 one pos is not active and one pos is active, it should be active
            Assert.IsTrue(outLayer.Neurons[4].State > Neuron.VEGETATIVE_STATE);
            Assert.AreEqual(0.1, outLayer.Neurons[4].FrequencyState);

            // test 6 if only one incomming is active, it should be active
            Assert.IsTrue(outLayer.Neurons[5].State <= Neuron.VEGETATIVE_STATE);

        }
    }
}
