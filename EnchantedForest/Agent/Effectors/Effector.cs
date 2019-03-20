using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public abstract class Effector
    
    {
        protected Forest Forest { get;}

        protected Effector(Forest forest)
        {
            Forest = forest;
        }

        public abstract bool DoIt();

    }
}