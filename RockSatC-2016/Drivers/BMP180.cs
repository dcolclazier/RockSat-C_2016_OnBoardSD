#undef BMP085_USE_DATASHEET_VALS // define for sanity check

namespace RockSatC_2016.Drivers
{
    //public class Bmp180 
    //{
    //    private enum Registers : byte
    //    {
    //        Bmp085_Register_Cal_Ac1 = 0xAA,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_Ac2 = 0xAC,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_Ac3 = 0xAE,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_Ac4 = 0xB0,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_Ac5 = 0xB2,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_Ac6 = 0xB4,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_B1 = 0xB6,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_B2 = 0xB8,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_Mb = 0xBA,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_Mc = 0xBC,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Cal_Md = 0xBE,  // R   Calibration data (16 bits)
    //        Bmp085_Register_Chipid = 0xD0,
    //        Bmp085_Register_Version = 0xD1,
    //        Bmp085_Register_Softreset = 0xE0,
    //        Bmp085_Register_Control = 0xF4,
    //        Bmp085_Register_Tempdata = 0xF6,
    //        Bmp085_Register_Pressuredata = 0xF6,
    //        Bmp085_Register_Readtempcmd = 0x2E,
    //        Bmp085_Register_Readpressurecmd = 0x34
    //    }

    //    public Bmp180(byte address = 0x77, Mode mode = Mode.Bmp085_Mode_Ultralowpower) {
    //        Bmp085Address = address;
    //        _slaveConfig = new I2CDevice.Configuration(Bmp085Address, 100);
    //        while (!Init(mode))
    //        {
                
    //            Debug.Print("BMP sensor not detected...");
    //        }
    //    }

    //    public enum Mode : byte
    //    {
    //        Bmp085_Mode_Ultralowpower = 0,
    //        Bmp085_Mode_Standard = 1,
    //        Bmp085_Mode_Highres = 2,
    //        Bmp085_Mode_Ultrahighres = 3
    //    }

    //    struct Bmp085CalibData
    //    {
    //        public short Ac1;
    //        public short Ac2;
    //        public short Ac3;
    //        public ushort Ac4;
    //        public ushort Ac5;
    //        public ushort Ac6;
    //        public short B1;
    //        public short B2;
    //        public short Mb;
    //        public short Mc;
    //        public short Md;
    //    }

    //    private byte Bmp085Address = 0x77;
    //    public const double SensorsPressureSealevelhpa = 1013.25;
    //    Mode _mode = Mode.Bmp085_Mode_Ultrahighres;
    //    Bmp085CalibData _bmp085Coeffs;

    //    private readonly I2CDevice.Configuration _slaveConfig;
    //    private const int TransactionTimeout = 1000; // ms

    //    public bool Init(Mode mode = Mode.Bmp085_Mode_Standard)
    //    {
    //        if ((mode > Mode.Bmp085_Mode_Ultrahighres) || (mode < 0)) 
    //            _mode = Mode.Bmp085_Mode_Standard;
    //        else 
    //            _mode = mode;
            

    //        byte[] whoami = { 0 };

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Chipid, whoami, TransactionTimeout);

    //        if (whoami[0] != 0x55) return false;
            
    //        ReadCoefficients();

    //        return true;
    //    }

    //    void ReadCoefficients()
    //    {

    //        var buffer = new byte[2];

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Ac1, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Ac1 = (short)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Ac2, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Ac2 = (short)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Ac3, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Ac3 = (short)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Ac4, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Ac4 = (ushort)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Ac5, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Ac5 = (ushort)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Ac6, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Ac6 = (ushort)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_B1, buffer, TransactionTimeout);
    //        _bmp085Coeffs.B1 = (short)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_B2, buffer, TransactionTimeout);
    //        _bmp085Coeffs.B2 = (short)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Mb, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Mb = (short)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Mc, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Mc = (short)((buffer[0] << 8) | buffer[1]);

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Cal_Md, buffer, TransactionTimeout);
    //        _bmp085Coeffs.Md = (short)((buffer[0] << 8) | buffer[1]);

    //    }

    //    ushort ReadRawTemperature()
    //    {
     
    //        I2CBus.Instance().WriteRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Control, (byte)Registers.Bmp085_Register_Readtempcmd, TransactionTimeout);

    //        Program.custom_delay_usec(4500);
    //        //Thread.Sleep(5);

    //        var buffer = new byte[2];

    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Tempdata, buffer, TransactionTimeout);
    //        return (ushort)((buffer[0] << 8) | buffer[1]);
    //    }

    //    int ReadRawPressure()
    //    {
    //        I2CBus.Instance().WriteRegister(_slaveConfig, (byte)Registers.Bmp085_Register_Control, (byte)((byte)Registers.Bmp085_Register_Readpressurecmd + ((byte)_mode << 6)), TransactionTimeout);

    //        switch (_mode)
    //        {
    //            case Mode.Bmp085_Mode_Ultralowpower:
    //                Program.custom_delay_usec(4500);
    //                //Thread.Sleep(5);
    //                break;
    //            case Mode.Bmp085_Mode_Standard:
    //                Program.custom_delay_usec(7500);
    //                //Thread.Sleep(8);
    //                break;
    //            case Mode.Bmp085_Mode_Highres:
    //                //Thread.Sleep(14);
    //                Program.custom_delay_usec(13500);
    //                break;
    //            case Mode.Bmp085_Mode_Ultrahighres:
    //                //Thread.Sleep(26);
    //                Program.custom_delay_usec(25500);
    //                break;
    //            default:
    //                throw new ArgumentOutOfRangeException();
    //        }

    //        var buffer16 = new byte[2];
    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte) Registers.Bmp085_Register_Pressuredata, buffer16, TransactionTimeout);

    //        var p32 = (ushort) (buffer16[1] | (buffer16[0] << 8)) << 8;

    //        var buffer8 = new byte[1];
    //        I2CBus.Instance().ReadRegister(_slaveConfig, (byte) Registers.Bmp085_Register_Pressuredata + 2, buffer8, TransactionTimeout);

    //        p32 += buffer8[0];
    //        p32 >>= (8 - (byte) _mode);

    //        return p32;
    //    }

    //    public double GetPressure() {
    //        /* Get the raw pressure and temperature values */
    //        var ut = ReadRawTemperature();
    //        var up = ReadRawPressure();

    //        /* Temperature compensation */
    //        var x1 = (int) ((ut - _bmp085Coeffs.Ac6)*_bmp085Coeffs.Ac5/Math.Pow(2, 15));
    //        var x2 = ((int) (_bmp085Coeffs.Mc*Math.Pow(2, 11)))/(x1 + _bmp085Coeffs.Md);
    //        var b5 = x1 + x2;

    //        /* Pressure compensation */
    //        var b6 = b5 - 4000;
    //        x1 = (_bmp085Coeffs.B2*((b6*b6) >> 12)) >> 11;
    //        x2 = (_bmp085Coeffs.Ac2*b6) >> 11;
    //        var x3 = x1 + x2;
    //        var b3 = (((_bmp085Coeffs.Ac1*4 + x3) << (byte) _mode) + 2) >> 2;
    //        x1 = (_bmp085Coeffs.Ac3*b6) >> 13;
    //        x2 = (_bmp085Coeffs.B1*((b6*b6) >> 12)) >> 16;
    //        x3 = ((x1 + x2) + 2) >> 2;
    //        var b4 = (_bmp085Coeffs.Ac4*(uint) (x3 + 32768)) >> 15;
    //        var b7 = (uint) ((uint) (up - b3)*(50000 >> (byte) _mode));

    //        var p = (b7 < 0x80000000) ? (int) ((b7 << 1)/b4) : (int) ((b7/b4) << 1);

    //        x1 = (p >> 8)*(p >> 8);
    //        x1 = (x1*3038) >> 16;
    //        x2 = (-7357*p) >> 16;
    //        return (p + ((x1 + x2 + 3791) >> 4));
    //    }

    //    public double GetTemperature() {
    //        int ut = ReadRawTemperature();

    //        // step 1
    //        var x1 = (int) ((ut - _bmp085Coeffs.Ac6)*_bmp085Coeffs.Ac5/Math.Pow(2, 15));
    //        var x2 = (int) ((_bmp085Coeffs.Mc*Math.Pow(2, 11))/(x1 + _bmp085Coeffs.Md));
    //        return ((x1 + x2 + 8)/Math.Pow(2, 4))/10;
    //    }

    //    public static double PressureToAltitude(double seaLevel, double atmospheric, double temp) {
    //        /* Hyposometric formula:                      */
    //        /*                                            */
    //        /*     ((P0/P)^(1/5.257) - 1) * (T + 273.15)  */
    //        /* h = -------------------------------------  */
    //        /*                   0.0065                   */
    //        /*                                            */
    //        /* where: h   = height (in meters)            */
    //        /*        P0  = sea-level pressure (in hPa)   */
    //        /*        P   = atmospheric pressure (in hPa) */
    //        /*        T   = temperature (in �C)           */

    //        return ((Math.Pow((seaLevel/atmospheric), 0.190223F) - 1.0F)*(temp + 273.15F))/0.0065;
    //    }
    //}
}
