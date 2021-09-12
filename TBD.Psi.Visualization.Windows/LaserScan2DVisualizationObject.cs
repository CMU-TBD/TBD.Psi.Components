namespace TBD.Psi.Visualization.Windows
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using Microsoft.Psi.Visualization.VisualizationObjects;
    using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
    using System.Collections.Generic;
    using Windows = System.Windows.Media.Media3D;
    using TBD.Psi.Sensors;
    using MathNet.Spatial.Euclidean;

    [VisualizationObject("Laser Scan 2D")]
    public class LaserScan2DVisualizationObject : ModelVisual3DVisualizationObject<LaserScan2D>
    {
        private Point3DListAsPointCloudVisualizationObject pointCloud;

        public LaserScan2DVisualizationObject()
        {
            this.PointCloud = new Point3DListAsPointCloudVisualizationObject();
        }

        /// <summary>
        /// Gets or sets the visualization object for the point cloud that represents the laser.
        /// </summary>
        [ExpandableObject]
        [DataMember]
        [DisplayName("Laser Scan")]
        [Description("The 3D point cloud representing the 2D laser scan.")]
        public Point3DListAsPointCloudVisualizationObject PointCloud
        {
            get { return this.pointCloud; }
            set { this.Set(nameof(this.PointCloud), ref this.pointCloud, value); }
        }

        public CoordinateSystem TransformToWorld = new CoordinateSystem();

        public override void NotifyPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(this.Visible))
            {
                this.UpdateVisibility();
            }
        }

        public override void UpdateData()
        {
            if (this.CurrentData != null)
            {
                var points = new List<Windows.Point3D>(this.CurrentData.Ranges.Length);
                // conver the laser scanner into points
                for (var i = 0; i < this.CurrentData.Ranges.Length; i++)
                {
                    var currAngle = this.CurrentData.MinAngle + (i * this.CurrentData.AngleIncrement);
                    if (this.CurrentData.Ranges[i] >= this.CurrentData.MinRange && this.CurrentData.Ranges[i] <= this.CurrentData.MaxRange)
                    {
                        var x = this.CurrentData.Ranges[i] * Math.Cos(currAngle);
                        var y = this.CurrentData.Ranges[i] * Math.Sin(currAngle);
                        var point = new Point3D(x, y, 0);
                        var transformedPoint = point.TransformBy(this.TransformToWorld);
                        points.Add(new Windows.Point3D(transformedPoint.X, transformedPoint.Y, transformedPoint.Z));
                    }
                }
                this.PointCloud.SetCurrentValue(this.SynthesizeMessage(points));
            }
            this.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            this.UpdateChildVisibility(this.pointCloud.ModelView, this.Visible && this.CurrentData != default);
        }
    }
}
