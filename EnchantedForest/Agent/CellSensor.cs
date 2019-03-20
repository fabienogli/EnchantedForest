using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class CellSensor : Sensor<Entity>
    {
        public Entity Observe(Forest forest)
        {
            return forest.ObserveCell();
        }
    }
}