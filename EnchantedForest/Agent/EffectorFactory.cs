
using System;
using System.Collections.Generic;
using EnchantedForest.Environment;
using Action = EnchantedForest.Agent.Action;

namespace EnchantedForest.Agent
{
    public class EffectorFactory
    {
        public static Forest Forest;
        public static Effector GetEffector(Action action)
        {
            switch (action)
            {
                case Action.Left:
                    return new LeftEffector(Forest);
                case Action.Right:
                    return new RightEffector(Forest);
                case Action.Up:
                    return new UpEffector(Forest);
                case Action.Down:
                    return new DownEffector(Forest);
                    break;
                case Action.ThrowRock:
                    return new ShootEffector(Forest);
                case Action.Leave:
                    return new LeaveEffector(Forest);
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }
        }
        
        public static Dictionary<Action, Effector> GetEffectors()
        {
            Dictionary<Action, Effector> dictionary = new Dictionary<Action, Effector>();
            dictionary.Add(Action.Up, GetEffector(Action.Up));
            dictionary.Add(Action.Down, GetEffector(Action.Down));
            dictionary.Add(Action.Left, GetEffector(Action.Left));
            dictionary.Add(Action.Right, GetEffector(Action.Right));
            dictionary.Add(Action.ThrowRock, GetEffector(Action.ThrowRock));
            dictionary.Add(Action.Leave, GetEffector(Action.Leave));
            return dictionary;
        }
    }
}