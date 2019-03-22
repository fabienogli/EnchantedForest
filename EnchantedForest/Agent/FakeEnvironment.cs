using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnchantedForest.Environment;
using EnchantedForest.Search;

namespace EnchantedForest.Agent
{
    public class FakeEnvironment : IEnvironment
    {
        private Forest Forest { get; set; }
        private ProbabilityMatrix Proba { get; set; } 
        private HashSet<int> AlreadyVisited { get; } 
        private HashSet<int> Unknown { get; }

        public FakeEnvironment(Forest forest, ProbabilityMatrix proba)
        {
            Forest = forest;
            Proba = proba;
            AlreadyVisited = new HashSet<int>();
            Unknown = new HashSet<int>();
            for (int i = 0; i < Forest.Map.Size; i++)
            {
                Unknown.Add(i);
            }
        }

        public void Visit(int cell, ProbabilityMatrix proba)
        {
            Unknown.Remove(cell);
            AlreadyVisited.Add(cell);
            Proba = proba;
        }

        public int GetCostForAction(Action action)
        {
            return Forest.GetCostForAction(action);
        }

        public List<State> GetSuccessors(State currentState)
        {
            var cell = currentState.Map.AgentPos;
            if (Unknown.Contains(cell))
            {
                return new List<State>();
            }

            var states = new List<State>();

            switch (currentState.Action)
            {
                case Action.ThrowUp:
                case Action.ThrowRight:
                case Action.ThrowDown:
                case Action.ThrowLeft:
                    var target = GetTarget(currentState);
                    if (AlreadyVisited.Contains(target))
                    {
                        return states;
                    }
                    var move = GetCorrespondingMove(currentState.Action);
                    var forest = new Forest(currentState.Forest);
                    forest.HandleAction(move);
                    var state = new State(forest.Map, move, forest);
                    states.Add(state);
                    return states;
            }

            var surroundings = currentState.Map.GetSurroundingCells(cell).ToList();
           
            foreach (var surrounding in surroundings)
            {
                var moved = currentState.Map.MoveToward(cell, surrounding);
                var thrown = currentState.Map.ThrowToward(cell, surrounding);
                
                var forest = new Forest(currentState.Forest);
                forest.HandleAction(moved);
                var state = new State(forest.Map, moved, forest);
                states.Add(state);


               
                forest = new Forest(currentState.Forest);
                forest.HandleAction(thrown);
                state = new State(forest.Map, thrown, forest);
                var target = GetTarget(state);

                if (AlreadyVisited.Contains(target))
                {
                    continue;
                }
                
                states.Add(state);
            }

            return states;
        }

        private Action GetCorrespondingMove(Action action)
        {
            switch (action)
            {
                case Action.ThrowUp:
                    return Action.Up;
                case Action.ThrowDown:
                    return Action.Down;
                case Action.ThrowLeft:
                    return Action.Left;
                case Action.ThrowRight:
                    return Action.Right;
                default:
                    throw new InvalidDataException("Action must be of throw");
            }
        }

        public double GetHeuristicForState(State state)
        {
            var agentPos = state.Map.AgentPos;
            return 1 - Proba.GetProbaFor(agentPos, Entity.Portal);
        }

        private int GetTarget(State current)
        {
            switch (current.Action)
            {
                case Action.ThrowUp:
                    return current.Map.GetUpFrom(current.Map.AgentPos);
                case Action.ThrowDown:
                    return current.Map.GetDownFrom(current.Map.AgentPos);
                case Action.ThrowLeft:
                    return current.Map.GetLeftFrom(current.Map.AgentPos);
                case Action.ThrowRight:
                    return current.Map.GetRightFrom(current.Map.AgentPos);
            }
            
            throw new InvalidDataException("Cannot get target of other than throw : " + current.Action );
        }
    }
}