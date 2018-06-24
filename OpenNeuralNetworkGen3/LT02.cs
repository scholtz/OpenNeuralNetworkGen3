using System;
using System.Collections.Generic;
using System.Text;

namespace OpenNeuralNetworkGen3
{
    /// <summary>
    /// Should return double [-1 , 1]
    /// </summary>
    public abstract class TestCase
    {
        public enum ResultType
        {
            OkPositive,
            FalsePositive,
            OkNegative,
            FalseNegative
        }
        public abstract void setRandomInitState(Network network);
        public abstract ResultType Test(Network network);
    }
    /*
    public class LT02 : TestCase
    {
        public override double? Test(Network network)
        {
            if (network.Layers[0].State == network.Layers[1].State) return null;

            var last = network.Layers.Count - 1;
            var diff = Math.Abs(network.Layers[0].State - network.Layers[last].State);
            if (network.Layers[0].State > 0.2)
                return network.Layers[0].State - network.Layers[last].State;
            if (network.Input[0].State < 0.2)
                return network.Layers[last].State - network.Layers[0].State;
            return 0;
            /*
            if (network.Input[0].State < 0.2 && network.Output[0].State > network.Output[1].State)
                return diff;

            return network.Output[0].State < network.Output[1].State;
        }
    }
/**/
}
