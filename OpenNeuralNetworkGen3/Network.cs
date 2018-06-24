using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace OpenNeuralNetworkGen3
{
    
    [Serializable]
    [XmlRoot(ElementName = "Network")]
    public class Network
    {
        #region Properties

        public Guid ID { get; set; } = Guid.NewGuid();
        public List<Layer> Layers { get; set; } = new List<Layer>();
        public List<Synaption> Synapses { get; set; } = new List<Synaption>();

        [XmlIgnore]
        private Dictionary<Guid, Neuron> neuronsCache = new Dictionary<Guid, Neuron>();
        [XmlIgnore]
        public Dictionary<Guid, Neuron> Neurons
        {
            get {
                if (neuronsCache.Count > 0)
                {
                    return neuronsCache;
                }
                return neuronsCache = Layers.Select(l => l.Neurons).SelectMany(n => n).ToDictionary(n => n.ID);
            }
        }
        #endregion
        #region Private variables
        private static XmlSerializer sserializer = new XmlSerializer(typeof(Synaption[]));

        #endregion
        #region Methods
        public Layer NewLayer(string Name = "")
        {
            if (string.IsNullOrEmpty(Name)) Name = Guid.NewGuid().ToString();
            var layer = new Layer() { Network = this, Name = Name };
            Add(layer);
            return layer;
        }

        public void ClearStates()
        {
            foreach (var level in Layers)
            {
                foreach (var neuron in level.Neurons)
                {
                    neuron.State = 0;
                    neuron.FrequencyState = 0;
                }
            }
        }
        public void Add(Layer layer)
        {
            Layers.Add(layer);
        }




        public static Network Load(TextReader reader)
        {
            var ret = new Network();
            ret.LoadNetwork(reader);
            return ret;
        }
        public static Network Load(string file)
        {
            var ret = new Network();
            ret.LoadNetwork(file);
            return ret;
        }

        public void LoadNetwork(TextReader reader)
        {

            Synapses = new List<Synaption>((Synaption[])sserializer.Deserialize(reader));
            IEnumerable<(Guid layerId, string name)> layerIds = Synapses.Select(n => n.ToNeuron).Select(s => (s.Layer.ID, s.Layer.Name)).Union(Synapses.Select(n => n.FromNeuron).Select(s => (s.Layer.ID, s.Layer.Name))).Distinct().ToArray();
            foreach (var layerId in layerIds)
            {
                var layer = new Layer() { ID = layerId.layerId,  Name = layerId.name };
                this.Layers.Add(layer);
                var allneurons = Synapses.Select(n => n.ToNeuron).Union(Synapses.Select(n => n.FromNeuron)).Distinct().ToArray();
                foreach (var neuron in allneurons.Where(n => n.Layer.ID == layer.ID).ToArray())
                {
                    neuron.Layer = layer;
                    layer.Add(neuron);
                }
            }


            var neurons = Synapses.Select(s => s.FromNeuron).Union(Synapses.Select(s => s.ToNeuron)).ToDictionary(s => s.ID);
            foreach (var n in neurons.Values)
            {
                n.InConnections = Synapses.Where(s => s.ToNeuron.ID == n.ID).ToList();
                n.OutConnections = Synapses.Where(s => s.FromNeuron.ID == n.ID).ToList();
            }
            foreach (var s in Synapses)
            {
                s.FromNeuron = neurons[s.FromNeuron.ID];
                s.ToNeuron = neurons[s.ToNeuron.ID];
            }
            /*
            Input = neurons.Values.Where(n => n.DirectionType == Neuron.DirectionTypeEnum.Input).ToList();
            Output = neurons.Values.Where(n => n.DirectionType == Neuron.DirectionTypeEnum.Output).ToList();
            Level1 = neurons.Values.Where(n => n.DirectionType == Neuron.DirectionTypeEnum.Level1).ToList();
            Level2 = neurons.Values.Where(n => n.DirectionType == Neuron.DirectionTypeEnum.Level2).ToList();
            /**/
        }

        public double SumOfEasinessToLearn()
        {
            return Neurons.Sum(n => n.Value.EasinessToLearn);
        }

        public void LoadNetwork(string filename)
        {
            using (TextReader reader = File.OpenText(filename))
            {
                LoadNetwork(reader);
            }
        }
        public void SaveNetwork(string filename)
        {
            using (TextWriter writer = File.CreateText(filename))
            {
                SaveNetwork(writer);
            }
        }

        public void SaveNetwork(TextWriter writer)
        {
            sserializer.Serialize(writer, Synapses.ToArray());
        }


        public double SynapticActivity()
        {
            double sum = 0;
            int count = 0;
            for (int layerIndex = 1; layerIndex < Layers.Count - 1; layerIndex++)
            {
                sum += Layers[layerIndex].Neurons.Sum(n => n.State);
                count += Layers[layerIndex].Neurons.Count;
            }
            return sum / count;
        }
        /*
        public void GenerateNetwork(int inputCount, int outputCount, int level1MathNeurons, int level1LogicalNeurons, int level2MathNeurons, int level2LogicalNeurons, int minNeuronsConnections = 3, int maxNeuronConnections = 7)
        {
            var rand = new Random();
            Network n = this;
            for (int i = 0; i < level1MathNeurons; i++)
            {
                var neuron = new MathNeuron() { DirectionType = Neuron.DirectionTypeEnum.Level1 };

                n.Level1.Add(neuron);
            }
            for (int i = 0; i < level1LogicalNeurons; i++)
            {
                var neuron = new CompareNeuron() { DirectionType = Neuron.DirectionTypeEnum.Level1 };

                n.Level1.Add(neuron);
            }

            for (int i = 0; i < level2MathNeurons; i++)
            {
                var neuron = new MathNeuron() { DirectionType = Neuron.DirectionTypeEnum.Level2 };
                n.Level2.Add(neuron);
            }
            for (int i = 0; i < level2LogicalNeurons; i++)
            {
                var neuron = new CompareNeuron() { DirectionType = Neuron.DirectionTypeEnum.Level2 };
                n.Level2.Add(neuron);
            }

            var connections = rand.Next(minNeuronsConnections * level1MathNeurons, maxNeuronConnections * level1MathNeurons);
            for (int i = 0; i < connections; i++)
            {
                var from = rand.Next(0, n.Level1.Count - 1);
                var fromNeuron = n.Level1[from];
                var to = rand.Next(0, n.Level1.Count - 1);
                var toNeuron = n.Level1[to];
                if (from == to) continue;// do not connect neurons to its own
                var synapse = new Synaption() { FromNeuron = fromNeuron, ToNeuron = toNeuron, EasinessOfActivation = (double)rand.NextDouble() / 2 };
                if (n.Synapses.Where(s => s.FromNeuron.ID == fromNeuron.ID && s.ToNeuron.ID == toNeuron.ID).FirstOrDefault() != null)
                {
                    continue;
                }

                fromNeuron.OutConnections.Add(synapse);
                toNeuron.InConnections.Add(synapse);
                n.Synapses.Add(synapse);
            }

            connections = rand.Next(minNeuronsConnections * Math.Min(level1MathNeurons, level2MathNeurons), maxNeuronConnections * level1MathNeurons);
            for (int i = 0; i < connections; i++)
            {
                var from = rand.Next(0, n.Level1.Count - 1);
                var fromNeuron = n.Level1[from];
                var to = rand.Next(0, n.Level2.Count - 1);
                var toNeuron = n.Level2[to];
                if (from == to) continue;// do not connect neurons to its own
                var synapse = new Synaption() { FromNeuron = fromNeuron, ToNeuron = toNeuron, EasinessOfActivation = (double)rand.NextDouble() / 2 };
                if (n.Synapses.Where(s => s.FromNeuron.ID == fromNeuron.ID && s.ToNeuron.ID == toNeuron.ID).FirstOrDefault() != null) {
                    continue;
                }
                fromNeuron.OutConnections.Add(synapse);
                toNeuron.InConnections.Add(synapse);
                n.Synapses.Add(synapse);
            }
            connections = rand.Next(minNeuronsConnections * level2MathNeurons, maxNeuronConnections * level2MathNeurons);
            for (int i = 0; i < connections; i++)
            {
                var from = rand.Next(0, n.Level2.Count - 1);
                var fromNeuron = n.Level2[from];
                var to = rand.Next(0, n.Level2.Count - 1);
                var toNeuron = n.Level2[to];
                if (from == to) continue;// do not connect neurons to its own
                var synapse = new Synaption() { FromNeuron = fromNeuron, ToNeuron = toNeuron, EasinessOfActivation = (double)rand.NextDouble() / 2 };
                if (n.Synapses.Where(s => s.FromNeuron.ID == fromNeuron.ID && s.ToNeuron.ID == toNeuron.ID).FirstOrDefault() != null)
                {
                    continue;
                }

                fromNeuron.OutConnections.Add(synapse);
                toNeuron.InConnections.Add(synapse);
                n.Synapses.Add(synapse);
            }
            for (int i = 0; i < inputCount; i++)
            {
                var neuron = new MathNeuron() { DirectionType = Neuron.DirectionTypeEnum.Input };
                neuron.Frequency = 1;// 0.3;
                connections = maxNeuronConnections;
                for (int j = 0; j < connections; j++)
                {
                    var fromNeuron = neuron;
                    var to = rand.Next(0, n.Level1.Count - 1);
                    var toNeuron = n.Level1[to];
                    var synapse = new Synaption() { FromNeuron = fromNeuron, ToNeuron = toNeuron, EasinessOfActivation = (double)rand.NextDouble() / 2 };
                    if (n.Synapses.Where(s => s.FromNeuron.ID == fromNeuron.ID && s.ToNeuron.ID == toNeuron.ID).FirstOrDefault() != null)
                    {
                        continue;
                    }

                    n.Synapses.Add(synapse);
                    fromNeuron.OutConnections.Add(synapse);
                    toNeuron.InConnections.Add(synapse);

                }
                n.Input.Add(neuron);
            }


            for (int i = 0; i < outputCount; i++)
            {
                var neuron = new MathNeuron() { DirectionType = Neuron.DirectionTypeEnum.Output };
                neuron.Frequency = 1;
                connections = maxNeuronConnections;
                for (int j = 0; j < connections; j++)
                {
                    var from = rand.Next(0, n.Level2.Count - 1);
                    var fromNeuron = n.Level2[from];
                    var toNeuron = neuron;
                    var synapse = new Synaption() { FromNeuron = fromNeuron, ToNeuron = toNeuron, EasinessOfActivation = (double)rand.NextDouble() / 2 };
                    if (n.Synapses.Where(s => s.FromNeuron.ID == fromNeuron.ID && s.ToNeuron.ID == toNeuron.ID).FirstOrDefault() != null)
                    {
                        continue;
                    }

                    n.Synapses.Add(synapse);
                    fromNeuron.OutConnections.Add(synapse);
                    toNeuron.InConnections.Add(synapse);
                }
                n.Output.Add(neuron);
            }
            Stats();
        }
        /**/
        public void Stats()
        {
            for (int layerIndex = 0; layerIndex < Layers.Count; layerIndex++)
            {
                Console.WriteLine("## Layer: " + layerIndex);
                var types = Layers[layerIndex].Neurons.Select(s => s.GetType()).GroupBy(s => s.Name);
                foreach (var type in types)
                {
                    Console.WriteLine(type.Key + ": " + type.Count());
                }
            }
            Console.WriteLine("#Synapsies    : " + this.Synapses.Count);

        }

        public void Tick(double v)
        {
            var old = this.Clone();

            foreach (var neuron in Synapses.Select(s=>s.ToNeuron).Distinct())
            {
                neuron.Tick(v, old);
            }
        }

        private Random random = new Random();
        public void Support(double v)
        {
            foreach (var neuron in Synapses.Select(s => s.ToNeuron).Distinct())
            {
                neuron.Support(v);
            }

        }
        public void SupportOkNegative(double v)
        {
            foreach (var neuron in Synapses.Select(s => s.ToNeuron).Distinct())
            {
                neuron.SupportOkNegative(v);
            }
        }

        public void NegateFalseNegative(double v)
        {
            foreach (var neuron in Synapses.Select(s => s.ToNeuron).Distinct())
            {
                neuron.NegateFalseNegative(v);
            }
        }

        public void NegateFalsePositive(double v)
        {
            foreach (var neuron in Synapses.Select(s => s.ToNeuron).Distinct())
            {
                neuron.NegateFalsePositive(v);
            }
        }
        public TestCase.ResultType Test(TestCase function)
        {
            ClearStates();
            function.setRandomInitState(this);
            Tick(0.1);
            return function.Test(this);
        }
        public double Test(TestCase function, int Tries, int Steps = 5, bool consoleOut = false)
        {

            int ok = 0;
            int fail = 0;
            for (int i = 0; i < Tries; i++)
            {
#if DEBUGOUT
                Console.Write("\n" + input + ": ");
#endif
                ClearStates();
                function.setRandomInitState(this);

                for (int ii = 0; ii < Steps; ii++)
                {
                    Tick(0.1);
                }

                var result = function.Test(this);
                if (result > 0)
                {
                    ok++;
                }
                if (result < 0)
                {
                    fail++;
                }

            }
            if (consoleOut)
            {
                Console.Write("\nOK: " + ok + " / " + fail + " :FAILED");
            }
            if (ok + fail == 0) return -1;
            return (double)ok / (ok + fail);
        }
        public void StudyIteration(TestCase function, int steps, double weight)
        {
            var caseM = Test(function);
            switch (caseM)
            {
                case TestCase.ResultType.OkNegative:
                    SupportOkNegative(weight);
                    break;
                case TestCase.ResultType.OkPositive:
                    Support(weight);
                    break;
                case TestCase.ResultType.FalsePositive:
                    NegateFalsePositive(weight * 10);
                    break;
                case TestCase.ResultType.FalseNegative:
                    NegateFalseNegative(weight * 10);
                    break;
            }

            return;
#if X
            var input = random.NextDouble();

            double case1 = Test(function, 10, steps, false);
            double case2 = Test(function, 5, steps, false);
            double case3 = Test(function, 1, steps, false);

            if (case1 > 0.7 && case2 > 0.7 && case3 > 0.7)
            {
                Console.WriteLine("Support: " + case1 + " " + case2 + " " + case3);
                Support(weight);
            }
            if (case1 < 0.3 && case2 < 0.3 && case3 < 0.3)
            {
                Console.WriteLine("Negate: " + case1 + " " + case2 + " " + case3);
                Negate(weight);
            }
            if (case1 == -1 && case2 == -1 && case3 == -1)
            {
                Console.WriteLine("Randomize: " + case1 + " " + case2 + " " + case3);
                if (random.NextDouble() > 0.5)
                {
                    Negate(weight);
                }
                else
                {
                    Support(weight);
                }
            }
#endif
        }
        public Network Clone()
        {
            var newNetwork = new Network();
            foreach (var layer in this.Layers)
            {
                var newLayer = newNetwork.NewLayer();
                newLayer.ID = layer.ID;
                foreach (var neuron in layer.Neurons)
                {
                    newLayer.Add(neuron.Clone());
                }
            }
            foreach (var item in this.Synapses)
            {
                newNetwork.Synapses.Add(item.Clone(newNetwork));
            }
            return newNetwork;
        }
#endregion
#region Autogenerated methods
        public override bool Equals(object obj)
        {
            var network = obj as Network;
            return network != null &&
                   ID.Equals(network.ID);
        }

        public override int GetHashCode()
        {
            return 1213502048 + EqualityComparer<Guid>.Default.GetHashCode(ID);
        }
#endregion
    }
}
