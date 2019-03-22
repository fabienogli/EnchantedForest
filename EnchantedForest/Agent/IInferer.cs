using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public interface IInferer
    {
        ProbabilityMatrix Infere(Entity observe);
    }
}