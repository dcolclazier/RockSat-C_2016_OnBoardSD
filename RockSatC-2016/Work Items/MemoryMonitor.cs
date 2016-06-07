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
        private int _preLaunchCount;

        private MemoryMonitor(int preLaunchPauseCount = 25)
        {
            var unused = new byte[] {};
            _workItem = new WorkItem(MonitorMemory,ref unused, loggable:false, persistent:true, pauseable:false );
            _preLaunchCount = preLaunchPauseCount;
        }

        private void MonitorMemory()
        {
            if (Debug.GC(true) > 60000) return;

            Rebug.Print("RAM critically low... pausing actions.  Freemem: " + Debug.GC(true) + "  TimeStamp: " + Clock.Instance.ElapsedMilliseconds);

            foreach (WorkItem action in _pauseableWorkItems) action.Stop();
            //var currentCount = _logger.PendingItems;
            while (_logger.PendingItems > 0)
            {
                
            }

            Rebug.Print("Resuming paused actions... Current FreeMem: " + Debug.GC(false) + "  TimeStamp: " + Clock.Instance.ElapsedMilliseconds);

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