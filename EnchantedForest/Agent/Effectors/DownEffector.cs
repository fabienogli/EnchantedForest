using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class DownEffector : Effector
    {
        public DownEffector(Forest forest) : base(forest)
        {
        }

        public override bool DoIt()
        {
            Forest.HandleAction(Action.Down);
            return true;
        }
    }
}