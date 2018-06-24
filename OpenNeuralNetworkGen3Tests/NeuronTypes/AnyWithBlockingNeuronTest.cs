using OpenNeuralNetworkGen3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace OpenNeuralNetworkGen3Tests
{
    [TestClass]
    public class AnyWithBlockingNeuronTest
    {
        class Test0203WithBlock : TestCase
        {
            public static Random random = new Random();
            public override void setRandomInitState(Network network)
            {
                // 0.2 (0 - 0.8)
                // 0.3 (0 - 1.2)

                network.Layers[0].Neurons[0].State = random.NextDouble() / 10 * 4 * 0.70710678118654752440084436210485 * 2;
                network.Layers[0].Neurons[1].State = random.NextDouble() / 10 * 6 * 0.70710678118654752440084436210485 * 2;
                network.Layers[0].Neurons[2].State = random.NextDouble();
            }

            public override ResultType Test(Network network)
            {
                if(network.Layers[1].Neurons[0].State > Neuron.VEGETATIVE_STATE)
                {
                    if(network.Layers[0].Neurons[2].State > 0.2)
                    {
                        return ResultType.FalsePositive;
                    }
                }
                else
                {
                    if(network.Layers[0].Neurons[2].State > network.Synapses[2].EasinessOfActivation)
                    {
                        return ResultType.OkNegative;
                    }
                }

                if (network.Layers[0].Neurons[0].State > 0.2 || network.Layers[0].Neurons[1].State > 0.3)
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
        public void LearnAnyWithBlockingNeuronBasicTest()
        {
            Network network = new Network();

            var inLayer = network.NewLayer();
            inLayer.Add(2, typeof(AnyWithBlockingNeuron));
            inLayer.Add(1, typeof(AnyWithBlockingNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(1, typeof(AnyWithBlockingNeuron));
            var testcase = new Test0203WithBlock();

            TestCase.ResultType result;
            // learn network to require Easiness of activation on first synaption 
            // 0.2 and second 0.3

            // EasinessOfActivation should not divergent to 0.2 and 0.3;// EasinessOfActivation should convergate DOWN from 0.4 to 0.2 and 0.3;

            var connection1 = inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            connection1.EasinessOfActivation = 0.2;
            var connection2 = inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            connection2.EasinessOfActivation = 0.3;
            var connection3 = inLayer.Neurons[2].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);
            connection3.EasinessOfActivation = 0.3;

            network.Layers[0].Neurons[0].EasinessToLearn = 0;
            network.Layers[0].Neurons[1].EasinessToLearn = 0;
            network.Layers[0].Neurons[2].EasinessToLearn = 0;
            network.Layers[1].Neurons[0].EasinessToLearn = 0.5;
            int i = 0;
            while (network.SumOfEasinessToLearn() > 0.00001)
            {
                i++;
                if (i > 100000) break;
                network.StudyIteration(testcase, 1, 1);
            }
            

            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.15;
            network.Layers[0].Neurons[1].State = 0.15;
            network.Layers[0].Neurons[2].State = 0.15;

            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkNegative, result);


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.35;
            network.Layers[0].Neurons[1].State = 0.35;
            network.Layers[0].Neurons[2].State = 0.15;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkPositive, result);


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.35;
            network.Layers[0].Neurons[1].State = 0.35;
            network.Layers[0].Neurons[2].State = 0.60;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkNegative, result);


        }
        /// <summary>
        /// This learning test must learn network to accept 0,2 from first neuron, 0,3 from second neuron, 0,2 from first blocking neuron, and skip 4th blocking neuron
        /// 
        /// it shares test case with LearnAnyWithBlockingNeuronBasicTest , but differs in a way that one blocking synapsis is added to the network that should not be accountable
        /// </summary>
        [TestMethod]
        public void LearnAnyWithBlockingNeuronTwoBlockingTest()
        {
            Network network = new Network();

            var inLayer = network.NewLayer();
            inLayer.Add(2, typeof(AnyNeuron));
            inLayer.Add(2, typeof(AnyWithBlockingNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(1, typeof(AnyWithBlockingNeuron));
            var testcase = new Test0203WithBlock();

            TestCase.ResultType result;
            // learn network to require Easiness of activation on first synaption 
            // 0.2 and second 0.3

            // EasinessOfActivation should not divergent to 0.2 and 0.3;// EasinessOfActivation should convergate DOWN from 0.4 to 0.2 and 0.3;

            var connection1 = inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            connection1.EasinessOfActivation = 0.2;
            var connection2 = inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            connection2.EasinessOfActivation = 0.3;
            var connection3 = inLayer.Neurons[2].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);
            connection3.EasinessOfActivation = 0.3;
            var connection4 = inLayer.Neurons[3].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);
            connection4.EasinessOfActivation = 0.3;

            network.Layers[0].Neurons[0].EasinessToLearn = 0;
            network.Layers[0].Neurons[1].EasinessToLearn = 0;
            network.Layers[0].Neurons[2].EasinessToLearn = 0;
            network.Layers[0].Neurons[3].EasinessToLearn = 0;
            network.Layers[1].Neurons[0].EasinessToLearn = 0.5;
            int i = 0;
            while (network.SumOfEasinessToLearn() > 0.00001)
            {
                i++;
                if (i > 100000) break;
                network.StudyIteration(testcase, 1, 1);
            }


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.15;
            network.Layers[0].Neurons[1].State = 0.15;
            network.Layers[0].Neurons[2].State = 0.15;
            network.Layers[0].Neurons[3].State = 0.15;

            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkNegative, result);


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.35;
            network.Layers[0].Neurons[1].State = 0.35;
            network.Layers[0].Neurons[2].State = 0.15;
            network.Layers[0].Neurons[3].State = 0.95;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkPositive, result);


            network.ClearStates();
            network.Layers[0].Neurons[0].State = 0.35;
            network.Layers[0].Neurons[1].State = 0.35;
            network.Layers[0].Neurons[2].State = 0.60;
            network.Layers[0].Neurons[3].State = 0.90;
            network.Tick(0.1);
            result = testcase.Test(network);
            Assert.AreEqual(TestCase.ResultType.OkNegative, result);


        }
        [TestMethod]
        public void AnyWithBlockingNeuronBasicTest()
        {
            Network network = new Network();
            //var inLayer = new Layer();
            var inLayer = network.NewLayer();
            inLayer.Add(2, typeof(AnyWithBlockingNeuron));
            var outLayer = network.NewLayer();
            outLayer.Add(6, typeof(AnyWithBlockingNeuron));

            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);

            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, false);


            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, false);
            /**/
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

            // test 2 if neg is active and pos is active, result should not be active
            Assert.IsTrue(outLayer.Neurons[1].State <= Neuron.VEGETATIVE_STATE);

            // test 3 if neg is not active and pos is not active, it should not be active
            Assert.IsTrue(outLayer.Neurons[2].State <= Neuron.VEGETATIVE_STATE);

            // test 4 if neg is active and pos is not active, it should not be active
            Assert.IsTrue(outLayer.Neurons[3].State <= Neuron.VEGETATIVE_STATE);

            // test 5 one pos is not active and one pos is active, it should be active
            Assert.IsTrue(outLayer.Neurons[4].State > Neuron.VEGETATIVE_STATE);

            // test 6 if only one incomming is active, it should be active
            Assert.IsTrue(outLayer.Neurons[5].State > Neuron.VEGETATIVE_STATE);

        }
    }
}
