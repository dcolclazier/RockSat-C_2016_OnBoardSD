using System;
using System.Threading;
using Microsoft.SPOT;
using RockSatC_2016.Drivers;
using RockSatC_2016.Flight_Computer;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RockSatC_2016.Work_Items {
    public class SerialBnoUpdater {

        private readonly SerialBno _bnoSensor;

        //private BNOData _bnoData;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 6; //3 points of data, 2 bytes each
        private readonly int _metaDataCount = 4; //2 size, 1 start byte, 1 type byte
        private readonly int _timeDataCount = 3; //1 8 byte time stamp
        private readonly int _precision;
        private readonly int _delay;

        public SerialBnoUpdater(int sigFigs = 4, int delay = 30000) {


            _bnoSensor = new SerialBno(SerialPorts.COM3,5000,5000,SerialBno.Bno055OpMode.OperationModeNdof);

            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount]; 
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.BnoDump;

            var dataSize = _dataCount + _timeDataCount;
            _dataArray[2] = (byte)((dataSize >> 8) & 0xFF);
            _dataArray[3] = (byte)(dataSize & 0xFF);
            _delay = delay;
            _precision = (int)System.Math.Pow(10, sigFigs - 1);
            
            _workItem = new WorkItem(BnoUpdater, ref _dataArray, loggable:true, persistent:true, pauseable:true);

            _bnoSensor.Begin();
        }

        private void BnoUpdater()
        {
            Thread.Sleep(_delay);
            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);
            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];
            
            var accelVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorAccelerometer);
            accelVec.X = (short) (accelVec.X*_precision);
            accelVec.Y = (short) (accelVec.Y*_precision);
            accelVec.Z = (short) (accelVec.Z*_precision);

            _dataArray[dataIndex++] = (byte)(((short)accelVec.X >> 8) & 0xFF);
            _dataArray[dataIndex++] = (byte)((short)accelVec.X & 0xFF);

            _dataArray[dataIndex++] = (byte)(((short)accelVec.Y >> 8) & 0xFF);
            _dataArray[dataIndex++] = (byte)((short)accelVec.Y & 0xFF);

            _dataArray[dataIndex++] = (byte)(((short)accelVec.Z >> 8) & 0xFF);
            _dataArray[dataIndex] = (byte)((short)accelVec.Z & 0xFF);

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);

        }

        public void Start() {
            _workItem.Start();
        }
        
    }
}