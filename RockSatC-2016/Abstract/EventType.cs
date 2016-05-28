namespace RockSatC_2016.Abstract {
    public enum EventType : byte {
        None = 0x00,
        BNOUpdate = 0x01,
        GeigerUpdate = 0x02,
        AccelDump = 0x03
    }
}