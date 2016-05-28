using System;

namespace RockSatC_2016.Utility {
    public class Quaternion {
        public double W { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Quaternion() {
            W = 1.0;
            X = Y = Z = 0.0;
        }

        public Quaternion(double iw, double ix, double iy, double iz) {
            W = iw;
            X = ix;
            Y = iy;
            Z = iz;
        }

        private Quaternion(double w, Vector vec) {
            W = w;
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
        }
        public double magnitude() 
        {
            double res = (W * W) + (X * X) + (Y * Y) + (Z * Z);
            return Math.Sqrt(res);
        }

        //public void normalize()
        //{
        //    double mag = magnitude();
        //     scale(1 / mag);
        //}
        Quaternion conjugate() 
        {
            return new Quaternion {
                W = W,
                X = -X,
                Y = -Y,
                Z = -Z
            };
        }
        Vector toEuler() 
        {
            double sqw = W * W;
            double sqx = X * X;
            double sqy = Y * Y;
            double sqz = Z * Z;

            var ret = new Vector {
                X = (float)Math.Atan2((2.0*(X*Y + Z*W)), (sqx - sqy - sqz + sqw)),
                Y = (float)Math.Asin(-2.0*(X*Z - Y*W)/(sqx + sqy + sqz + sqw)),
                Z = (float)Math.Atan2(2.0*(Y*Z + X*W), (-sqx - sqy + sqz + sqw))
            };

            return ret;
        }
        //void scale(double scalar) 
        //{
        //Quaternion ret;
        //ret._w = this->_w* scalar;
        //ret._x = this->_x* scalar;
        //ret._y = this->_y* scalar;
        //ret._z = this->_z* scalar;
        //return ret;
        //}

}
}