using System;

namespace TBD.Psi.Sensors
{
    public class LaserScan2D
    {
        private readonly float[] data;
        private readonly float maxAngle;
        private readonly float minAngle;
        private readonly float angleIncrement;
        private readonly TimeSpan scanTime;
        private readonly float maxRange;
        private readonly float minRange;

        public LaserScan2D(float[] ranges, float maxAngle, float minAngle, float angleIncrement, TimeSpan scanTime, float maxRange, float minRange)
        {
            this.data = ranges;
            this.maxAngle = maxAngle;
            this.minAngle = minAngle;
            this.angleIncrement = angleIncrement;
            this.scanTime = scanTime;
            this.maxRange = maxRange;
            this.minRange = minRange;
        }

        public float[] Ranges => this.data;
        public float MaxAngle => this.maxAngle;
        public float MinAngle => this.minAngle;
        public float AngleIncrement => this.angleIncrement;
        public TimeSpan ScanTime => this.scanTime;
        public float MaxRange => this.maxRange;
        public float MinRange => this.minRange;

    }
}
