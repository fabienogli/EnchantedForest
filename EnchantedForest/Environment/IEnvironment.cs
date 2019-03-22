

using System.Collections.Generic;
using EnchantedForest.Agent;
using EnchantedForest.Search;

namespace EnchantedForest.Environment
{
    public interface IEnvironment
    {
        int GetCostForAction(Action action);
        List<State> GetSuccessors(State currentState);
        double GetHeuristicForState(State state);
    }
}