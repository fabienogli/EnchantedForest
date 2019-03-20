
using System;
using System.Collections.Generic;
using EnchantedForest.Agent.Effectors;
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
                case Action.ThrowUp:
                case Action.ThrowDown:
                case Action.ThrowLeft:
                case Action.ThrowRight:
                    return new ShootEffector(Forest, action);
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
            dictionary.Add(Action.ThrowLeft, GetEffector(Action.ThrowLeft));
            dictionary.Add(Action.ThrowRight, GetEffector(Action.ThrowRight));
            dictionary.Add(Action.ThrowUp, GetEffector(Action.ThrowUp));
            dictionary.Add(Action.ThrowDown, GetEffector(Action.ThrowDown));
            dictionary.Add(Action.Leave, GetEffector(Action.Leave));
            return dictionary;
        }
    }
}