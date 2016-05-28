using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using RockSatC_2016.Abstract;
using RockSatC_2016.Event_Data;
using RockSatC_2016.Flight_Computer;

namespace RockSatC_2016.Utility {
    public static class ThreadPool {


        private static readonly object Locker = new object();
        private static readonly ArrayList AvailableThreads = new ArrayList();
        private static readonly Queue ThreadActions = new Queue();
        private static readonly ManualResetEvent ThreadSynch = new ManualResetEvent(false);
        private static readonly FlightComputer FlightComputer = FlightComputer.Instance;
        private const int MaxThreads = 3;

        
        
        public static void QueueWorkItem(WorkItem workItem) {
          
                //queue the work action 
                lock (ThreadActions) {
                    ThreadActions.Enqueue(workItem);
                }
                //if we have less ThreadWorkers working than our MaxThreads, go ahead and spin one up.
                if (AvailableThreads.Count < MaxThreads) {
                    Debug.Print("Threadpool spooling up additional thread...");
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

                //nonsafe
                workItem.Action();
                if (workItem.EventType != EventType.None) FlightComputer.TriggerEvent(workItem.EventType, workItem.EventData, ref workItem.ArrayData);
                if (workItem.Persistent) QueueWorkItem(workItem);


                ////safe
                //try {
                //    //try to execute, then trigger any events, then re-add to queue if repeatable.
                //    workItem.Action();
                //    if (workItem.EventType != EventType.None) FlightComputer.TriggerEvent(workItem.EventType, workItem.EventData, ref workItem.ArrayData);
                //    if (workItem.Persistent) QueueWorkItem(workItem);
                //}
                //catch (Exception e) {
                //    Debug.Print("ThreadPool: Unhandled error executing action - " + e.Message + e.InnerException);
                //    Debug.Print("StackTrace: " + e.StackTrace);
                //    //maybe just reset the flight computer?
                //}
            }
        }
    }
}