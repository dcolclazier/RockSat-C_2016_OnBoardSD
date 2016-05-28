using Microsoft.SPOT;
using RockSatC_2016.Abstract;
using RockSatC_2016.Drivers;
using RockSatC_2016.Event_Data;
using RockSatC_2016.Utility;
using SecretLabs.NETMF.Hardware.Netduino;
using Math = System.Math;

namespace RockSatC_2016.Work_Items {
    public class SerialBNOUpdater {

        private readonly SerialBNO _bnoSensor;

        private BNOData _bnoData;

        private readonly WorkItem _workItem;
        private readonly byte[] _newData;
        private int _dataSize = 14; //7 points of data, 2 bytes each
        private int _metaDataCount = 10; // 6 bytes of time data, 2 size, 1 start byte, 1 type byte
        private int _offset = 4;
        private readonly int _precision;

        public SerialBNOUpdater(int sigFigs = 4) {
            _bnoSensor = new SerialBNO(SerialPorts.COM4,5000,5000,SerialBNO.Bno055OpMode.Operation_Mode_Ndof);
            _bnoData = new BNOData();


            _newData = new byte[_dataSize + _metaDataCount]; 
            _newData[0] = (byte)PacketType.StartByte; // start bit = 0xff
            _newData[1] = (byte)PacketType.BNODump;
            _newData[2] = (byte)((_dataSize >> 8) & 0xFF);
            _newData[3] = (byte)(_dataSize & 0xFF);

            _precision = (int)Math.Pow(10, sigFigs - 1);
            //_precision = 1;
            //for (int i = 0; i < sigFigs-1; i++)
            //{
            //    _precision *= 10;
            //}


            _workItem = new WorkItem(GyroUpdater, ref _newData, EventType.BNOUpdate, _bnoData, true, true);

            _bnoSensor.begin();
        }

        private void GyroUpdater()
        {

            var dataIndex = 0;

            var time = RTC.CurrentTime();
            _newData[dataIndex++ + _offset] = time[0];
            _newData[dataIndex++ + _offset] = time[1];
            _newData[dataIndex++ + _offset] = time[2];

            var gyro_vec = _bnoSensor.read_vector(SerialBNO.Bno055VectorType.Vector_Gyroscope);

            _bnoData.gyro_x = (short)(gyro_vec.X * _precision);
            _newData[dataIndex++ + _offset] = (byte) ((_bnoData.gyro_x >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte) (_bnoData.gyro_x & 0xFF);

            _bnoData.gyro_y = (short)(gyro_vec.Y * _precision);
            _newData[dataIndex++ + _offset] = (byte)((_bnoData.gyro_y >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(_bnoData.gyro_y & 0xFF);

            _bnoData.gyro_z = (short)(gyro_vec.Z * _precision);
            _newData[dataIndex++ + _offset] = (byte)((_bnoData.gyro_z >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(_bnoData.gyro_z & 0xFF);

            var accel_vec = _bnoSensor.read_vector(SerialBNO.Bno055VectorType.Vector_Accelerometer);

            _bnoData.accel_x = (short)(accel_vec.X * _precision);
            _newData[dataIndex++ + _offset] = (byte)((_bnoData.accel_x >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(_bnoData.accel_x & 0xFF);

            _bnoData.accel_y = (short)(accel_vec.Y * _precision);
            _newData[dataIndex++ + _offset] = (byte)((_bnoData.accel_y >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(_bnoData.accel_y & 0xFF);

            _bnoData.accel_z = (short)(accel_vec.Z * _precision);
            _newData[dataIndex++ + _offset] = (byte)((_bnoData.accel_z >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(_bnoData.accel_z & 0xFF);

            _bnoData.temp = _bnoSensor.read_signed_byte(SerialBNO.Bno055Registers.Bno055_Temp_Addr);
            _newData[dataIndex++ + _offset] = (byte)((_bnoData.temp >> 8) & 0xFF);
            _newData[dataIndex++ + _offset] = (byte)(_bnoData.temp & 0xFF);


            time = RTC.CurrentTime();
            _newData[dataIndex++ + _offset] = time[0];
            _newData[dataIndex++ + _offset] = time[1];
            _newData[dataIndex++ + _offset] = time[2];

            //Debug.Print("Gyro - <" + _bnoData.gyro_x.ToString("F2") + ", "
            //            + _bnoData.gyro_y.ToString("F2") + ", "
            //            + _bnoData.gyro_z.ToString("F2") + ">\n" +
            //            "Accel - <" + _bnoData.accel_x.ToString("F2") + ", "
            //            + _bnoData.accel_y.ToString("F2") + ", "
            //            + _bnoData.accel_z.ToString("F2") + ">\n" +
            //            "Temp: " + _bnoData.temp);
            Debug.Print("BNO Sensor update complete.");
        }

        public void Start() {
            _workItem.Start();
        }
        
    }
}