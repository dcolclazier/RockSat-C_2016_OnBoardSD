using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using Microsoft.SPOT;
using RockSatC_2016.Utility;

namespace RockSatC_2016.Drivers {
    
    public class SerialBNO {

        public SerialBNO(string comPort, int readTimeout, int writeTimeout, Bno055OpMode mode = Bno055OpMode.Operation_Mode_Accgyro) {
            _mode = mode;
            Debug.Print("Initialing BNO Serial Port... " + comPort + ", " + _baud + " bps");
            Debug.Print("ReadTimeout: " + readTimeout);
            Debug.Print("WriteTimeout: " + writeTimeout);
            _comPort = new SerialPort(comPort, _baud,0,8,StopBits.One) {
                ReadTimeout = readTimeout,
                WriteTimeout = writeTimeout
            };
        }

        public bool begin()
        {
            Debug.Print("Opening BNO communications channel...");
            _comPort.Open();

            configMode();
            write_byte(Bno055Registers.Bno055_Page_Id_Addr, 0);

            var bnoID = read_byte(Bno055Registers.Bno055_Chip_Id_Addr);
            Debug.Print("Read chip ID for BNO055: 0x" + bnoID.ToString("x"));

            if (bnoID != Bno055Id) return false;

            //reset the device
            write_byte(Bno055Registers.Bno055_Sys_Trigger_Addr, 0x20, false);

            //sleep 650 ms after reset for chip to be ready(as suggested in datasheet)
            Thread.Sleep(650);

            //set to normal power mode
            write_byte(Bno055Registers.Bno055_Pwr_Mode_Addr, (byte)Bno055PowerMode.Power_Mode_Normal);

            //default to external oscillator
            write_byte(Bno055Registers.Bno055_Sys_Trigger_Addr, 0x80);

            //enter normal operation mode
            opMode();

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
                case Bno055VectorType.Vector_Magnetometer:
                case Bno055VectorType.Vector_Euler:
                    modifier = 16.0f;
                    break;
                case Bno055VectorType.Vector_Gyroscope:
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

        private byte[] serial_send(byte[] command, int expectedLength, bool ack = true, int max_tries = 10, int tries = 0) {

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
            if (++tries == max_tries)
                throw new IOException("Exceeded max tries to acknowlege serial command without bus error.");


            return serial_send(command, expectedLength, max_tries:max_tries, tries:tries);
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

            if (ack) verify(response);
        }

        private void verify(byte[] response) {
            if (response[0] != 0xEE && response[1] != 0x01)
                throw new IOException("Error writing to register: 0x" + response);
        }

        private string bytearraytostring(byte[] array) {
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

            if (response[0] != 0xBB) throw new IOException("Serial Read error: 0x" + bytearraytostring(response));

            var length = response[1];
            var data = new byte[length];

            for (int i = 2; i < response.Length; i++) 
                data[i - 2] = response[i];
            //Debug.Print("Serial Receive: 0x" + bytearraytostring(data));

            if (data.Length != length) throw new IOException("Timeout reading serial data...");

            return data;
        }

        public byte[] getRevision() {
            var accel = read_byte(Bno055Registers.Bno055_Accel_Rev_Id_Addr);
            var mag = read_byte(Bno055Registers.Bno055_Mag_Rev_Id_Addr);
            var gyro = read_byte(Bno055Registers.Bno055_Gyro_Rev_Id_Addr);
            var bl = read_byte(Bno055Registers.Bno055_Bl_Rev_Id_Addr);
            var swLsb = read_byte(Bno055Registers.Bno055_Sw_Rev_Id_Lsb_Addr);
            var swMsb = read_byte(Bno055Registers.Bno055_Sw_Rev_Id_Msb_Addr);
            var sw = (byte) ((swMsb << 8) | swLsb);

            return new[] {accel, mag, gyro, bl, sw};
        }

        public void setExternalCrystal(bool external_crystal) {
            configMode();
            write_byte(Bno055Registers.Bno055_Sys_Trigger_Addr, (byte) (external_crystal ? 0x80 : 0x00));
            opMode();
        }

        private void configMode() {
            Debug.Print("BNO055 entering configuration mode...");
            setMode(Bno055OpMode.Operation_Mode_Config);
        }

        private void opMode() {
            Debug.Print("BNO055 entering normal operation mode...");
            setMode(_mode);
        }

        private void setMode(Bno055OpMode mode)
        {
            write_byte(Bno055Registers.Bno055_Opr_Mode_Addr, (byte)mode);
            Thread.Sleep(20);
        }

        private const byte Bno055Id = 0xA0;
        private const int _baud = 115200;
        private readonly SerialPort _comPort;
        private readonly Bno055OpMode _mode;
        private static readonly object Locker = new object();

        #region BNO055 Registers
        public enum Bno055Registers : byte
        {
            /* Page id register definition */
            Bno055_Page_Id_Addr = 0X07,

            /* PAGE0 REGISTER DEFINITION START*/
            Bno055_Chip_Id_Addr = 0x00,
            Bno055_Accel_Rev_Id_Addr = 0x01,
            Bno055_Mag_Rev_Id_Addr = 0x02,
            Bno055_Gyro_Rev_Id_Addr = 0x03,
            Bno055_Sw_Rev_Id_Lsb_Addr = 0x04,
            Bno055_Sw_Rev_Id_Msb_Addr = 0x05,
            Bno055_Bl_Rev_Id_Addr = 0X06,

            /* Accel data register */
            Bno055_Accel_Data_X_Lsb_Addr = 0X08,
            Bno055_Accel_Data_X_Msb_Addr = 0X09,
            Bno055_Accel_Data_Y_Lsb_Addr = 0X0A,
            Bno055_Accel_Data_Y_Msb_Addr = 0X0B,
            Bno055_Accel_Data_Z_Lsb_Addr = 0X0C,
            Bno055_Accel_Data_Z_Msb_Addr = 0X0D,

            /* Mag data register */
            Bno055_Mag_Data_X_Lsb_Addr = 0X0E,
            Bno055_Mag_Data_X_Msb_Addr = 0X0F,
            Bno055_Mag_Data_Y_Lsb_Addr = 0X10,
            Bno055_Mag_Data_Y_Msb_Addr = 0X11,
            Bno055_Mag_Data_Z_Lsb_Addr = 0X12,
            Bno055_Mag_Data_Z_Msb_Addr = 0X13,

            /* Gyro data registers */
            Bno055_Gyro_Data_X_Lsb_Addr = 0X14,
            Bno055_Gyro_Data_X_Msb_Addr = 0X15,
            Bno055_Gyro_Data_Y_Lsb_Addr = 0X16,
            Bno055_Gyro_Data_Y_Msb_Addr = 0X17,
            Bno055_Gyro_Data_Z_Lsb_Addr = 0X18,
            Bno055_Gyro_Data_Z_Msb_Addr = 0X19,

            /* Euler data registers */
            Bno055_Euler_H_Lsb_Addr = 0X1A,
            Bno055_Euler_H_Msb_Addr = 0X1B,
            Bno055_Euler_R_Lsb_Addr = 0X1C,
            Bno055_Euler_R_Msb_Addr = 0X1D,
            Bno055_Euler_P_Lsb_Addr = 0X1E,
            Bno055_Euler_P_Msb_Addr = 0X1F,

            /* Quaternion data registers */
            Bno055_Quaternion_Data_W_Lsb_Addr = 0X20,
            Bno055_Quaternion_Data_W_Msb_Addr = 0X21,
            Bno055_Quaternion_Data_X_Lsb_Addr = 0X22,
            Bno055_Quaternion_Data_X_Msb_Addr = 0X23,
            Bno055_Quaternion_Data_Y_Lsb_Addr = 0X24,
            Bno055_Quaternion_Data_Y_Msb_Addr = 0X25,
            Bno055_Quaternion_Data_Z_Lsb_Addr = 0X26,
            Bno055_Quaternion_Data_Z_Msb_Addr = 0X27,

            /* Linear acceleration data registers */
            Bno055_Linear_Accel_Data_X_Lsb_Addr = 0X28,
            Bno055_Linear_Accel_Data_X_Msb_Addr = 0X29,
            Bno055_Linear_Accel_Data_Y_Lsb_Addr = 0X2A,
            Bno055_Linear_Accel_Data_Y_Msb_Addr = 0X2B,
            Bno055_Linear_Accel_Data_Z_Lsb_Addr = 0X2C,
            Bno055_Linear_Accel_Data_Z_Msb_Addr = 0X2D,

            /* Gravity data registers */
            Bno055_Gravity_Data_X_Lsb_Addr = 0X2E,
            Bno055_Gravity_Data_X_Msb_Addr = 0X2F,
            Bno055_Gravity_Data_Y_Lsb_Addr = 0X30,
            Bno055_Gravity_Data_Y_Msb_Addr = 0X31,
            Bno055_Gravity_Data_Z_Lsb_Addr = 0X32,
            Bno055_Gravity_Data_Z_Msb_Addr = 0X33,

            /* Temperature data register */
            Bno055_Temp_Addr = 0X34,

            /* Status registers */
            Bno055_Calib_Stat_Addr = 0X35,
            Bno055_Selftest_Result_Addr = 0X36,
            Bno055_Intr_Stat_Addr = 0X37,

            Bno055_Sys_Clk_Stat_Addr = 0X38,
            Bno055_Sys_Stat_Addr = 0X39,
            Bno055_Sys_Err_Addr = 0X3A,

            /* Unit selection register */
            Bno055_Unit_Sel_Addr = 0X3B,
            Bno055_Data_Select_Addr = 0X3C,

            /* Mode registers */
            Bno055_Opr_Mode_Addr = 0X3D,
            Bno055_Pwr_Mode_Addr = 0X3E,

            Bno055_Sys_Trigger_Addr = 0X3F,
            Bno055_Temp_Source_Addr = 0X40,

            /* Axis remap registers */
            Bno055_Axis_Map_Config_Addr = 0X41,
            Bno055_Axis_Map_Sign_Addr = 0X42,

            /* SIC registers */
            Bno055_Sic_Matrix_0_Lsb_Addr = 0X43,
            Bno055_Sic_Matrix_0_Msb_Addr = 0X44,
            Bno055_Sic_Matrix_1_Lsb_Addr = 0X45,
            Bno055_Sic_Matrix_1_Msb_Addr = 0X46,
            Bno055_Sic_Matrix_2_Lsb_Addr = 0X47,
            Bno055_Sic_Matrix_2_Msb_Addr = 0X48,
            Bno055_Sic_Matrix_3_Lsb_Addr = 0X49,
            Bno055_Sic_Matrix_3_Msb_Addr = 0X4A,
            Bno055_Sic_Matrix_4_Lsb_Addr = 0X4B,
            Bno055_Sic_Matrix_4_Msb_Addr = 0X4C,
            Bno055_Sic_Matrix_5_Lsb_Addr = 0X4D,
            Bno055_Sic_Matrix_5_Msb_Addr = 0X4E,
            Bno055_Sic_Matrix_6_Lsb_Addr = 0X4F,
            Bno055_Sic_Matrix_6_Msb_Addr = 0X50,
            Bno055_Sic_Matrix_7_Lsb_Addr = 0X51,
            Bno055_Sic_Matrix_7_Msb_Addr = 0X52,
            Bno055_Sic_Matrix_8_Lsb_Addr = 0X53,
            Bno055_Sic_Matrix_8_Msb_Addr = 0X54,

            /* Accelerometer Offset registers */
            Accel_Offset_X_Lsb_Addr = 0X55,
            Accel_Offset_X_Msb_Addr = 0X56,
            Accel_Offset_Y_Lsb_Addr = 0X57,
            Accel_Offset_Y_Msb_Addr = 0X58,
            Accel_Offset_Z_Lsb_Addr = 0X59,
            Accel_Offset_Z_Msb_Addr = 0X5A,

            /* Magnetometer Offset registers */
            Mag_Offset_X_Lsb_Addr = 0X5B,
            Mag_Offset_X_Msb_Addr = 0X5C,
            Mag_Offset_Y_Lsb_Addr = 0X5D,
            Mag_Offset_Y_Msb_Addr = 0X5E,
            Mag_Offset_Z_Lsb_Addr = 0X5F,
            Mag_Offset_Z_Msb_Addr = 0X60,

            /* Gyroscope Offset register s*/
            Gyro_Offset_X_Lsb_Addr = 0X61,
            Gyro_Offset_X_Msb_Addr = 0X62,
            Gyro_Offset_Y_Lsb_Addr = 0X63,
            Gyro_Offset_Y_Msb_Addr = 0X64,
            Gyro_Offset_Z_Lsb_Addr = 0X65,
            Gyro_Offset_Z_Msb_Addr = 0X66,

            /* Radius registers */
            Accel_Radius_Lsb_Addr = 0X67,
            Accel_Radius_Msb_Addr = 0X68,
            Mag_Radius_Lsb_Addr = 0X69,
            Mag_Radius_Msb_Addr = 0X6A
        }
        private enum Bno055PowerMode : byte
        {
            Power_Mode_Normal = 0X00,
            Power_Mode_Lowpower = 0X01,
            Power_Mode_Suspend = 0X02
        }
        public enum Bno055OpMode : byte
        {
            /* Operation mode settings*/
            Operation_Mode_Config = 0X00,
            Operation_Mode_Acconly = 0X01,
            Operation_Mode_Magonly = 0X02,
            Operation_Mode_Gyronly = 0X03,
            Operation_Mode_Accmag = 0X04,
            Operation_Mode_Accgyro = 0X05,
            Operation_Mode_Maggyro = 0X06,
            Operation_Mode_Amg = 0X07,
            Operation_Mode_Imuplus = 0X08,
            Operation_Mode_Compass = 0X09,
            Operation_Mode_M4G = 0X0A,
            Operation_Mode_Ndof_Fmc_Off = 0X0B,
            Operation_Mode_Ndof = 0X0C
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
            Vector_Accelerometer = Bno055Registers.Bno055_Accel_Data_X_Lsb_Addr,
            Vector_Magnetometer = Bno055Registers.Bno055_Mag_Data_X_Lsb_Addr,
            Vector_Gyroscope = Bno055Registers.Bno055_Gyro_Data_X_Lsb_Addr,
            Vector_Euler = Bno055Registers.Bno055_Euler_H_Lsb_Addr,
            Vector_Linearaccel = Bno055Registers.Bno055_Linear_Accel_Data_X_Lsb_Addr,
            Vector_Gravity = Bno055Registers.Bno055_Gravity_Data_X_Lsb_Addr
        }
        #endregion
    }
}