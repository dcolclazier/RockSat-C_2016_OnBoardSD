using System;
using System.Text;
using RockSatC_2016.Flight_Computer;

namespace RockSatC_2016.Work_Items
{
    public static class Rebug
    {
        public static int _metaDataCount = 4;
        public static int _timeDataCount = 3;

        public static void Print(string what)
        {
            var bytes = Encoding.UTF8.GetBytes(what);
            var packet = new byte[bytes.Length + _metaDataCount + _timeDataCount];
            packet[0] = (byte) PacketType.StartByte;
            packet[1] = (byte) PacketType.DebugMessage;

            var size = bytes.Length + _timeDataCount;
            var msb = (byte) ((size >> 8) & 0xff);
            var lsb = (byte) (size & 0xff);

            packet[2] = msb;
            packet[3] = lsb;

            var time = BitConverter.GetBytes(Clock.Instance.ElapsedMilliseconds);
            packet[4] = time[0];
            packet[5] = time[1];
            packet[6] = time[2];

            for (int i = 7; i < packet.Length; i++)
                packet[i] = bytes[i - 7];
            FlightComputer.Logger.AddPacket(ref packet);

        }
    }
}