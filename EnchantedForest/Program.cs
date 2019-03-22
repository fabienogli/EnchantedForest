using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using EnchantedForest.Agent;
using EnchantedForest.Environment;
using EnchantedForest.View;

namespace EnchantedForest
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Run();
        }

        private static void Run()
        {
            var forest = new Forest(3 * 3);
            var viewer = new Viewer(true);
            var agent = new Agent.Agent(forest);
            viewer.Subscribe(forest);

            ThreadStart viewStarter = viewer.Run;
            var viewThread = new Thread(viewStarter);
            viewThread.Start();

            ThreadStart envStarter = forest.Run;
            var envThread = new Thread(envStarter);
            envThread.Start();

            ThreadStart agentStarter = agent.Run;
            var agentThread = new Thread(agentStarter);
            agentThread.Start();
        }
    }
}