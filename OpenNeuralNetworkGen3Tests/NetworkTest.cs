using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNeuralNetworkGen3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace OpenNeuralNetworkGen3Tests
{
    [TestClass]
    public class NetworkTest
    {
        public class ComplexLearnCase : TestCase
        {
            private static Random random = new Random();
            public override void setRandomInitState(Network network)
            {
                foreach(var neuron in network.Layers[0].Neurons)
                {
                    neuron.State = 0;
                }
                for(int i = 0; i < random.Next(0, network.Layers[0].Neurons.Count ); i++)
                {
                    network.Layers[0].Neurons[i].State = 1;
                }
            }

            public override ResultType Test(Network network)
            {

                int i = -1;
                while (i < network.Layers[0].Neurons.Count - 1 && network.Layers[0].Neurons[i+1].State == 1)
                {
                    i++;
                }

                if(i == -1)
                {
                    if (network.Layers[2].Neurons.Count(n => n.State > Neuron.VEGETATIVE_STATE) > 0)
                    {
                        return ResultType.FalsePositive;
                    }
                    else
                    {
                        return ResultType.OkNegative;
                    }
                }

                if (network.Layers[2].Neurons.Count(n=>n.State > Neuron.VEGETATIVE_STATE) > 1)
                {
                    if(network.Layers[2].Neurons[i].State > Neuron.VEGETATIVE_STATE)
                    {
                        return ResultType.FalsePositive;
                    }
                    else
                    {
                        return ResultType.FalseNegative;

                    }
                }

                if (network.Layers[2].Neurons[i].State > Neuron.VEGETATIVE_STATE)
                {
                    return ResultType.OkPositive;
                }

                return ResultType.FalseNegative;

            }
        }

        [TestMethod]
        public void ComplexLearnTest()
        {
            var network = new Network();
            var inLayer = network.NewLayer("in")
                            .Add(3, typeof(AnyNeuron));

            var midLayer1 = network.NewLayer("mid")
                                .Add(3, typeof(AllWithBlockingNeuron))
                                .Add(3, typeof(AnyWithBlockingNeuron));

            var outLayer = network.NewLayer("out")
             .Add(3, typeof(AnyWithBlockingNeuron))
             ;

            var testcase = new ComplexLearnCase();


            inLayer.ConnetToLayer(midLayer1, network, null, false);
            inLayer.ConnetToLayer(midLayer1, network, null, true);

            midLayer1.ConnetToLayer(outLayer, network, null, false);
            midLayer1.ConnetToLayer(outLayer, network, null, true);

            int i = 0;
            while (network.SumOfEasinessToLearn() > 0.00001)
            {
                i++;
                if (i > 100000) break;
                network.StudyIteration(testcase, 2, 1);
            }

            for (int ii = 0; ii < 100; ii++)
            {
                network.ClearStates();
                testcase.setRandomInitState(network);
                network.Tick(0.1);
                network.Tick(0.1);
                var result = testcase.Test(network);
                Assert.IsTrue(result == TestCase.ResultType.OkNegative || result == TestCase.ResultType.OkPositive, "Test "+ii);
            }

        }
        [TestMethod]
        public void ComplexLearnWithPredefinedNetworkTest()
        {
            var testcase = new ComplexLearnCase();
            var network = new Network();

            var inLayer = network.NewLayer("in")
                            .Add(5, typeof(AnyNeuron));

            var midLayer1 = network.NewLayer("mid")
                                .Add(3, typeof(AllWithBlockingNeuron));

            var outLayer = network.NewLayer("out")
             .Add(4, typeof(AnyWithBlockingNeuron))
             .Add(1, typeof(AllWithBlockingNeuron));
            var random = new Random();
            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[0], random.NextDouble() , false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[0], random.NextDouble(), false);
            inLayer.Neurons[2].ConnectTo(midLayer1.Neurons[0], random.NextDouble(), false);
            inLayer.Neurons[3].ConnectTo(midLayer1.Neurons[0], random.NextDouble(), false);

            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[1], random.NextDouble(), false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[1], random.NextDouble(), false);
            inLayer.Neurons[2].ConnectTo(midLayer1.Neurons[1], random.NextDouble(), false);

            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[2], random.NextDouble(), false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[2], random.NextDouble(), false);

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], random.NextDouble(), false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], random.NextDouble(), true);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[0], random.NextDouble(), true);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[0], random.NextDouble(), true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[0], random.NextDouble(), true);


            midLayer1.Neurons[2].ConnectTo(outLayer.Neurons[1], random.NextDouble(), false);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[1], random.NextDouble(), true);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[1], random.NextDouble(), true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[1], random.NextDouble(), true);

            midLayer1.Neurons[1].ConnectTo(outLayer.Neurons[2], random.NextDouble(), false);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[2], random.NextDouble(), true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[2], random.NextDouble(), true);

            midLayer1.Neurons[0].ConnectTo(outLayer.Neurons[3], random.NextDouble(), false);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[3], random.NextDouble(), true);

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[4], random.NextDouble(), false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[4], random.NextDouble(), false);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[4], random.NextDouble(), false);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[4], random.NextDouble(), false);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[4], random.NextDouble(), false);

            int i = 0;
            while (network.SumOfEasinessToLearn() > 0.00001)
            {
                i++;
                if (i > 100000) break;
                network.StudyIteration(testcase, 2, 1);
            }

            for (int ii = 0; ii < 100; ii++)
            {
                network.ClearStates();
                testcase.setRandomInitState(network);
                network.Tick(0.1);
                network.Tick(0.1);
                var result = testcase.Test(network);
                Assert.IsTrue(result == TestCase.ResultType.OkNegative || result == TestCase.ResultType.OkPositive, "Test " + ii);
            }

        }

        [TestMethod]
        public void ComplexTest()
        {
            var testCase = new ComplexLearnCase();
            var network = new Network();
            var inLayer = network.NewLayer("in")
                            .Add(5, typeof(AnyNeuron));

            var midLayer1 = network.NewLayer("mid")
                                .Add(3, typeof(AllWithBlockingNeuron));

            var outLayer = network.NewLayer("out")
             .Add(4, typeof(AnyWithBlockingNeuron))
             .Add(1, typeof(AllWithBlockingNeuron));

            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[2].ConnectTo(midLayer1.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[3].ConnectTo(midLayer1.Neurons[0], Neuron.VEGETATIVE_STATE, false);

            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[1], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[1], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[2].ConnectTo(midLayer1.Neurons[1], Neuron.VEGETATIVE_STATE, false);

            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[2], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[2], Neuron.VEGETATIVE_STATE, false);

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[0], Neuron.VEGETATIVE_STATE, true);


            midLayer1.Neurons[2].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[1], Neuron.VEGETATIVE_STATE, true);

            midLayer1.Neurons[1].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[2], Neuron.VEGETATIVE_STATE, true);

            midLayer1.Neurons[0].ConnectTo(outLayer.Neurons[3], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[3], Neuron.VEGETATIVE_STATE, true);

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[4], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[4], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[4], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[4], Neuron.VEGETATIVE_STATE, false);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[4], Neuron.VEGETATIVE_STATE, false);

            network.ClearStates();
            inLayer.Neurons[0].State = 1;

            network.Tick(0.1);
            network.Tick(0.1);

            Assert.IsTrue(outLayer.Neurons[0].State > Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[1].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[2].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[3].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[4].State <= Neuron.VEGETATIVE_STATE);


            network.ClearStates();
            inLayer.Neurons[0].State = 1;
            inLayer.Neurons[1].State = 1;

            network.Tick(0.1);
            network.Tick(0.1);

            Assert.AreEqual(TestCase.ResultType.OkPositive, testCase.Test(network));

            Assert.IsTrue(outLayer.Neurons[0].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[1].State > Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[2].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[3].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[4].State <= Neuron.VEGETATIVE_STATE);



            network.ClearStates();
            inLayer.Neurons[0].State = 1;
            inLayer.Neurons[1].State = 1;
            inLayer.Neurons[2].State = 1;

            network.Tick(0.1);
            network.Tick(0.1);
            Assert.AreEqual(TestCase.ResultType.OkPositive, testCase.Test(network));

            Assert.IsTrue(outLayer.Neurons[0].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[1].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[2].State > Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[3].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[4].State <= Neuron.VEGETATIVE_STATE);



            network.ClearStates();
            inLayer.Neurons[0].State = 1;
            inLayer.Neurons[1].State = 1;
            inLayer.Neurons[2].State = 1;
            inLayer.Neurons[3].State = 1;

            network.Tick(0.1);
            network.Tick(0.1);
            Assert.AreEqual(TestCase.ResultType.OkPositive, testCase.Test(network));

            Assert.IsTrue(outLayer.Neurons[0].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[1].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[2].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[3].State > Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[4].State <= Neuron.VEGETATIVE_STATE);

            network.ClearStates();
            inLayer.Neurons[0].State = 1;
            inLayer.Neurons[1].State = 1;
            inLayer.Neurons[2].State = 1;
            inLayer.Neurons[3].State = 1;
            inLayer.Neurons[4].State = 1;

            network.Tick(0.1);
            network.Tick(0.1);
            Assert.AreEqual(TestCase.ResultType.OkPositive, testCase.Test(network));

            Assert.IsTrue(outLayer.Neurons[0].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[1].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[2].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[3].State <= Neuron.VEGETATIVE_STATE);
            Assert.IsTrue(outLayer.Neurons[4].State > Neuron.VEGETATIVE_STATE);

        }


        [TestMethod]
        public void SaveAndLoad()
        {
            Network network = new Network();
            network.NewLayer("in")
                .Add(3, typeof(AllNeuron))
                .Add(3, typeof(AnyWithBlockingNeuron));

            network.NewLayer("out")
                .Add(3, typeof(AnyNeuron))
                .Add(3, typeof(MaxOneNeuron))
                .Add(3, typeof(AllWithBlockingNeuron));

            network.Layers[0].ConnetToLayer(network.Layers[1], network);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (TextWriter writer = new StreamWriter(memoryStream))
                {
                    network.SaveNetwork(writer);
                    network.SaveNetwork("d:/network.test01.xml");
                }

                using (TextReader reader = new StreamReader(new MemoryStream(memoryStream.ToArray())))
                {
                    var newNetwork = Network.Load(reader);
                    newNetwork.SaveNetwork("d:/network.test02.xml");

                    Assert.AreEqual(network.Layers.Count, newNetwork.Layers.Count);
                    Assert.AreEqual(network.Synapses.Count, newNetwork.Synapses.Count);
                    Assert.AreEqual(network.SynapticActivity(), newNetwork.SynapticActivity());
                    for (int i = 0; i < network.Layers.Count; i++)
                    {
                        Assert.AreEqual(network.Layers[i].Neurons.Count, newNetwork.Layers.Where(l => l.Name == network.Layers[i].Name).FirstOrDefault()?.Neurons.Count);
                    }
                }
            }
        }
    }
}
