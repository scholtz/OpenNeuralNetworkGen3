using OpenNeuralNetworkGen3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace OpenNeuralNetworkGen3Tests
{
    [TestClass]
    public class AllNeuronTest
    {
        class Test0203 : TestCase
        {
            public static Random random = new Random();
            public override void setRandomInitState(Network network)
            {
                // 0.2 (0 - 0.8)
                // 0.3 (0 - 1.2)

                network.Layers[0].Neurons[0].State = random.NextDouble() / 10 * 4 * 0.70710678118654752440084436210485 * 2;
                network.Layers[0].Neurons[1].State = random.NextDouble() / 10 * 6 * 0.70710678118654752440084436210485 * 2;
            }

            public override ResultType Test(Network network)
            {
                if (network.Layers[0].Neurons[0].State > 0.2 && network.Layers[0].Neurons[1].State > 0.3)
                {
                    if (network.Layers[1].Neurons[0].State > Neuron.VEGETATIVE_STATE)
                    {
                        // correct
                        return ResultType.OkPositive;
                    }
                    else
                    {
                        // false positive
                        return ResultType.FalseNegative;
                    }
                }
                else
                {
                    if (network.Layers[1].Neurons[0].State > Neuron.VEGETATIVE_STATE)
                    {
                        return ResultType.FalsePositive;
                    }
                    else
                    {
                        //correct
                        return ResultType.OkNegative;
                    }
                }
            }
        }
        [TestMethod]
        public void NeuronSupportTest()
        {
            Network network = new Network();

            var inLayer = network.NewLayer();
            inLayer.Add(1, typeof(AllNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(1, typeof(AllNeuron));

            var syn = inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);

            inLayer.Neurons[0].State = 1;
            outLayer.Neurons[0].State = 1;
            Assert.AreEqual(Neuron.VEGETATIVE_STATE, syn.EasinessOfActivation);
            outLayer.Neurons[0].EasinessToLearn = 0.5;
            network.Support(0.1);
            // when we receive positive signal, we do not change EasinessOfActivation
            Assert.AreEqual(Neuron.VEGETATIVE_STATE, syn.EasinessOfActivation);
            // when we receive positive signal, we will reduce easiness to learn
            Assert.IsTrue(outLayer.Neurons[0].EasinessToLearn < 0.5);

            network.ClearStates();

            syn.EasinessOfActivation = Neuron.VEGETATIVE_STATE;
            inLayer.Neurons[0].State = 1;
            outLayer.Neurons[0].State = 0;
            outLayer.Neurons[0].EasinessToLearn = 0.5;
            network.Support(0.1);
            // we do not modify EasinessOfActivation when we receive positive signal
            // however we lower chance for this neuron to learn something new
            Assert.AreEqual(Neuron.VEGETATIVE_STATE, syn.EasinessOfActivation);
            Assert.IsTrue(outLayer.Neurons[0].EasinessToLearn < 0.5);


        }

        [TestMethod]
        public void NeuronNegateTest()
        {
            Network network = new Network();

            var inLayer = network.NewLayer();
            inLayer.Add(1, typeof(AllNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(1, typeof(AllNeuron));

            var syn = inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);

            //inLayer.Neurons[0].State = 1;

            inLayer.Neurons[0].State = 1;
            outLayer.Neurons[0].State = 1;
            Assert.AreEqual(Neuron.VEGETATIVE_STATE, syn.EasinessOfActivation);
            outLayer.Neurons[0].EasinessToLearn = 0.5;
            network.NegateFalsePositive(0.1);
            // when we receive positive signal, we will reduce easiness of activation
            Assert.IsTrue(syn.EasinessOfActivation > Neuron.VEGETATIVE_STATE);
            // when we receive positive signal, we will reduce easiness to learn
            Assert.IsTrue(outLayer.Neurons[0].EasinessToLearn > 0.5);

            network.ClearStates();

            syn.EasinessOfActivation = Neuron.VEGETATIVE_STATE;
            inLayer.Neurons[0].State = 0;
            outLayer.Neurons[0].State = 1;
            outLayer.Neurons[0].EasinessToLearn = 0.5;
            network.NegateFalsePositive(0.1);
            // when from is not active, and To neuron is active, and we do not like this result
            Assert.IsTrue(syn.EasinessOfActivation < Neuron.VEGETATIVE_STATE);
            // however we lower chance for this neuron to learn something new
            Assert.IsTrue(outLayer.Neurons[0].EasinessToLearn > 0.5);


        }
        [TestMethod]
        public void LearnSimpleAllNeuronTest()
        {
            Network network = new Network();

            var inLayer = network.NewLayer();
            inLayer.Add(2, typeof(AllNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(1, typeof(AllNeuron));
            var testcase = new Test0203();

            TestCase.ResultType result;
            // learn network to require Easiness of activation on first synaption 
            // 0.2 and second 0.3

            // EasinessOfActivation should not divergent to 0.2 and 0.3;// EasinessOfActivation should convergate DOWN from 0.4 to 0.2 and 0.3;

            var connection1 = inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            connection1.EasinessOfActivation = 0.2;
            var connection2 = inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            connection2.EasinessOfActivation = 0.3;

            network.Layers[0].Neurons[0].EasinessToLearn = 0.5;
            network.Layers[0].Neurons[1].EasinessToLearn = 0.5;
            network.Layers[1].Neurons[0].EasinessToLearn = 0.5;


            for (int i = 0; i < 1000; i++)
                network.StudyIteration(testcase, 1, 1);

            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.15;
            network.Layers[0].Neurons[1].State = 0.2;

            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkNegative, result);


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.35;
            network.Layers[0].Neurons[1].State = 0.35;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkPositive, result);

            // EasinessOfActivation should convergate UP from 0.1 to 0.2 and 0.3;

            connection1.EasinessOfActivation = 0.1;
            connection2.EasinessOfActivation = 0.1;

            network.Layers[0].Neurons[0].EasinessToLearn = 0.5;
            network.Layers[0].Neurons[1].EasinessToLearn = 0.5;
            network.Layers[1].Neurons[0].EasinessToLearn = 0.5;

            for (int i = 0; i < 1000; i++)
                network.StudyIteration(testcase, 1, 1);

            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.15;
            network.Layers[0].Neurons[1].State = 0.2;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkNegative, result);


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.35;
            network.Layers[0].Neurons[1].State = 0.35;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkPositive, result);

            // EasinessOfActivation should convergate DOWN from 0.4 to 0.2 and 0.3;

            connection1.EasinessOfActivation = 0.4;
            connection2.EasinessOfActivation = 0.4;
            network.Layers[0].Neurons[0].EasinessToLearn = 0.5;
            network.Layers[0].Neurons[1].EasinessToLearn = 0.5;
            network.Layers[1].Neurons[0].EasinessToLearn = 0.5;

            for (int i = 0; i < 1000; i++)
                network.StudyIteration(testcase, 1, 1);

            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.15;
            network.Layers[0].Neurons[1].State = 0.2;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkNegative, result);


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.35;
            network.Layers[0].Neurons[1].State = 0.35;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkPositive, result);


            // EasinessOfActivation should convergate DOWN from 0.4 to 0.2 and 0.3;

            connection1.EasinessOfActivation = 0.4;
            connection2.EasinessOfActivation = 0.4;
            network.Layers[0].Neurons[0].EasinessToLearn = 0.5;
            network.Layers[0].Neurons[1].EasinessToLearn = 0.5;
            network.Layers[1].Neurons[0].EasinessToLearn = 0.5;

            for (int i = 0; i < 100000; i++)
                network.StudyIteration(testcase, 1, 1);

            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.15;
            network.Layers[0].Neurons[1].State = 0.2;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkNegative, result);


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.35;
            network.Layers[0].Neurons[1].State = 0.35;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkPositive, result);


        }

        [TestMethod]
        public void AllNeuronBasicTest()
        {
            Network network = new Network();
            //var inLayer = new Layer();
            var inLayer = network.NewLayer();
            inLayer.Add(2, typeof(AllNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(2, typeof(AllNeuron));

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, false);

            Assert.AreEqual(3, network.Synapses.Count);

            network.ClearStates();

            inLayer.Neurons[0].State = 1;
            inLayer.Neurons[1].State = 0;

            network.Tick(0.1);

            Assert.IsTrue(outLayer.Neurons[0].State <= 0.1);
            Assert.IsTrue(outLayer.Neurons[1].State > 0.1);
            Assert.AreEqual(0.1, outLayer.Neurons[1].FrequencyState);
        }
    }
}
