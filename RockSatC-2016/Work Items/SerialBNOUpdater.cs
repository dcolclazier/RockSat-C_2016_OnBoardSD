using System;
using System.Threading;
using RockSatC_2016.Drivers;
using RockSatC_2016.Flight_Computer;
using SecretLabs.NETMF.Hardware.Netduino;

namespace RockSatC_2016.Work_Items {
    public class SerialBnoUpdater {

        private readonly SerialBno _bnoSensor;

        //private BNOData _bnoData;

        private readonly WorkItem _workItem;
        private readonly byte[] _dataArray;
        private readonly int _dataCount = 9; //3 points of data, 3 bytes each
        private readonly int _metaDataCount = 2; //2 size, 1 start byte, 1 type byte
        private readonly int _timeDataCount = 3; //1 8 byte time stamp
        //private readonly int _precision;
        private readonly int _delay;

        public SerialBnoUpdater(int sigFigs = 4, int delay = 30000) {


            _bnoSensor = new SerialBno(SerialPorts.COM3,5000,5000,SerialBno.Bno055OpMode.OperationModeNdof);

            _dataArray = new byte[_dataCount + _metaDataCount + _timeDataCount]; 
            _dataArray[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _dataArray[1] = (byte)PacketType.BnoDump;

            _delay = delay;
            //_precision = (int)Math.Pow(10, sigFigs - 1);
            
            _workItem = new WorkItem(GyroUpdater, ref _dataArray, loggable:true, persistent:true, pauseable:true);

            _bnoSensor.Begin();
        }

        private void GyroUpdater()
        {
            Thread.Sleep(_delay);
            var dataIndex = _metaDataCount;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);
            _dataArray[dataIndex++] = time[0];
            _dataArray[dataIndex++] = time[1];
            _dataArray[dataIndex++] = time[2];
          
            var accelVec = _bnoSensor.read_vector(SerialBno.Bno055VectorType.VectorAccelerometer);
            accelVec.X *= 100;
            accelVec.Y *= 100;
            accelVec.Z *= 100;

            _dataArray[dataIndex++] = (accelVec.X < 0 ? (byte)1 : (byte)0);
            accelVec.X = (float)Math.Abs(accelVec.X);
            _dataArray[dataIndex++] = (byte)(((short)accelVec.X >> 8) & 0xFF);
            _dataArray[dataIndex++] = (byte)((short)accelVec.X & 0xFF);

            _dataArray[dataIndex++] = (accelVec.Y < 0 ? (byte)1 : (byte)0);
            accelVec.Y = (float)Math.Abs(accelVec.Y);
            _dataArray[dataIndex++] = (byte)(((short)accelVec.Y >> 8) & 0xFF);
            _dataArray[dataIndex++] = (byte)((short)accelVec.Y & 0xFF);

            _dataArray[dataIndex++] = (accelVec.Z < 0 ? (byte)1 : (byte)0);
            accelVec.Z = (float)Math.Abs(accelVec.Z);
            _dataArray[dataIndex++] = (byte)(((short)accelVec.Z >> 8) & 0xFF);
            _dataArray[dataIndex] = (byte)((short)accelVec.Z & 0xFF);

            //accelVec.X = (short) (accelVec.X*_precision);
            //accelVec.Y = (short) (accelVec.Y*_precision);
            //accelVec.Z = (short) (accelVec.Z*_precision);
            //-0.14 -->  -14  -->  byte(neg pos), byte(value msb), byte(
            //-0.54 -->  -54  --> 
            //+9.81 -->  +981

            Array.Copy(_dataArray, _workItem.PacketData, _dataArray.Length);

            
            
        }

        public void Start() {
            _workItem.Start();
        }
        
    }
}