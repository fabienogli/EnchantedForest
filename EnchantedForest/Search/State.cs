
using System;
using EnchantedForest.Environment;
using Action = EnchantedForest.Agent.Action;

namespace EnchantedForest.Search
{
    public class State
    {
        public Tuple<Map, Action> Current { get; }
        
        public Action Action => Current.Item2;

        public Map Map => Current.Item1;

        public State(Map entities, Action action)
        {
            Current = new Tuple<Map, Action>(entities, action);
        }
    }
}