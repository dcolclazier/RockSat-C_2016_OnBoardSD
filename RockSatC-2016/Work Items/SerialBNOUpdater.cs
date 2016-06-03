using System.Threading;
using Microsoft.SPOT;
using RockSatC_2016.Abstract;
using RockSatC_2016.Drivers;
using RockSatC_2016.Event_Data;
using RockSatC_2016.Utility;
using SecretLabs.NETMF.Hardware.Netduino;
using Math = System.Math;

namespace RockSatC_2016.Work_Items {
    public class SerialBnoUpdater {

        private readonly SerialBNO _bnoSensor;

        //private BNOData _bnoData;

        private readonly WorkItem _workItem;
        private readonly byte[] _newData;
        private int _dataSize = 14; //7 points of data, 2 bytes each
        private int _metaDataCount = 10; // 6 bytes of time data, 2 size, 1 start byte, 1 type byte
        private int _offset = 4;
        private readonly int _precision;
        private int _delay;

        public SerialBnoUpdater(int sigFigs = 4, int delay = 100) {


            _bnoSensor = new SerialBNO(SerialPorts.COM4,5000,5000,SerialBNO.Bno055OpMode.Operation_Mode_Ndof);
            //_bnoData = new BNOData();

            _newData = new byte[_dataSize + _metaDataCount]; 
            _newData[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _newData[1] = (byte)PacketType.BnoDump;
            _newData[2] = (byte)((_dataSize >> 8) & 0xFF);
            _newData[3] = (byte)(_dataSize & 0xFF);
            _delay = delay;
            _precision = (int)Math.Pow(10, sigFigs - 1);
            //_precision = 1;
            //for (int i = 0; i < sigFigs-1; i++)
            //{
            //    _precision *= 10;
            //}


            _workItem = new WorkItem(GyroUpdater, ref _newData, loggable:true, pauseable:true, persistent:true);

            _bnoSensor.begin();
        }

        private void GyroUpdater()
        {

            var dataIndex = 0;

            var time = new byte[] {0,0,0};
            _newData[dataIndex++ + _offset] = time[0];
            _newData[dataIndex++ + _offset] = time[1];
            _newData[dataIndex++ + _offset] = time[2];

            var gyroVec = _bnoSensor.read_vector(SerialBNO.Bno055VectorType.Vector_Gyroscope);
            gyroVec.X = (short) (gyroVec.X*_precision);
            gyroVec.Y = (short) (gyroVec.Y*_precision);
            gyroVec.Z = (short) (gyroVec.Z*_precision);

            _newData[dataIndex++ + _offset] = (byte) (((short)gyroVec.X >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte) ((short)gyroVec.X & 0xFF);

            _newData[dataIndex++ + _offset] = (byte)(((short)gyroVec.Y >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)((short)gyroVec.Y & 0xFF);

            _newData[dataIndex++ + _offset] = (byte)(((short)gyroVec.Z >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)((short)gyroVec.Z & 0xFF);


            var accelVec = _bnoSensor.read_vector(SerialBNO.Bno055VectorType.Vector_Accelerometer);
            accelVec.X = (short) (accelVec.X*_precision);
            accelVec.Y = (short) (accelVec.Y*_precision);
            accelVec.Z = (short) (accelVec.Z*_precision);

            _newData[dataIndex++ + _offset] = (byte)(((short)accelVec.X >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)((short)accelVec.X & 0xFF);

            _newData[dataIndex++ + _offset] = (byte)(((short)accelVec.Y >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)((short)accelVec.Y & 0xFF);

            _newData[dataIndex++ + _offset] = (byte)(((short)accelVec.Z >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)((short)accelVec.Z & 0xFF);

            var temp = _bnoSensor.read_signed_byte(SerialBNO.Bno055Registers.Bno055_Temp_Addr);
            _newData[dataIndex++ + _offset] = (byte)((temp >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(temp & 0xFF);


            time = new byte[] { 0, 0, 0 };
            _newData[dataIndex++ + _offset] = time[0];
            _newData[dataIndex++ + _offset] = time[1];
            _newData[dataIndex + _offset] = time[2];

            //Debug.Print("Gyro - <" + _bnoData.gyro_x.ToString("F2") + ", "
            //            + _bnoData.gyro_y.ToString("F2") + ", "
            //            + _bnoData.gyro_z.ToString("F2") + ">\n" +
            //            "Accel - <" + _bnoData.accel_x.ToString("F2") + ", "
            //            + _bnoData.accel_y.ToString("F2") + ", "
            //            + _bnoData.accel_z.ToString("F2") + ">\n" +
            //            "Temp: " + _bnoData.temp);
            Thread.Sleep(_delay);
            Debug.Print("BNO Sensor update complete.");
        }

        public void Start() {
            _workItem.Start();
        }
        
    }
}