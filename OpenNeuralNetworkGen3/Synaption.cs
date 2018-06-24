using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OpenNeuralNetworkGen3
{
    [XmlInclude(typeof(AllNeuron))]
    [XmlInclude(typeof(AllWithBlockingNeuron))]
    [XmlInclude(typeof(AnyNeuron))]
    [XmlInclude(typeof(AnyWithBlockingNeuron))]
    [XmlInclude(typeof(MaxOneNeuron))]
    [Serializable]
    public class Synaption
    {
        public Synaption()
        {

        }
        public Synaption(Neuron From, Neuron To, double EasinessOfActivation, bool IsBlocking)
        {
            FromNeuron = From;
            ToNeuron = To;
            this.EasinessOfActivation = EasinessOfActivation;
            this.IsBlocking = IsBlocking;
            FromNeuron.OutConnections.Add(this);
            ToNeuron.InConnections.Add(this);
        }
        public Guid ID { get; set; } = Guid.NewGuid();
        public Neuron FromNeuron;
        public Neuron ToNeuron;
        public double EasinessOfActivation;
        /// <summary>
        /// Easiness To Learn new things
        /// 
        /// Higher number means that learnign will have higher impact.
        /// Lower number means that it will be more difficult to learn something new this neuron.
        /// </summary>
        public double EasinessToLearn { get; set; } = 0.5; // <0 - 1>
        /// <summary>
        /// When incomming neuron is active, when is blocking is true, do not activate other neuron
        /// </summary>
        public bool IsBlocking = false;

        public override bool Equals(object obj)
        {
            var synapsia = obj as Synaption;
            return synapsia != null &&
                   ID.Equals(synapsia.ID);
        }

        public override int GetHashCode()
        {
            return 1213502048 + EqualityComparer<Guid>.Default.GetHashCode(ID);
        }

        internal Synaption Clone(Network newNetwork)
        {
            Synaption ret = new Synaption();
            ret.EasinessOfActivation = EasinessOfActivation;
            ret.IsBlocking = IsBlocking;
            ret.FromNeuron = newNetwork.Neurons[this.FromNeuron.ID];
            ret.ToNeuron = newNetwork.Neurons[this.ToNeuron.ID];
            return ret;
        }

        internal void Negate(double v)
        {
            var val = EasinessOfActivation + v;
            if (val <= Neuron.MAX_LEVEL_FOR_ACTIVATION)
            {
                EasinessOfActivation = val;
            }
        }
        internal void Support(double v)
        {
            var val = EasinessOfActivation - v;
            if (val >= Neuron.MIN_LEVEL_FOR_ACTIVATION)
            {
                EasinessOfActivation = val;
            }
        }
    }
}
