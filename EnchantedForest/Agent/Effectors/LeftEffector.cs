using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class LeftEffector : Effector
    {
        public LeftEffector(Forest forest) : base(forest)
        {
        }

        public override bool DoIt()
        {
            Forest.HandleAction(Action.Left);
            return true;
        }
    }
}