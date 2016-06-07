using System;
using System.Threading;
using RockSatC_2016.Drivers;
using RockSatC_2016.Work_Items;

namespace RockSatC_2016.Flight_Computer
{
    public class TimeSync
    {
        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataSize = 11;
        private readonly int _metaDataCount = 2;
        private readonly int _delay;


        private long _timeSinceLastRun = 0;

        public TimeSync(int delay = 30000)
        {
            _dataArray = new byte[_dataSize + _metaDataCount]; // start, type, size, size, hours, minutes, seconds, 8 bytes for millis()
            _dataArray[0] = (byte) PacketType.StartByte;
            _dataArray[1] = (byte) PacketType.TimeSync;
          
            _delay = delay;
            _workItem = new WorkItem(SyncTime, ref _dataArray, loggable:true, persistent:true);
        }

        private void SyncTime()
        {
           var time = RTC.CurrentTime();
            var millis = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            var dataIndex = _metaDataCount;

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];

            _dataArray[dataIndex++] = millis[0];
            _dataArray[dataIndex++] = millis[1];
            _dataArray[dataIndex++] = millis[2];
            _dataArray[dataIndex++] = millis[3];
            _dataArray[dataIndex++] = millis[4];
            _dataArray[dataIndex++] = millis[5];
            _dataArray[dataIndex++] = millis[6];
            _dataArray[dataIndex] = millis[7];

            Array.Copy(_dataArray,_workItem.PacketData,0);

            Thread.Sleep(_delay);
        }

        public void Run()
        {
            FlightComputer.Instance.Execute(_workItem);
        }
    }
}