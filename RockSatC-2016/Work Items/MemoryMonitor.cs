using System.Collections;
using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Flight_Computer;
using Debug = RockSatC_2016.Drivers.Rebug;

namespace RockSatC_2016.Work_Items
{
    public class MemoryMonitor 
    {
        private static MemoryMonitor _instance;
        public static MemoryMonitor Instance => _instance ?? (_instance = new MemoryMonitor());
        public static WorkItem CleanupMemory { get; private set; }

        private readonly WorkItem _workItem;
        private readonly ArrayList _pauseableWorkItems = new ArrayList();
        private Logger _logger;
        private readonly int _preLaunchCount;

        private MemoryMonitor(int preLaunchPauseCount = 10)
        {
            var unused = new byte[] {};
            _workItem = new WorkItem(MonitorMemory,ref unused, loggable:false, persistent:true, pauseable:false );
            CleanupMemory = new WorkItem(PauseActions, ref unused,false,false,false);
            _preLaunchCount = preLaunchPauseCount;
        }

        private void MonitorMemory()
        {
            if ((!FlightComputer.Launched || Clock.Instance.ElapsedMilliseconds < 60000) && _logger.PendingItems > _preLaunchCount)
                FlightComputer.Instance.Execute(CleanupMemory);

            if (Microsoft.SPOT.Debug.GC(true) > 60000) return;

            FlightComputer.Instance.Execute(CleanupMemory);

        }

        public void PauseActions()
        {
            Debug.Print("RAM critically low... pausing actions.  Freemem: " + Microsoft.SPOT.Debug.GC(true) + "  TimeStamp: " + Clock.Instance.ElapsedMilliseconds);
            foreach (WorkItem action in _pauseableWorkItems) action.Stop();
            while (_logger.PendingItems > 0)
            {
                
            }

            Debug.Print("Resuming paused actions... Current FreeMem: " + Microsoft.SPOT.Debug.GC(false) + "  TimeStamp: " + Clock.Instance.ElapsedMilliseconds);

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