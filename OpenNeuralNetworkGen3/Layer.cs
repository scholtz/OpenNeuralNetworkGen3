using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OpenNeuralNetworkGen3
{

    [Serializable]

    public class Layer
    {
        public Layer()
        {

        }

        #region Properties
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "";
        [XmlIgnore]
        public List<Neuron> Neurons { get; set; } = new List<Neuron>();

        [XmlIgnore]
        public Network Network { get; set; }
        #endregion
        #region Private variables

        private Random random = new Random();
        #endregion
        #region Methods
        public Layer Add(int v, Type type)
        {
            for (int i = 0; i < v; i++)
            {
                var neuronInstance = System.Reflection.Assembly.GetAssembly(type).CreateInstance(type.ToString());
                if (neuronInstance == null)
                {
                    throw new Exception("Unable to create neuron of type " + type);
                }
                var neuron = neuronInstance as Neuron;
                neuron.Layer = this;
                Neurons.Add((Neuron)neuron);

            }
            return this;
        }
        public void ConnetToLayer(Layer layerTo, Network network, int? Connections = null, bool isBlocking = false)
        {
            if (!Connections.HasValue)
            {
                // bind every neuron in first layer to second layer

                foreach(var fromNeuron in Neurons)
                {
                    foreach(var toNeuron in layerTo.Neurons)
                    {
                        if (!fromNeuron.IsConnectedTo(toNeuron))
                        {
                            fromNeuron.ConnectTo(toNeuron, (double)random.NextDouble() / 2, isBlocking);
                        }

                    }
                }
            }
            else
            {
                for (int i = 0; i < Connections; i++)
                {
                    var from = random.Next(0, this.Neurons.Count - 1);
                    var fromNeuron = this.Neurons[from];

                    var to = random.Next(0, layerTo.Neurons.Count - 1);
                    var toNeuron = layerTo.Neurons[to];

                    if (fromNeuron.Equals(toNeuron)) continue;// do not connect neurons to its own
                    if (!fromNeuron.IsConnectedTo(toNeuron))
                    {
                        fromNeuron.ConnectTo(toNeuron, (double)random.NextDouble() / 2, isBlocking);
                    }
                }
            }
        }

        internal void Add(Neuron neuron)
        {
            neuron.Layer = this;
            Neurons.Add(neuron);
        }
        #endregion
    }
}
