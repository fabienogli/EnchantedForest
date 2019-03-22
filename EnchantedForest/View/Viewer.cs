using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using EnchantedForest.Environment;

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
                if (t.TotalMilliseconds - n.TotalMilliseconds > 600)
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

            if (!GetNextEpoch()) 
                return;
            
            Console.Clear();
            RenderLegend();
            RenderMap();
            RenderPerf();
        }

        private void RenderPerf()
        {
            Console.WriteLine("Fitness : " + currentEpoch.Fitness);
        }

        private bool GetNextEpoch()
        {
            if (epochs.Count <= 0) 
                return false;
            
            currentEpoch = epochs.Dequeue();
            return true;
        }

        private static void RenderLegend()
        {
            var empty = EntityStringer.ObjectToString(Entity.Nothing).Trim();
            var agent = EntityStringer.ObjectToString(Entity.Agent).Trim();
            var monster = EntityStringer.ObjectToString(Entity.Monster).Trim();
            var poop = EntityStringer.ObjectToString(Entity.Poop).Trim();
            var cloud = EntityStringer.ObjectToString(Entity.Cloud).Trim();
            var portal = EntityStringer.ObjectToString(Entity.Portal).Trim();
            var pit = EntityStringer.ObjectToString(Entity.Pit).Trim();

            var legend =
                $"{empty}=empty {agent}=agent {portal}=portal {monster}=monster {poop}=poop {cloud}=cloud {pit}=pit";
            Console.WriteLine(legend);
        }

        private void RenderMap()
        {
            //Assuming the map is squared
            var size = (int) Math.Sqrt(currentEpoch.Map.Size);
            var sb = new StringBuilder();
            for (var col = 0; col < size; col++)
            {
                for (var row = 0; row < size; row++)
                {
                    var obj = currentEpoch.Map.GetEntityAt(Convert2DTo1D(row, col));
                    var objectString = EntityStringer.ObjectToString(obj);
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