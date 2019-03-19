using System;

namespace EnchantedForest.Environment
{
    public class PathNotFoundException : Exception
    {
        public PathNotFoundException()
        {
        }

        public PathNotFoundException(string message) : base(message)
        {
        }

        public PathNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}