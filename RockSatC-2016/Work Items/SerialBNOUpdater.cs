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
        private readonly int _dataCount = 9; //3 points of data, 2 bytes each
        private readonly int _metaDataCount = 2; //2 size, 1 start byte, 1 type byte
        private readonly int _timeDataCount = 3; //1 8 byte time stamp
        private readonly int _precision;
        private readonly int _delay;
        private long _timeSinceLastRun;

        public SerialBnoUpdater(int sigFigs = 4, int delay = 30000) {


            _bnoSensor = new SerialBno(SerialPorts.COM3,5000,5000,SerialBno.Bno055OpMode.OperationModeNdof);

            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount]; 
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.BnoDump;

            _delay = delay;
            _precision = (int)System.Math.Pow(10, sigFigs - 1);
            
            _workItem = new WorkItem(BnoUpdater, ref _dataArray, loggable:true, persistent:true, pauseable:true);

            _bnoSensor.Begin();
        }

        private void BnoUpdater()
        {
            
            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);

            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];
            
            var accelVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorAccelerometer);
            
            accelVec.X *= 100;
            accelVec.Y *= 100;
            accelVec.Z *= 100;

            _dataArray[dataIndex++] = (accelVec.X < 0 ? (byte) 1 : (byte) 0);
            accelVec.X = (float) System.Math.Abs(accelVec.X);

            _dataArray[dataIndex++] = (byte)(((short)accelVec.X >> 8) & 0xFF);
            _dataArray[dataIndex++] = (byte)((short)accelVec.X & 0xFF);


            _dataArray[dataIndex++] = (accelVec.Y < 0 ? (byte)1 : (byte)0);
            accelVec.Y = (float)System.Math.Abs(accelVec.Y);

            _dataArray[dataIndex++] = (byte)(((short)accelVec.Y >> 8) & 0xFF);
            _dataArray[dataIndex++] = (byte)((short)accelVec.Y & 0xFF);

            _dataArray[dataIndex++] = (accelVec.Z < 0 ? (byte)1 : (byte)0);
            accelVec.Z = (float)System.Math.Abs(accelVec.Z);

            _dataArray[dataIndex++] = (byte)(((short)accelVec.Z >> 8) & 0xFF);
            _dataArray[dataIndex] = (byte)((short)accelVec.Z & 0xFF);

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);

            Thread.Sleep(_delay);
        }

        public void Start() {
            _workItem.Start();
        }
        
    }
}