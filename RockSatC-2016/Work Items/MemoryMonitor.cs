using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using RockSatC_2016.Flight_Computer;

namespace RockSatC_2016.Work_Items
{
    public class MemoryMonitor 
    {
        private static MemoryMonitor _instance;
        public static MemoryMonitor Instance => _instance ?? (_instance = new MemoryMonitor());

        private readonly WorkItem _workItem;
        private readonly ArrayList _pauseableWorkItems = new ArrayList();
        private Logger _logger;

        private MemoryMonitor()
        {
            var unused = new byte[] {};
            _workItem = new WorkItem(MonitorMemory,ref unused, loggable:false, persistent:true, pauseable:false );
            
        }

        private void MonitorMemory()
        {
            if (!FlightComputer.Launched && _logger.PendingItems > 25)
                    PauseAction();

            if (Debug.GC(true) > 60000) return;

            PauseAction();

        }

        private void PauseAction()
        {
            Debug.Print("Pausing actions to allow logger to catch up... " + Debug.GC(false));

            foreach (WorkItem action in _pauseableWorkItems) action.Stop();
            while (_logger.PendingItems > 0)
            {
                Debug.Print("Current item count: " + _logger.PendingItems);
                Thread.Sleep(5);
            };

            Debug.Print("Resuming paused actions... Current FreeMem: " + Debug.GC(true));

            foreach (WorkItem action in _pauseableWorkItems) action.Start();
        }

        public void RegisterPauseableAction(WorkItem actionToRegister) {
            _pauseableWorkItems.Add(actionToRegister);
        }

        public void Start(ref Logger logger)
        {
            _logger = logger;
            _workItem.Start();
            
        }
    }
}