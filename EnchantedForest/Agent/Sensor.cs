using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public interface Sensor<T>
    {
        T Observe(Forest forest);
    }
}