using EnchantedForest.Environment;

namespace EnchantedForest.Agent
{
    public class LeaveEffector : Effector
    {
        public LeaveEffector(Forest forest) : base(forest)
        {
        }

        public override bool DoIt()
        {
            Forest.HandleAction(Action.Leave);
            return true;
        }
    }
}