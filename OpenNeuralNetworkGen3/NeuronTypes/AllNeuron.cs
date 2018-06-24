using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using static OpenNeuralNetworkGen3.Program;

namespace OpenNeuralNetworkGen3
{
    /// <summary>
    /// neuron is active when any of the incomming synapses are active
    /// </summary>
    [Serializable]
    public class AllNeuron : Neuron
    {

        static Random random = new Random();


        public AllNeuron()
        {

            Frequency = 1;// random.NextDouble() + 0.3;
        }

        public override void Tick(double v, Network stepBackNetwork)
        {


            if (FrequencyState > 0) // neuron was activated and should finish its activation state
            {
                FrequencyState = FrequencyState + v / Frequency;
                if (FrequencyState > 1)
                {
                    FrequencyState = 0;
                    State = VEGETATIVE_STATE;
                }
                else
                {
                    State = FrequencyValue(FrequencyState);
                }
                return;
            }


            bool toBeActivated = true;
            foreach (var synapse in InConnections)
            {
                var state = stepBackNetwork.Neurons[synapse.FromNeuron.ID].State;

                if (state <= synapse.EasinessOfActivation)
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

            if (State > 1) State = 1;
            if (State < 0) State = 0;

        }
        public override Neuron Clone()
        {
            return new AllNeuron()
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
}
