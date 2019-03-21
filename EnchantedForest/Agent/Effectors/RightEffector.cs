using EnchantedForest.Environment;

namespace EnchantedForest.Agent.Effectors
{
    public class RightEffector : Effector
    {
        public RightEffector(Forest forest) : base(forest)
        {
        }

        public override bool DoIt()
        {
            Forest.HandleAction(Action.Right);
            return true;
        }
    }
}