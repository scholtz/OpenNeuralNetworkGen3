using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using static OpenNeuralNetworkGen3.Program;

namespace OpenNeuralNetworkGen3
{
    /*
    [Serializable]
    public class CompareNeuron : Neuron
    {

        static Random random = new Random();


        public CompareNeuron()
        {

            Frequency = 1;// random.NextDouble() + 0.3;
        }

        public override void Tick(double v, Network stepBackNetwork)
        {


            if(FrequencyState > 0) // neuron was activated and should finish its activation state
            {
                FrequencyState = FrequencyState + v / Frequency;
                if (FrequencyState > 1) {
                    FrequencyState = 0;
                    State = VEGETATIVE_STATE;
                } else
                {
                    State = FrequencyValue(FrequencyState);
                }
                return;
            }

            if (InConnections.Where(s => s.FromNeuron.State > s.EasinessOfActivation).Count() == InConnections.Count)
            {

                bool toBeActivated = true;
                foreach (var synapse in InConnections)
                {
                    var state = stepBackNetwork.Neurons[synapse.FromNeuron.ID].State;

                    if (state < synapse.EasinessOfActivation)
                    {
                        toBeActivated = false;
                        break;
                    }
                }
                if (toBeActivated)
                {
                    FrequencyState = FrequencyState + v / Frequency;
                    if (FrequencyState > 1) FrequencyState = v;
                    State = FrequencyValue(FrequencyState);
                }

            }
            else
            {
                State = VEGETATIVE_STATE;
            }
            if (State > 1) State = 1;
            if (State < 0) State = 0;

        }
        private double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        private double Pow3(double x)
        {
            return Math.Pow(1 + x, 3) - 1;
        }


        public override Neuron Clone()
        {
            return new CompareNeuron()
            {
                ID = this.ID,
#if CloneAll
                Frequency = Frequency,
#endif
                FrequencyState = FrequencyState,
#if CloneAll
                InConnections = InConnections,
                OutConnections = OutConnections,
#endif
                State = State
            };
        }
    }
    /**/
}
