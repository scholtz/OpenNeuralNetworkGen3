#define NEW
#define LEARN

#define NDEBUGOUT

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;



namespace OpenNeuralNetworkGen3
{
    public class Program
    {





        static Random r = new Random();


        static void Main(string[] args)
        {
            Network network = new Network();


#if NEW
            //network.GenerateNetwork(5, 5, 30, 10, 30, 10, 2, 10);

            /*
            var inLayer = new Layer();
            inLayer.Add(5, typeof(MathNeuron));
            network.Add(inLayer);
            var layer2 = new Layer();
            layer2.Add(30, typeof(MathNeuron));
            layer2.Add(10, typeof(CompareNeuron));
            network.Add(layer2);
            var layer3 = new Layer();
            layer3.Add(30, typeof(MathNeuron));
            layer3.Add(10, typeof(CompareNeuron));
            network.Add(layer3);
            var outLayer = new Layer();
            outLayer.Add(5, typeof(MathNeuron));
            network.Add(outLayer);
            inLayer.ConnetToLayer(layer2, network, 50);
            layer2.ConnetToLayer(layer2, network, 100);
            layer2.ConnetToLayer(layer3, network, 50);
            layer3.ConnetToLayer(layer3, network, 100);
            layer3.ConnetToLayer(outLayer, network, 50);
            /**/

            var inLayer = new Layer();
            inLayer.Add(5, typeof(AnyNeuron));
            network.Add(inLayer);

            var midLayer1 = new Layer();
            midLayer1.Add(3, typeof(AnyWithBlockingNeuron));
            network.Add(midLayer1);

            var outLayer = new Layer();
            outLayer.Add(4, typeof(AnyWithBlockingNeuron));
            outLayer.Add(1, typeof(AllWithBlockingNeuron));
            network.Add(outLayer);

            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[0], 0.1, false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[0], 0.1, false);
            inLayer.Neurons[2].ConnectTo(midLayer1.Neurons[0], 0.1, false);
            inLayer.Neurons[3].ConnectTo(midLayer1.Neurons[0], 0.1, false);

            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[1], 0.1, false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[1], 0.1, false);
            inLayer.Neurons[2].ConnectTo(midLayer1.Neurons[1], 0.1, false);

            inLayer.Neurons[0].ConnectTo(midLayer1.Neurons[2], 0.1, false);
            inLayer.Neurons[1].ConnectTo(midLayer1.Neurons[2], 0.1, false);

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[0], 0.1, false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[0], 0.1, true);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[0], 0.1, true);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[0], 0.1, true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[0], 0.1, true);


            midLayer1.Neurons[2].ConnectTo(outLayer.Neurons[1], 0.1, false);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[1], 0.1, true);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[1], 0.1, true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[1], 0.1, true);

            midLayer1.Neurons[1].ConnectTo(outLayer.Neurons[2], 0.1, false);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[2], 0.1, true);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[2], 0.1, true);

            midLayer1.Neurons[0].ConnectTo(outLayer.Neurons[3], 0.1,false);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[3], 0.1, true);

            inLayer.Neurons[0].ConnectTo(outLayer.Neurons[4], 0.1, false);
            inLayer.Neurons[1].ConnectTo(outLayer.Neurons[4], 0.1, false);
            inLayer.Neurons[2].ConnectTo(outLayer.Neurons[4], 0.1, false);
            inLayer.Neurons[3].ConnectTo(outLayer.Neurons[4], 0.1, false);
            inLayer.Neurons[4].ConnectTo(outLayer.Neurons[4], 0.1, false);






            network.SaveNetwork("d:\\n5-network.new.xml");
#else
            network.LoadNetwork("d:\\g02-network-alpha.xml");
#endif

#if LEARN
            /*
            var LT02 = new LT02();
            double? ok = null;
            for (int i = 0; i < 1000; i++)
            {
                network.StudyIteration(LT02, 3, 1);
                network.Test(LT02, 10, 3, true);
                //Console.Write(".");
                //Console.WriteLine("Activity: "+network.SynapticActivity());
            }
            /**/
            network.SaveNetwork("d:\\n5-network2.xml");
#endif

            Console.ReadLine();

        }
    }
}