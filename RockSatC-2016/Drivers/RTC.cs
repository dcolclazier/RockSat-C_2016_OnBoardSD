using System;
using System.Diagnostics;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace RockSatC_2016.Work_Items
{

    internal static class Tools
    {
        public static string Dec2Hex(int Input, int MinLength = 0)
        {
            return Input.ToString("x" + MinLength);
        }
        public static long bcd2bin(byte[] bcd)
        {
            long result = 0;

            foreach (var b in bcd)
            {
                int digit1 = b >> 4;
                int digit2 = b & 0x0f;
                result = (result*100) + digit1*10 + digit2;
            }
            return result;
        }

        public static int bcd2int(int bcd)
        {
            return int.Parse(bcd.ToString("X"));
        }
        public static byte[] bintobcd(int value)
        {
            if (value < 0 || value > 99999999)
                throw new ArgumentOutOfRangeException("value");
            byte[] ret = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                ret[i] = (byte)(value % 10);
                value /= 10;
                ret[i] |= (byte)((value % 10) << 4);
                value /= 10;
            }
            return ret;
        }
        public static uint Hex2Dec(string HexNumber)
        {
            // Always in upper case
            HexNumber = HexNumber.ToUpper();
            // Contains all Hex posibilities
            string ConversionTable = "0123456789ABCDEF";
            // Will contain the return value
            uint RetVal = 0;
            // Will increase
            uint Multiplier = 1;

            for (int Index = HexNumber.Length - 1; Index >= 0; --Index)
            {
                RetVal += (uint)(Multiplier * (ConversionTable.IndexOf(HexNumber[Index])));
                Multiplier = (uint)(Multiplier * ConversionTable.Length);
            }

            return RetVal;
        }
        public static char[] Bytes2Chars(byte[] Input)
        {
            char[] Output = new char[Input.Length];
            for (int Counter = 0; Counter < Input.Length; ++Counter)
                Output[Counter] = (char)Input[Counter];
            return Output;
        }
    }
    // ReSharper disable once InconsistentNaming
    internal static class RTC
    {
        private static readonly I2CDevice.Configuration SlaveConfig;
        private static byte _h;
        private static byte _m;
        private static byte _s;
        private static readonly object locker = new object();
        private const int TransactionTimeout = 1000;
        private const byte ClockRateKHz = 59;

        public static byte Address => 0x68;
        public static byte StatusReg => 0x0f;

        static RTC()
        {
            SlaveConfig = new I2CDevice.Configuration(Address,ClockRateKHz);
        }

        public static void Adjust(byte newHour, byte newMin, byte newSec, byte newDay, byte newMonth, int newYear)
        {
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x00,newSec,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x01,Tools.bintobcd(newMin)[0],TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x02,newHour,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x03, 1,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x04,newDay,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x05,newMonth,TransactionTimeout);
            I2CBus.GetInstance().WriteRegister(SlaveConfig,0x06,(byte)(newYear-2000),TransactionTimeout);
        }

        public static byte[] CurrentTime()
        {

            var time = new byte[7];
            I2CBus.GetInstance().ReadRegister(SlaveConfig, 0x00, time, TransactionTimeout);

            var realseconds = Tools.bcd2bin(new [] { time[0] });
            var minutes = Tools.bcd2bin(new [] { time[1] });
            var hours = Tools.bcd2bin(new [] { time[2] });

            Debug.Print("Current time: " + hours + ":" + minutes + ":" + realseconds + ":" + time[3] + ":" + time[4] + ":" + time[5] + ":" + time[6]);
            return new[]
            {
                (byte)hours,
                (byte)minutes,
                (byte)realseconds
            };

        }
 
    }
}