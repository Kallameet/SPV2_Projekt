using System;

namespace SensorValuesServer
{
    [Serializable]
    public class SensorValues
    {
        public int AccelerometerX { get; set; }

        public int AccelerometerY { get; set; }

        public int AccelerometerZ { get; set; }

        public override string ToString()
        {
            return String.Format("AccelerometerX: {0}, AccelerometerY: {1}, AccelerometerZ: {2}", AccelerometerX,
                AccelerometerY, AccelerometerZ);
        }
    }
}
