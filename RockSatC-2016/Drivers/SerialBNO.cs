using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using RockSatC_2016.Utility;

namespace RockSatC_2016.Drivers {
    
    public class SerialBno {

        public SerialBno(string comPort, int readTimeout, int writeTimeout, Bno055OpMode mode = Bno055OpMode.OperationModeAccgyro) {
            _mode = mode;
            _comPort = new SerialPort(comPort, Baud,0,8,StopBits.One) {
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout
            };
        }

        public bool Begin()
        {
            Debug.Print("Opening BNO communications channel...");
            _comPort.Open();

            ConfigMode();
            write_byte(Bno055Registers.Bno055PageIdAddr, 0);

            var bnoId = read_byte(Bno055Registers.Bno055ChipIdAddr);
            Debug.Print("Read chip ID for BNO055: 0x" + bnoId.ToString("x"));

            if (bnoId != Bno055Id) return false;

            //reset the device
            write_byte(Bno055Registers.Bno055SysTriggerAddr, 0x20, false);

            //sleep 650 ms after reset for chip to be ready(as suggested in datasheet)
            Thread.Sleep(650);

            //set to normal power mode
            write_byte(Bno055Registers.Bno055PwrModeAddr, (byte)Bno055PowerMode.PowerModeNormal);

            //default to external oscillator
            write_byte(Bno055Registers.Bno055SysTriggerAddr, 0x80);

            //enter normal operation mode
            OpMode();

            return true;
        }

        public Vector read_vector(Bno055VectorType vec, int count = 3) {
            var data = read_bytes((Bno055Registers)vec, count*2);
            var rawResult = new int[count];
            for (int i = 0; i < count; i++) {
                rawResult[i] = ((data[i*2 + 1] << 8) | data[i*2]);
                if (rawResult[i] > 32767) rawResult[i] -= 65536;
            }
            var modifier = 100.0f;
            // ReSharper disable once SwitchStatementMissingSomeCases - missing cases = 100.0f
            switch (vec) {
                case Bno055VectorType.VectorMagnetometer:
                case Bno055VectorType.VectorEuler:
                    modifier = 16.0f;
                    break;
                case Bno055VectorType.VectorGyroscope:
                    modifier = 900.0f;
                    break;
            }
            return new Vector()
            {
                X = rawResult[0] / modifier,
                Y = rawResult[1] / modifier,
                Z = rawResult[2] / modifier
            };

        }

        private byte[] serial_send(byte[] command, int expectedLength, bool ack = true, int maxTries = 10, int tries = 0) {

            _comPort.Flush();
            _comPort.Write(command, 0, command.Length);
            
            //If no ack needed, we're done.
            if (!ack) return null;
            
            //wait for serial stream to fill with expected ack data
            Thread.Sleep(65); //bug THIS THIS SLOW... should wait until correct amount of data is ready to be read.
            
            var response = new byte[_comPort.BytesToRead];
            var readCount = _comPort.Read(response, 0, response.Length);
            //If we timed out, throw an exception.
            if (readCount == 0) Debug.Print("Serial ACK timeout...");

            //if we didn't get an error code (0xEE07), we're done.
            if (response.Length > 0 && !(response[0] == 0xEE && response[1] == 0x07)) return response;
            //if we tried 5 times to get a non-error ACK and didn't, throw an exception.
            if (++tries == maxTries)
                throw new IOException("Exceeded max tries to acknowlege serial command without bus error.");


            return serial_send(command, expectedLength, maxTries:maxTries, tries:tries);
        }

        private void write_byte(Bno055Registers reg, byte data, bool ack = true) {

            write_bytes(reg,new []{data},ack);
        }

        private void write_bytes(Bno055Registers reg, byte[] data, bool ack = true) {
            var command = new byte[4 + data.Length];
            command[0] = 0xAA; //start byte
            command[1] = 0x00; //0x00: Write, 0x01: Read
            command[2] = (byte) reg; // register address
            command[3] = (byte) data.Length; //Length
            for (var i = 0; i < data.Length; i++) {
                command[i + 4] = data[i];
            }

            var response = serial_send(command, 2, ack);

            if (ack) Verify(response);
        }

        private void Verify(byte[] response) {
            if (response[0] != 0xEE && response[1] != 0x01)
                throw new IOException("Error writing to register: 0x" + response);
        }

        private string Bytearraytostring(byte[] array) {
            var hexString = new StringBuilder();
            for (var i = 0; i < array.Length; i++) {
                hexString.Append(array[i].ToString("x"));
            }
            return hexString.ToString();
        }

        private byte read_byte(Bno055Registers reg) {
            return read_bytes(reg, 1)[0];
        }

        public byte read_signed_byte(Bno055Registers reg) {
            var data = read_byte(reg);
            return (data > 127) ? (byte) (data - 256) : data;
        }

        private byte[] read_bytes(Bno055Registers reg, int expectedLength, bool ack = true)
        {
            //build read command
            var command = new byte[4];
            command[0] = 0xAA; //start byte
            command[1] = 0x01; //0x00: Write, 0x01: Read
            command[2] = (byte)reg; // register address
            command[3] = (byte)expectedLength; //Length

            //send read command and get ack
            var response = serial_send(command, expectedLength, ack);

            if (response[0] != 0xBB) throw new IOException("Serial Read error: 0x" + Bytearraytostring(response));

            var length = response[1];
            var data = new byte[length];

            for (int i = 2; i < response.Length; i++) 
                data[i - 2] = response[i];
            //Debug.Print("Serial Receive: 0x" + bytearraytostring(data));

            if (data.Length != length) throw new IOException("Timeout reading serial data...");

            return data;
        }

        public byte[] GetRevision() {
            var accel = read_byte(Bno055Registers.Bno055AccelRevIdAddr);
            var mag = read_byte(Bno055Registers.Bno055MagRevIdAddr);
            var gyro = read_byte(Bno055Registers.Bno055GyroRevIdAddr);
            var bl = read_byte(Bno055Registers.Bno055BlRevIdAddr);
            var swLsb = read_byte(Bno055Registers.Bno055SwRevIdLsbAddr);
            var swMsb = read_byte(Bno055Registers.Bno055SwRevIdMsbAddr);
            var sw = (byte) ((swMsb << 8) | swLsb);

            return new[] {accel, mag, gyro, bl, sw};
        }

        public void SetExternalCrystal(bool externalCrystal) {
            ConfigMode();
            write_byte(Bno055Registers.Bno055SysTriggerAddr, (byte) (externalCrystal ? 0x80 : 0x00));
            OpMode();
        }

        private void ConfigMode() {
            Debug.Print("BNO055 entering configuration mode...");
            SetMode(Bno055OpMode.OperationModeConfig);
        }

        private void OpMode() {
            Debug.Print("BNO055 entering normal operation mode...");
            SetMode(_mode);
        }

        private void SetMode(Bno055OpMode mode)
        {
            write_byte(Bno055Registers.Bno055OprModeAddr, (byte)mode);
            Thread.Sleep(20);
        }

        private const byte Bno055Id = 0xA0;
        private const int Baud = 115200;
        private readonly SerialPort _comPort;
        private readonly Bno055OpMode _mode;
        private static readonly object Locker = new object();

        #region BNO055 Registers
        public enum Bno055Registers : byte
        {
            /* Page id register definition */
            Bno055PageIdAddr = 0X07,

            /* PAGE0 REGISTER DEFINITION START*/
            Bno055ChipIdAddr = 0x00,
            Bno055AccelRevIdAddr = 0x01,
            Bno055MagRevIdAddr = 0x02,
            Bno055GyroRevIdAddr = 0x03,
            Bno055SwRevIdLsbAddr = 0x04,
            Bno055SwRevIdMsbAddr = 0x05,
            Bno055BlRevIdAddr = 0X06,

            /* Accel data register */
            Bno055AccelDataXLsbAddr = 0X08,
            Bno055AccelDataXMsbAddr = 0X09,
            Bno055AccelDataYLsbAddr = 0X0A,
            Bno055AccelDataYMsbAddr = 0X0B,
            Bno055AccelDataZLsbAddr = 0X0C,
            Bno055AccelDataZMsbAddr = 0X0D,

            /* Mag data register */
            Bno055MagDataXLsbAddr = 0X0E,
            Bno055MagDataXMsbAddr = 0X0F,
            Bno055MagDataYLsbAddr = 0X10,
            Bno055MagDataYMsbAddr = 0X11,
            Bno055MagDataZLsbAddr = 0X12,
            Bno055MagDataZMsbAddr = 0X13,

            /* Gyro data registers */
            Bno055GyroDataXLsbAddr = 0X14,
            Bno055GyroDataXMsbAddr = 0X15,
            Bno055GyroDataYLsbAddr = 0X16,
            Bno055GyroDataYMsbAddr = 0X17,
            Bno055GyroDataZLsbAddr = 0X18,
            Bno055GyroDataZMsbAddr = 0X19,

            /* Euler data registers */
            Bno055EulerHLsbAddr = 0X1A,
            Bno055EulerHMsbAddr = 0X1B,
            Bno055EulerRLsbAddr = 0X1C,
            Bno055EulerRMsbAddr = 0X1D,
            Bno055EulerPLsbAddr = 0X1E,
            Bno055EulerPMsbAddr = 0X1F,

            /* Quaternion data registers */
            Bno055QuaternionDataWLsbAddr = 0X20,
            Bno055QuaternionDataWMsbAddr = 0X21,
            Bno055QuaternionDataXLsbAddr = 0X22,
            Bno055QuaternionDataXMsbAddr = 0X23,
            Bno055QuaternionDataYLsbAddr = 0X24,
            Bno055QuaternionDataYMsbAddr = 0X25,
            Bno055QuaternionDataZLsbAddr = 0X26,
            Bno055QuaternionDataZMsbAddr = 0X27,

            /* Linear acceleration data registers */
            Bno055LinearAccelDataXLsbAddr = 0X28,
            Bno055LinearAccelDataXMsbAddr = 0X29,
            Bno055LinearAccelDataYLsbAddr = 0X2A,
            Bno055LinearAccelDataYMsbAddr = 0X2B,
            Bno055LinearAccelDataZLsbAddr = 0X2C,
            Bno055LinearAccelDataZMsbAddr = 0X2D,

            /* Gravity data registers */
            Bno055GravityDataXLsbAddr = 0X2E,
            Bno055GravityDataXMsbAddr = 0X2F,
            Bno055GravityDataYLsbAddr = 0X30,
            Bno055GravityDataYMsbAddr = 0X31,
            Bno055GravityDataZLsbAddr = 0X32,
            Bno055GravityDataZMsbAddr = 0X33,

            /* Temperature data register */
            Bno055TempAddr = 0X34,

            /* Status registers */
            Bno055CalibStatAddr = 0X35,
            Bno055SelftestResultAddr = 0X36,
            Bno055IntrStatAddr = 0X37,

            Bno055SysClkStatAddr = 0X38,
            Bno055SysStatAddr = 0X39,
            Bno055SysErrAddr = 0X3A,

            /* Unit selection register */
            Bno055UnitSelAddr = 0X3B,
            Bno055DataSelectAddr = 0X3C,

            /* Mode registers */
            Bno055OprModeAddr = 0X3D,
            Bno055PwrModeAddr = 0X3E,

            Bno055SysTriggerAddr = 0X3F,
            Bno055TempSourceAddr = 0X40,

            /* Axis remap registers */
            Bno055AxisMapConfigAddr = 0X41,
            Bno055AxisMapSignAddr = 0X42,

            /* SIC registers */
            Bno055SicMatrix0LsbAddr = 0X43,
            Bno055SicMatrix0MsbAddr = 0X44,
            Bno055SicMatrix1LsbAddr = 0X45,
            Bno055SicMatrix1MsbAddr = 0X46,
            Bno055SicMatrix2LsbAddr = 0X47,
            Bno055SicMatrix2MsbAddr = 0X48,
            Bno055SicMatrix3LsbAddr = 0X49,
            Bno055SicMatrix3MsbAddr = 0X4A,
            Bno055SicMatrix4LsbAddr = 0X4B,
            Bno055SicMatrix4MsbAddr = 0X4C,
            Bno055SicMatrix5LsbAddr = 0X4D,
            Bno055SicMatrix5MsbAddr = 0X4E,
            Bno055SicMatrix6LsbAddr = 0X4F,
            Bno055SicMatrix6MsbAddr = 0X50,
            Bno055SicMatrix7LsbAddr = 0X51,
            Bno055SicMatrix7MsbAddr = 0X52,
            Bno055SicMatrix8LsbAddr = 0X53,
            Bno055SicMatrix8MsbAddr = 0X54,

            /* Accelerometer Offset registers */
            AccelOffsetXLsbAddr = 0X55,
            AccelOffsetXMsbAddr = 0X56,
            AccelOffsetYLsbAddr = 0X57,
            AccelOffsetYMsbAddr = 0X58,
            AccelOffsetZLsbAddr = 0X59,
            AccelOffsetZMsbAddr = 0X5A,

            /* Magnetometer Offset registers */
            MagOffsetXLsbAddr = 0X5B,
            MagOffsetXMsbAddr = 0X5C,
            MagOffsetYLsbAddr = 0X5D,
            MagOffsetYMsbAddr = 0X5E,
            MagOffsetZLsbAddr = 0X5F,
            MagOffsetZMsbAddr = 0X60,

            /* Gyroscope Offset register s*/
            GyroOffsetXLsbAddr = 0X61,
            GyroOffsetXMsbAddr = 0X62,
            GyroOffsetYLsbAddr = 0X63,
            GyroOffsetYMsbAddr = 0X64,
            GyroOffsetZLsbAddr = 0X65,
            GyroOffsetZMsbAddr = 0X66,

            /* Radius registers */
            AccelRadiusLsbAddr = 0X67,
            AccelRadiusMsbAddr = 0X68,
            MagRadiusLsbAddr = 0X69,
            MagRadiusMsbAddr = 0X6A
        }
        private enum Bno055PowerMode : byte
        {
            PowerModeNormal = 0X00,
            PowerModeLowpower = 0X01,
            PowerModeSuspend = 0X02
        }
        public enum Bno055OpMode : byte
        {
            /* Operation mode settings*/
            OperationModeConfig = 0X00,
            OperationModeAcconly = 0X01,
            OperationModeMagonly = 0X02,
            OperationModeGyronly = 0X03,
            OperationModeAccmag = 0X04,
            OperationModeAccgyro = 0X05,
            OperationModeMaggyro = 0X06,
            OperationModeAmg = 0X07,
            OperationModeImuplus = 0X08,
            OperationModeCompass = 0X09,
            OperationModeM4G = 0X0A,
            OperationModeNdofFmcOff = 0X0B,
            OperationModeNdof = 0X0C
        }
        public struct Bno055RevInfo
        {
            public uint AccelRev;
            public uint MagRev;
            public uint GyroRev;
            public uint SwRev;
            public uint BlRev;
        }
        public enum Bno055VectorType
        {
            VectorAccelerometer = Bno055Registers.Bno055AccelDataXLsbAddr,
            VectorMagnetometer = Bno055Registers.Bno055MagDataXLsbAddr,
            VectorGyroscope = Bno055Registers.Bno055GyroDataXLsbAddr,
            VectorEuler = Bno055Registers.Bno055EulerHLsbAddr,
            VectorLinearaccel = Bno055Registers.Bno055LinearAccelDataXLsbAddr,
            VectorGravity = Bno055Registers.Bno055GravityDataXLsbAddr
        }
        #endregion
    }
}