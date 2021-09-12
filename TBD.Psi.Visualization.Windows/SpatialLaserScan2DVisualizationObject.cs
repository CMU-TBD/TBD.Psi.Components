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
    public class SpatialLaserScan2DVisualizationObject : ModelVisual3DVisualizationObject<(CoordinateSystem, LaserScan2D)>
    {
        private LaserScan2DVisualizationObject baseObject;

        public SpatialLaserScan2DVisualizationObject()
        {
            this.baseObject = new LaserScan2DVisualizationObject();
        }

        /// <summary>
        /// Gets or sets the visualization object for the point cloud that represents the laser.
        /// </summary>
        [ExpandableObject]
        [DataMember]
        [DisplayName("Laser Scan")]
        [Description("The Laser Scan Properties.")]
        public LaserScan2DVisualizationObject LaserScan
        {
            get { return this.baseObject; }
            set { this.Set(nameof(this.baseObject), ref this.baseObject, value); }
        }

        public override void NotifyPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(this.Visible))
            {
                this.UpdateVisibility();
            }
        }

        public override void UpdateData()
        {
            if (this.CurrentData != default)
            {
                this.baseObject.TransformToWorld = this.CurrentData.Item1;
                this.baseObject.SetCurrentValue(this.SynthesizeMessage(this.CurrentData.Item2));
            }
            this.UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            this.UpdateChildVisibility(this.baseObject.ModelView, this.Visible && this.CurrentData != default);
        }
    }
}
