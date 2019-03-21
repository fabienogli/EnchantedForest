using EnchantedForest.Environment;

namespace EnchantedForest.Agent.Effectors
{
    public class ShootEffector : Effector
    {
        private Action ShootingAction {get;}
        public ShootEffector(Forest forest, Action shootingAction) : base(forest)
        {
            ShootingAction = shootingAction;
        }

        public override bool DoIt()
        {
            Forest.HandleAction(ShootingAction);
            return true;
        }
    }
}