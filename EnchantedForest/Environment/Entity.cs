using System;

namespace EnchantedForest.Environment
{
    [Flags]
    public enum Entity
    {
        Nothing = 1 << 0,
        Agent = 1 << 1,
        Monster = 1 << 2,
        Poop = 1 << 3,
        Pit = 1 << 4,
        Cloud = 1 << 5,
        Portal = 1 << 6
    }
    
    public static class EntityStringer
    {
        public static string ObjectToString(Entity obj)
        {
            switch (Entity.Nothing | obj)
            {
                case Entity.Nothing:
                    return "-";
                case Entity.Nothing | Entity.Agent:
                    return "x";
                case Entity.Nothing | Entity.Monster:
                    return "m";
                case Entity.Nothing | Entity.Poop:
                    return "c";
                case Entity.Nothing | Entity.Pit:
                    return "p";
                case Entity.Nothing | Entity.Cloud:
                    return "w";
                case Entity.Nothing | Entity.Portal:
                    return "0";
                default:
                    return "-";
            }
        }
    }
}