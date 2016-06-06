using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Work_Items;

namespace RockSatC_2016.Flight_Computer {
    public class Logger  {

        private readonly Queue _pendingData = new Queue();

        private readonly WorkItem _workItem;
        private readonly string _file;
        public int PendingItems => _pendingData.Count;

        public Logger()
        {
            for (int i = 0; i < 100; i++)
            {
                var prePath = @"\SD\";
                var postPath = "data" + i + ".dat";
                var fileTest = Path.Combine(Directory.GetCurrentDirectory(), prePath+postPath);

                if (File.Exists(fileTest)) continue;
                _file = fileTest;
                break;
            }
            //Debug.Print("Logger set up to use file: " + _file);
            //Debug.Print("Creating logger work item...");

            var unused = new byte[] {};
            _workItem = new WorkItem(LogWorker, ref unused, false, persistent: true);
           
        }
        
        private void LogWorker() {
            if (PendingItems == 0) return;
            
            var packet = (QueuePacket)_pendingData.Dequeue();
            using (var stream = new FileStream(_file, FileMode.Append))
            {
                stream.Write(packet.ArrayData, 0, packet.ArrayData.Length);

                var b = Encoding.UTF8.GetBytes("dummy data to force StreamWriter data to get written to SD");
                File.WriteAllBytes(@"\SD\SDdummy.txt", b);
            }
        }
        
        struct QueuePacket {
            public byte[] ArrayData { get; }

            public QueuePacket(byte[] arrayData) {
                ArrayData = arrayData;
            }
        }

        public void AddPacket(ref byte[] arrayData)
        {
            OnDataFound(true, ref arrayData);
        }

        private void OnDataFound(bool loggable, ref byte[] arrayData) {
            if (!loggable) return;
            if (arrayData.Length == 0) return;

            var thisArray = new byte[arrayData.Length];
            Array.Copy(arrayData,thisArray, thisArray.Length);
            _pendingData.Enqueue(new QueuePacket(thisArray));
        }

        public void Start() {
            FlightComputer.Instance.Execute(_workItem);
            FlightComputer.OnEventTriggered += OnDataFound;
        }

        public void Stop() {
            Debug.Print("Stopping logger...");
            FlightComputer.OnEventTriggered -= OnDataFound;
        }

        public void Dispose() {
            Debug.Print("Disposing of logger...");
            FlightComputer.OnEventTriggered -= OnDataFound;
        }

        
    }
}