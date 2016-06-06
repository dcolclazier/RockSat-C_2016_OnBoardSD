using System;
using System.Collections;
using System.Threading;
using RockSatC_2016.Drivers;
using RockSatC_2016.Work_Items;

namespace RockSatC_2016.Flight_Computer {
    public static class ThreadPool {


        private static readonly object Locker = new object();
        private static readonly ArrayList AvailableThreads = new ArrayList();
        private static readonly Queue ThreadActions = new Queue();
        private static readonly ManualResetEvent ThreadSynch = new ManualResetEvent(false);
        private static readonly FlightComputer FlightComputer = FlightComputer.Instance;
        private const int MaxThreads = 4;

        
        
        public static void QueueWorkItem(WorkItem workItem) {
          
                //queue the work action 
                lock (ThreadActions) {
                    ThreadActions.Enqueue(workItem);
                }
                //if we have less ThreadWorkers working than our MaxThreads, go ahead and spin one up.
                if (AvailableThreads.Count < MaxThreads) {
                    //Debug.Print("Threadpool spooling up additional thread...");
                    var thread = new Thread(ThreadWorker);
                    AvailableThreads.Add(thread);
                    thread.Start();
                }
                //pulse all ThreadWorkers
                lock (Locker) {
                    ThreadSynch.Set();
                }
        }

        private static void ThreadWorker() {

            while (true) {
                //Wait for pulse from ThreadPool, signifying a new work item has been queued
                // ReSharper disable once InconsistentlySynchronizedField
                ThreadSynch.WaitOne();

                var workItem = new WorkItem();
                lock (ThreadActions) {
                    //pull the next work item off of the queue
                    if (ThreadActions.Count > 0) workItem = ThreadActions.Dequeue() as WorkItem;
                    //no pending actions - reset threads so they wait for next pulse.
                    else lock (ThreadSynch) 
                            ThreadSynch.Reset();
                }

                //if no action, go back to waiting.
                if (workItem?.Action == null) continue;

                //unsafe
                workItem.Action();
                FlightComputer.TriggerEvent(workItem.Loggable, ref workItem.PacketData);
                if (workItem.Persistent) QueueWorkItem(workItem);


                //Debug.Print("Current Thread Queue count: " + ThreadActions.Count);
                //safe - make sure to enable for flight.
                //try
                //{
                //    //try to execute, then trigger any events, then re-add to queue if repeatable.
                //    workItem.Action();
                //    FlightComputer.TriggerEvent(workItem.Loggable, ref workItem.PacketData);
                //    if (workItem.Persistent) QueueWorkItem(workItem);
                //}
                //catch (Exception e)
                //{
                //    Debug.Print("ThreadPool: Unhandled error executing action - " + e.Message + e.InnerException);
                //    Debug.Print("StackTrace: " + e.StackTrace);
                //    //bug -  maybe just reset the flight computer?
                //}
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}