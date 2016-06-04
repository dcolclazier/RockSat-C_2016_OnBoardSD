
using System;
using System.Collections;
using System.IO;
using System.IO.Ports;
using System.Text;
using Microsoft.SPOT;
using RockSatC_2016.Abstract;
using RockSatC_2016.Event_Data;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Utility;

namespace RockSatC_2016.Event_Listeners {
    public class Logger  {
        //private string _buffer = "";
        //private readonly int _maxBufferSize;
        private readonly Queue _pendingData = new Queue();
        //private readonly SerialPort _openLogger;

        private readonly StreamWriter streamWriter;
        private readonly WorkItem _workItem;
        private string _file;
        public int PendingItems => _pendingData.Count;

        public Logger()
        {

            var dir = Directory.GetCurrentDirectory();
            _file = Path.Combine(dir, @"\SD\data.dat");
            if (File.Exists(_file)) Debug.Print("File exists..");
            else
            {
                Debug.Print("WTF");
            }



            Debug.Print("Logger init in _file: " + _file);
            

            //_maxBufferSize = maxBuffer;
            Debug.Print("Initializing serial port...");
            //_openLogger = new SerialPort(comPort, baud, Parity.None, 8, StopBits.One);
            Debug.Print("Serial port initialized... opening serial port.");
            //_openLogger.Open();
            Debug.Print("Serial port opened.");

            Debug.Print("Creating logger thread and adding to pool...");
            var unused = new byte[] {};
            _workItem = new WorkItem(LogWorker, ref unused, false, persistent: true, pauseable: false);
           
        }
        
        private void LogWorker() {
            if (_pendingData.Count == 0) return;

            
            //Debug.Print("Data found to be written...");
            var packet = (QueuePacket)_pendingData.Dequeue();
            //File.WriteAllBytes(_file,packet.ArrayData);
            using (var stream = new FileStream(_file, FileMode.Append))
            {
                stream.Write(packet.ArrayData, 0, packet.ArrayData.Length);
                //Debug.Print("File size:" + stream.Length);


                var b = System.Text.Encoding.UTF8.GetBytes("dummy data to force StreamWriter data to get written to SD");
                File.WriteAllBytes(@"\SD\SDdummy.txt", b);
            }

            //_openLogger.Write(packet.ArrayData, 0, packet.ArrayData.Length);

            //var logEntry = "";
            //    switch (packet.Name) {
            //        case EventType.BNOUpdate:
            //            var data = packet.EventData as BNOData;
            //            logEntry = "T:" + data.temp + ";" + "A:" + data.accel_x + "," + data.accel_y + "," +
            //                           data.accel_z + ";" + "G:" + data.gyro_x + "," + data.gyro_y + "," + data.gyro_z + ";";
            //            break;
            //        case EventType.GeigerUpdate:
            //            var geigerData = packet.EventData as GeigerData;
            //            logEntry = "R:" + geigerData.shielded_geigerCount + "," + geigerData.unshielded_geigerCount + ";";
            //            break;
            //        case EventType.AccelDump:
            //            var entry = Encoding.UTF8.GetBytes("AD:");
            //            _openLogger.Write(entry,0,entry.Length);
            //            _openLogger.Write(packet.packetData,0,packet.packetData.Length);
            //            Debug.Print("Flushed accel byte array directly to SD card...");
            //            return;
            //        case EventType.None:
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException(nameof(packet.Name),"Event Type not handled by logger... ");
            //    }
            //    if (_buffer.Length + logEntry.Length > _maxBufferSize) {
            //        var data = System.Text.Encoding.UTF8.GetBytes(_buffer);
            //        _openLogger.Write(data, 0, data.Length);
            //        Debug.Print("Buffer flushed to SD Card - clearing..." + Debug.GC(true));
            //        _buffer = "";
            //    }
            //    _buffer += logEntry;
            Debug.Print("Queue After running logworker:  " + _pendingData.Count + ", FreeMem: " + Debug.GC(true));
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

            var thisarray = arrayData;
            var count = arrayData.Length;
            arrayData = new byte[count];
            //Debug.Print("Adding to queue... new count: " + _pendingData.Count);
            _pendingData.Enqueue(new QueuePacket(thisarray));
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