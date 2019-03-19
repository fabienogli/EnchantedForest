using System;
using System.Linq;
using System.Text;

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
            if (obj.Equals(Entity.Nothing))
            {
                return "-";
            }
            
            StringBuilder sb = new StringBuilder();
            // todo Irindul March 19, 2019 : Mapping for one Cloud = w etc...
            
            sb.Append(obj.HasFlag(Entity.Agent) ? "x" : " ");
            sb.Append(obj.HasFlag(Entity.Monster) ? "m" : " ");
            sb.Append(obj.HasFlag(Entity.Pit) ? "p" : " ");
            sb.Append(obj.HasFlag(Entity.Poop) ? "c" : " ");
            sb.Append(obj.HasFlag(Entity.Cloud) ? "w" : " ");
            sb.Append(obj.HasFlag(Entity.Portal) ? "w" : " ");
            

            return sb.ToString();

            switch (obj)
            {
                case Entity.Nothing:
                    return "----";
                case Entity.Agent:
                    return "x";
                case Entity.Monster:
                    return "m";
                case Entity.Poop:
                    return "c";
                case Entity.Pit:
                    return "p";
                case Entity.Cloud:
                    return "w";
                case Entity.Portal:
                    return "0";
                case Entity.Cloud | Entity.Poop:
                    return "b";
                default:
                    return "-";
            }
        }

        public static string AddCharIfContains(Entity obj, Entity entity, string character)
        {
            return obj.HasFlag(entity) ? character : " ";
        }
    }
}