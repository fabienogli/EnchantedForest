using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using EnchantedForest.Environment;
using EnchantementForest.Environment;

namespace EnchantedForest.View
{
    public class Viewer : IObserver<Forest>
    {
        private readonly Queue<Forest> epochs;
        private Forest currentEpoch;

        private bool Running { get; set; }

        private bool Display { get; }

        public Viewer(bool display)
        {
            Display = display;
            Running = true;
            epochs = new Queue<Forest>();
        }

        public void Run()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            TimeSpan n = watch.Elapsed;
            Render();
            while (Running)
            {
                TimeSpan t = watch.Elapsed;
                if (t.TotalMilliseconds - n.TotalMilliseconds > 800)
                {
                    //Reset timespans
                    n = t;
                    Render();
                }
            }
        }

        private void Render()
        {
            //For learning phase, we don't want to display, but we keep the Notify calls in case we want to have some logs
            if (!Display)
            {
                return;
            }

            Console.Clear();
            GetNextEpoch();
            RenderLegend();
            RenderMap();
            RenderPerf();
        }

        private void RenderPerf()
        {
            Console.WriteLine("Fitness : " + currentEpoch.Fitness);
        }

        private void GetNextEpoch()
        {
            if (epochs.Count > 0)
            {
                currentEpoch = epochs.Dequeue();
            }
        }

        private void RenderLegend()
        {
            string empty = EntityStringer.ObjectToString(Entity.Nothing).Trim();
            string agent = EntityStringer.ObjectToString(Entity.Agent).Trim();
            string monster = EntityStringer.ObjectToString(Entity.Monster).Trim();
            string poop = EntityStringer.ObjectToString(Entity.Poop).Trim();
            string cloud = EntityStringer.ObjectToString(Entity.Cloud).Trim();
            string portal = EntityStringer.ObjectToString(Entity.Portal).Trim();
            string pit = EntityStringer.ObjectToString(Entity.Pit).Trim();

            //todo add "Monster & agent etc"

            string legend =
                $"{empty}=empty {agent}=agent {portal}=portal {monster}=monster {poop}=poop {cloud}=cloud {pit}=pit";
            Console.WriteLine(legend);
        }

        private void RenderMap()
        {
            //Assuming the map is squared
            int size = (int) Math.Sqrt(currentEpoch.Map.Size);
            StringBuilder sb = new StringBuilder();
            for (int col = 0; col < size; col++)
            {
                for (int row = 0; row < size; row++)
                {
                    Entity obj = currentEpoch.Map.GetEntityAt(Convert2DTo1D(row, col));
                    string objectString = EntityStringer.ObjectToString(obj);
                    if (row == 0)
                    {
                        sb.Append("| ");
                    }

                    sb.Append(objectString)
                        .Append(" | ");
                }

                sb.Append(System.Environment.NewLine);
            }

            Console.Write(sb.ToString());
        }


        private int Convert2DTo1D(int col, int row)
        {
            var rowLength = currentEpoch.Map.SquaredSize;

            return row * rowLength + col;
        }

        public virtual void Subscribe(IObservable<Forest> observable)
        {
            observable.Subscribe(this);
            currentEpoch = (Forest) observable;
        }

        public void OnCompleted()
        {
            //End 
            Running = false;
        }

        public void OnError(Exception error)
        {
            /*
             * Does nothing
             */
        }

        public void OnNext(Forest mansion)
        {
            Forest copy = new Forest(mansion);
            epochs.Enqueue(copy);
        }
    }
}