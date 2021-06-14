
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBD.Psi.Visualization.Windows
{
    using MathNet.Spatial.Euclidean;
    using System.Numerics;
    using Microsoft.Psi.Visualization.Views.Visuals3D;
    using Microsoft.Psi.Visualization.VisualizationObjects;
    using RosSharp.Urdf;
    using Win3D = System.Windows.Media.Media3D;
    using TBD.Psi.Utility;

    [VisualizationObject("URDF")]

    public class URDFVisualizationObject : ModelVisual3DVisualizationObjectCollectionBase<URDFLinkVisualizationObject, (CoordinateSystem baseTransformation, Robot urdf, Dictionary<string, double> jointStates)>
    {
        // The collection of child visualization objects.
        private readonly List<URDFLinkVisualizationObject> children = new List<URDFLinkVisualizationObject>();

        public URDFVisualizationObject()
        {
            this.Items = this.children;
        }

        private void AddNew(Link link, CoordinateSystem frame)
        {
            // Create a new child TVisObj.  It will already be
            // initialized with all the properties of the prototype.
            URDFLinkVisualizationObject child = this.CreateNew();
            // add the value
            child.SetCurrentValue(this.SynthesizeMessage((frame, link)));
            // Add it to the collection
            this.children.Add(child);

            // Ad the new visualization object's model view as a child of our model view
            this.ModelView.Children.Add(child.ModelView);
        }

        private void updateLinkValueRecursively(Link link, CoordinateSystem frame)
        {
            // create new child object if doesn't exist
            if (!this.children.Where(l => l.LinkName == link.name).Any())
            {
                this.AddNew(link, frame);
            }
            else
            {
                // update link position
                this.children.Where(l => l.LinkName == link.name).First().SetCurrentValue(this.SynthesizeMessage((frame, link)));
            }
            // now we call all the child links
            foreach( var joint in link.joints)
            {
                if (joint.type == "fixed")
                {
                    var transform = SpatialExtensions.ConstructCoordinateSystem(joint.origin.Xyz[0], joint.origin.Xyz[1], joint.origin.Xyz[2], joint.origin.Rpy[0], joint.origin.Rpy[1], joint.origin.Rpy[2]);
                    // apply transform to current frame
                    var newFrame = transform.TransformBy(frame);
                    this.updateLinkValueRecursively(joint.ChildLink, newFrame);
                }
                if (joint.type == "revolute" || joint.type == "continuous")
                {
                    var fixedTransform = SpatialExtensions.ConstructCoordinateSystem(joint.origin.Xyz[0], joint.origin.Xyz[1], joint.origin.Xyz[2], joint.origin.Rpy[0], joint.origin.Rpy[1], joint.origin.Rpy[2]);
                    var newFrame = fixedTransform.TransformBy(frame);
                    // now we figure out the joint value
                    var jointValue = 0.0;
                    if (this.CurrentData.jointStates.ContainsKey(joint.name))
                    {
                        jointValue = this.CurrentData.jointStates[joint.name];
                    }
                    // TODO in the future, we can use the URDF to check the limits.
                    // apply the rotation with the given joint value.
                    var rotQ = System.Numerics.Quaternion.CreateFromAxisAngle(new Vector3((float)joint.axis.xyz[0], (float)joint.axis.xyz[1], (float)joint.axis.xyz[2]), Convert.ToSingle(jointValue));
                    newFrame = SpatialExtensions.ConstructCoordinateSystem(new Vector3D(), rotQ).TransformBy(newFrame);
                    this.updateLinkValueRecursively(joint.ChildLink, newFrame);
                }
            }
        }


        public override void UpdateData()
        {
            if (this.CurrentData.baseTransformation != default && this.CurrentData.urdf != default)
            {
                // start the recursive update
                this.updateLinkValueRecursively(this.CurrentData.urdf.root, this.CurrentData.baseTransformation);
            }
        }
    }
}
