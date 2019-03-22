

using EnchantedForest.Agent;

namespace EnchantedForest.Environment
{
    public interface IEnvironment
    {
        int GetCostForAction(Action action);
    }
}