
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
        
        public Forest Forest { get; }

        public State(Map entities, Action action, Forest forest)
        {
            Current = new Tuple<Map, Action>(entities, action);
            Forest = forest;
        }
    }
    
}