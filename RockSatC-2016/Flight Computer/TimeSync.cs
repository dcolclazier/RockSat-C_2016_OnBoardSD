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
        private readonly int _offset;
        private readonly int _metaDataCount = 4;
        private readonly int _delay;

        public TimeSync(int dataSize = 11, int delay = 30000)
        {
            _dataArray = new byte[dataSize + _metaDataCount]; // start, type, size, size, hours, minutes, seconds, 8 bytes for millis()
            _dataArray[0] = (byte) PacketType.StartByte;
            _dataArray[1] = (byte) PacketType.TimeSync;
            _dataArray[2] = (byte) ((dataSize >> 8) & 0xff);
            _dataArray[3] = (byte) (dataSize & 0xff);
            _offset = _metaDataCount;
            _delay = delay;
            _workItem = new WorkItem(SyncTime, ref _dataArray, loggable:true, persistent:true);
        }

        private void SyncTime()
        {
            var dataIndex = 0;
            var time = RTC.CurrentTime();
            var millis = BitConverter.GetBytes(Timer.Instance.ElapsedMilliseconds);

            _dataArray[dataIndex++ + _offset] = time[0];
            _dataArray[dataIndex++ + _offset] = time[1];
            _dataArray[dataIndex++ + _offset] = time[2];

            _dataArray[dataIndex++ + _offset] = millis[0];
            _dataArray[dataIndex++ + _offset] = millis[1];
            _dataArray[dataIndex++ + _offset] = millis[2];
            _dataArray[dataIndex++ + _offset] = millis[3];
            _dataArray[dataIndex++ + _offset] = millis[4];
            _dataArray[dataIndex++ + _offset] = millis[5];
            _dataArray[dataIndex++ + _offset] = millis[6];
            _dataArray[dataIndex + _offset] = millis[7];

            //time = RTC.CurrentTime();
            //_dataArray[dataIndex++ + _offset] = time[0];
            //_dataArray[dataIndex++ + _offset] = time[1];
            //_dataArray[dataIndex + _offset] = time[2];

            Array.Copy(_dataArray,_workItem.PacketData,0);
            //_workItem.PacketData = _dataArray;
            Thread.Sleep(_delay);
        }

        public void Run()
        {
            FlightComputer.Instance.Execute(_workItem);
        }
    }
}