using EnchantedForest.Environment;

namespace EnchantedForest.Agent.Effectors
{
    public class DeathSensor: Sensor<bool>
    {
        public bool Observe(Forest forest)
        {
            return forest.IsAgentDead();
        }
    }
}