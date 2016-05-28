namespace RockSatC_2016.Utility {
    public struct Vector {
        public float X { get; set; } 
        public float Y { get; set; } 
        public float Z { get; set; }
        //public Vector() {
        //    X = 0;
        //    Y = 0;
        //    Z = 0;
        //}

        public Vector(float x, float y, float z) {
            X = x;
            Y = y;
            Z = z;
        }
    }
}