using System;
using System.Diagnostics;
using RockSatC_2016.Flight_Computer;
using RockSatC_2016.Work_Items;

namespace RockSatC_2016
{
    public class TimeSync
    {
        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _offset;

        public TimeSync(int dataSize = 11)
        {
            _dataArray = new byte[18]; // start, type, size, size, hours, minutes, seconds, 8 bytes for millis()
            _dataArray[0] = (byte) PacketType.StartByte;
            _dataArray[1] = (byte) PacketType.TimeSync;
            _dataArray[2] = (byte) ((dataSize >> 8) & 0xff);
            _dataArray[3] = (byte) (dataSize & 0xff);
            _offset = 4;

            _workItem = new WorkItem(SyncTime, ref _dataArray, loggable:true);
        }

        private void SyncTime()
        {
            var dataIndex = 0;
            var time = RTC.CurrentTime();
            var millis = BitConverter.GetBytes(Stopwatch.Instance.ElapsedMilliseconds);

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
        }

        public void RunOnce()
        {
            FlightComputer.Instance.Execute(_workItem);
        }
    }
}