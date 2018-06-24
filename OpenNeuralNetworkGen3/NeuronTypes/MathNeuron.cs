using System;
using System.Collections.Generic;
using System.Text;
using static OpenNeuralNetworkGen3.Program;

namespace OpenNeuralNetworkGen3
{
    /*
    [Serializable]
    public class MathNeuron : Neuron
    {
        private Random random = new Random();


        public override void Tick(double v, Network stepBackNetwork)
        {
            if (InConnections.Count == 0) return;

            double total = 0;
            foreach (var synapse in InConnections)
            {
                var state = stepBackNetwork.Neurons[synapse.FromNeuron.ID].State;
                if (state > synapse.EasinessOfActivation)
                {
                    total += state;
                }
            }
            State = Sigmoid(total);

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
            return new MathNeuron()
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
