using System;

namespace SensorValues
{
    [Serializable]
    public class SensorValues
    {
        public double AccelerometerX { get; set; }

        public double AccelerometerY { get; set; }

        public double AccelerometerZ { get; set; }

        public override string ToString()
        {
            return String.Format("AccelerometerX: {0}, AccelerometerY: {1}, AccelerometerZ: {2}", AccelerometerX,
                AccelerometerY, AccelerometerZ);
        }
    }
}
