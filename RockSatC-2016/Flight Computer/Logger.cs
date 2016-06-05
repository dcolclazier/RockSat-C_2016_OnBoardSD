using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.SPOT;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Utility;

namespace RockSatC_2016.Event_Listeners {
    public class Logger  {
        //private string _buffer = "";
        //private readonly int _maxBufferSize;
        private readonly Queue _pendingData = new Queue();
        //private readonly SerialPort _openLogger;

        private readonly WorkItem _workItem;
        private readonly string _file;
        public int PendingItems => _pendingData.Count;

        public Logger()
        {

            //NEW
            for (int i = 0; i < 100; i++)
            {
                var prePath = @"\SD\";
                var postPath = "data.dat" + i;
                var test = prePath + postPath;
                var dirTest = Directory.GetCurrentDirectory();
                var fileTest = Path.Combine(dirTest, test);

                if (File.Exists(fileTest)) continue;
                _file = fileTest;
                break;
            }
            

            //OLD
            //var dir = Directory.GetCurrentDirectory();
            //_file = Path.Combine(dir, @"\SD\data.dat");
            //if (File.Exists(_file)) Debug.Print("File exists..");
            //else
            //{
            //    Debug.Print("WTF");
            //}



            Debug.Print("Logger init in _file: " + _file);
            

            //_maxBufferSize = maxBuffer;
            Debug.Print("Initializing serial port...");
            //_openLogger = new SerialPort(comPort, baud, Parity.None, 8, StopBits.One);
            Debug.Print("Serial port initialized... opening serial port.");
            //_openLogger.Open();
            Debug.Print("Serial port opened.");

            Debug.Print("Creating logger thread and adding to pool...");
            var unused = new byte[] {};
            _workItem = new WorkItem(LogWorker, ref unused, false, persistent: true);
           
        }
        
        private void LogWorker() {
            if (_pendingData.Count == 0) return;
            
            //Debug.Print("_dataArray found to be written...");
            var packet = (QueuePacket)_pendingData.Dequeue();
            //File.WriteAllBytes(_file,packet.ArrayData);
            using (var stream = new FileStream(_file, FileMode.Append))
            {
                stream.Write(packet.ArrayData, 0, packet.ArrayData.Length);
                //Debug.Print("File size:" + stream.Length);


                var b = Encoding.UTF8.GetBytes("dummy data to force StreamWriter data to get written to SD");
                File.WriteAllBytes(@"\SD\SDdummy.txt", b);
            }

            if(_pendingData.Count < 10) return;

            //if(_pendingData.Count % 10 == 0) Debug.Print("Queue After running logworker:  " + _pendingData.Count + ", FreeMem: " + Debug.GC(true));
        }
        
        struct QueuePacket {
            //public EventType Name { get; }
            //public IEventData EventData { get; }
            public byte[] ArrayData { get; }

            public QueuePacket(byte[] arrayData) {
                //Name = eventName;
                //EventData = eventData;
                ArrayData = arrayData;
            }
        }

        private void OnDataFound(bool loggable, ref byte[] arrayData) {
            if (!loggable) return;
            if (arrayData.Length == 0) return;

            var thisArray = new byte[arrayData.Length];
            Array.Copy(arrayData,thisArray, thisArray.Length);

            //var thisarray = arrayData;
            //var count = arrayData.Length;
            //arrayData = new byte[count];
            //Debug.Print("Adding to queue... new count: " + _pendingData.Count);
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