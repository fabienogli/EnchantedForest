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
                return "  -   ";
            }
            
            StringBuilder sb = new StringBuilder();
            // todo Irindul March 19, 2019 : Mapping for one Cloud = w etc...
            
            sb.Append(obj.HasFlag(Entity.Agent) ? "x" : " ");
            sb.Append(obj.HasFlag(Entity.Portal) ? "0" : " ");
            sb.Append(obj.HasFlag(Entity.Monster) ? "m" : " ");
            sb.Append(obj.HasFlag(Entity.Pit) ? "p" : " ");
            sb.Append(obj.HasFlag(Entity.Poop) ? "c" : " ");
            sb.Append(obj.HasFlag(Entity.Cloud) ? "w" : " ");
            
            return sb.ToString();
        }
    }
}