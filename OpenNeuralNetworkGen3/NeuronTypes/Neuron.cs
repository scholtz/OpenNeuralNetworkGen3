using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace OpenNeuralNetworkGen3
{

    abstract public class Neuron
    {
        #region Enums
        public enum DirectionTypeEnum
        {
            Input,
            Level1,
            Level2,
            Output
        }
        public enum NeuronTypeEnum
        {
            Math,
            Logical
        }
        public const double VEGETATIVE_STATE = 0.1;
        public const double MIN_LEVEL_FOR_ACTIVATION = 0;
        public const double MAX_LEVEL_FOR_ACTIVATION = 1;
        #endregion
        #region Private variables
        private Random random = new Random();
        #endregion
        #region Properties
        public Guid ID { get; set; } = Guid.NewGuid();
        public double Frequency { get; set; } = 0;

        public Layer Layer { get; set; }
        [XmlIgnore]
        public double State { get; set; } = 0;
        [XmlIgnore]
        public double FrequencyState { get; set; } = 0; // 0 - 1

        [XmlIgnore]
        public List<Synaption> OutConnections { get; set; } = new List<Synaption>();
        [XmlIgnore]
        public List<Synaption> InConnections { get; set; } = new List<Synaption>();

        #endregion
        #region Methods
        public double FrequencyValue(double frequencyState)
        {
            // 0   = 0.1
            // 0.2 = 1
            // 0.7 = 0.1/ 1.1
            // 1   = 0.1
            if (frequencyState < 0.2)
            {
                return (Math.Sin((Math.PI / 2) * frequencyState / 0.2) + 0.1) / 1.1;
            }
            if (frequencyState < 0.7)
            {
                return (Math.Cos((Math.PI / 2) * (frequencyState - 0.2) / 0.6) + 0.1) / 1.1;
            }

            return 0.1;
        }

        public Synaption ConnectTo(Neuron toNeuron, double EasinessOfActivation, bool IsBlocking)
        {
            var synaption = new Synaption(this, toNeuron, EasinessOfActivation, IsBlocking);
            this.Layer.Network.Synapses.Add(synaption);
            return synaption;
        }

        internal bool IsConnectedTo(Neuron toNeuron)
        {
            if (OutConnections.Where(n => n.ID == toNeuron.ID).Count() > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Easiness to learn evaluates how easy it is possible to change the neuron activness state
        /// 
        /// If the value is 0, it means that it cannot learn anything new, so incomming synapses will not change EasinessOfActivation with this neuron
        /// 
        /// If the value is 1, it means that incomming synapses will easily change its EasinessOfActivation 
        /// 
        /// </summary>
        public void SupportOkNegative(double v)
        {
            foreach (var synapse in InConnections)
            {
                synapse.EasinessToLearn = Math.Max(0.00001, synapse.EasinessToLearn / 1.001);
            }
        }

        public void Support(double v)
        {
            bool isActive = State > Neuron.VEGETATIVE_STATE;
            //if (!isActive) v = v / 10;
            foreach (var synapse in InConnections)
            {
                if (isActive)
                {
                    /*
                    var val = synapse.EasinessOfActivation - EasinessToLearn * random.NextDouble() * v / 100;

                    if (val > 0.07)
                    {
                        synapse.EasinessOfActivation = val;
                    }
                    /**/
                }
                else
                {
                    /*
                    var val = synapse.EasinessOfActivation + EasinessToLearn * random.NextDouble() * v / 100;

                    if (val <= 0.4)
                    {
                        synapse.EasinessOfActivation = val;
                    }
                    /**/

                }
            }
            foreach (var synapse in InConnections)
            {
                synapse.EasinessToLearn = Math.Max(0.00001, synapse.EasinessToLearn / 1.001);
            }
        }

        public void NegateFalseNegative(double v)
        {
            
            bool blockingFound = false;
            foreach (var synapse in InConnections.Where(s => s.IsBlocking))
            {
                var signalFromIsActive = synapse.FromNeuron.State > synapse.EasinessOfActivation;
                if (signalFromIsActive)
                {
                    blockingFound = true;
                    synapse.Negate(synapse.EasinessToLearn * v / 1000);
                    synapse.EasinessToLearn = Math.Min(1, synapse.EasinessToLearn * 1.01);
                }
                //if (!signalFromIsActive) 

            }
            if (blockingFound) return;
            /**/
            foreach (var synapse in InConnections)//.Where(s => !s.IsBlocking)
            {
                var signalFromIsActive = synapse.FromNeuron.State > synapse.EasinessOfActivation;
                if (signalFromIsActive)
                {
                    synapse.Support(synapse.EasinessToLearn * v  / 1000);
                }
                else
                if (!signalFromIsActive)
                {
                    synapse.Support(synapse.EasinessToLearn * v * 2 / 1000);
                }

                synapse.EasinessToLearn = Math.Min(1, synapse.EasinessToLearn * 1.01);
            }
        }
        public void NegateFalsePositive(double v)
        {
            //if (isActive) v = v * 2;
            foreach (var synapse in InConnections.Where(s => s.IsBlocking))
            {
                var signalFromIsActive = synapse.FromNeuron.State > synapse.EasinessOfActivation;
                if (!signalFromIsActive) v = v / 2;
                synapse.Support(synapse.EasinessToLearn * v / 1000);
                synapse.EasinessToLearn = Math.Min(1, synapse.EasinessToLearn * 1.01);
            }
            foreach (var synapse in InConnections.Where(s => !s.IsBlocking))
            {
                var signalFromIsActive = synapse.FromNeuron.State > synapse.EasinessOfActivation;


                if (signalFromIsActive)
                {
                    synapse.Negate(synapse.EasinessToLearn * v / 1000);
                }
                else
                if (!signalFromIsActive)
                {
                    synapse.Support(synapse.EasinessToLearn * v / 1000);
                }

                synapse.EasinessToLearn = Math.Min(1, synapse.EasinessToLearn * 1.01);
            }

        }

        public abstract void Tick(double v, Network stepBackNetwork);
        public abstract Neuron Clone();
        #endregion
        #region Autogenerated
        public override bool Equals(object obj)
        {
            var neuron = obj as Neuron;
            return neuron != null &&
                   ID.Equals(neuron.ID);
        }

        public override int GetHashCode()
        {
            return 1213502048 + EqualityComparer<Guid>.Default.GetHashCode(ID);
        }
        #endregion
    }
}
