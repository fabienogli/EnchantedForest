using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class UpEffector : Effector
    {
        public UpEffector(Forest forest) : base(forest)
        {
        }

        public override bool DoIt()
        {
           Forest.HandleAction(Action.Up);
           return true;
        }
    }
}