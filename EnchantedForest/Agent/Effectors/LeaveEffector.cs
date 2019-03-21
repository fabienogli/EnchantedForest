using EnchantedForest.Environment;

namespace EnchantedForest.Agent.Effectors
{
    public class LeaveEffector : Effector
    {
        public LeaveEffector(Forest forest) : base(forest)
        {
        }

        public override bool DoIt()
        {
            return Forest.HandleAction(Action.Leave);
        }
    }
}