
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TBD.Psi.Visualization.Windows
{
    using MathNet.Spatial.Euclidean;
    using Microsoft.Psi.Visualization.Views.Visuals3D;
    using Microsoft.Psi.Visualization.VisualizationObjects;
    using HelixToolkit.Wpf;
    using RosSharp.Urdf;
    using Win3D = System.Windows.Media.Media3D;
    using System.Windows.Media;
    using System.IO;
    using System.Windows.Media.Media3D;
    using TBD.Psi.Utility;

    [VisualizationObject("URDF link")]
    public class URDFLinkVisualizationObject : ModelVisual3DVisualizationObject<(CoordinateSystem, Link)>
    {
        private Dictionary<string, string> packageMapping = new Dictionary<string, string>();
        private bool initialized = false;
        private Win3D.ModelVisual3D model = null;
        private string linkName = "";

        public URDFLinkVisualizationObject()
        {
            // Figure out a way add this path at runtime.
            var rosWsPaths = new string[]
            {
                 @"D:\ROS",
                 @"C:\opt",
            };
            foreach(var wsPath in rosWsPaths)
            {
                this.recursivePackagePathSearch(wsPath);
            }
        }

        public override void NotifyPropertyChanged(string propertyName)
        {
            if (propertyName == nameof(this.Visible))
            {
                this.UpdateVisibility();
            }
        }

        private Win3D.Matrix3D ToMatrix3D(CoordinateSystem cs)
        {
            return new Win3D.Matrix3D(
                cs.Storage.At(0, 0), cs.Storage.At(1, 0), cs.Storage.At(2, 0), cs.Storage.At(3, 0),
                cs.Storage.At(0, 1), cs.Storage.At(1, 1), cs.Storage.At(2, 1), cs.Storage.At(3, 1),
                cs.Storage.At(0, 2), cs.Storage.At(1, 2), cs.Storage.At(2, 2), cs.Storage.At(3, 2),
                cs.Storage.At(0, 3), cs.Storage.At(1, 3), cs.Storage.At(2, 3), cs.Storage.At(3, 3)
                );
        }

        /// <summary>
        /// Recursively traverse the file structure and find ROS Packages. If found
        /// the path and name is added to the mapping.  
        /// </summary>
        /// <param name="currPath">Current Path to search.</param>
        private void recursivePackagePathSearch(string currPath)
        {
            if (!Directory.Exists(currPath))
            {
                return;
            }
            // get all the files in this path
            var files = Directory.GetFiles(currPath).Select(n => Path.GetFileName(n));
            // Check if its a ROS Package
            if (files.Contains("package.xml"))
            {
                // TODO: We should parse the xml instead of just assuming it is.
                // get the package name
                var packageName = Path.GetDirectoryName(currPath);
                this.packageMapping[packageName] = currPath;
                return;
            }
            // recursively traverse the sub paths.
            foreach(var subPath in Directory.GetDirectories(currPath))
            {
                this.recursivePackagePathSearch(subPath);
            }
            
        }

        private string resolvePackageName(string fileName)
        {

            if (fileName.StartsWith("package://"))
            {
                var name = fileName.Substring(10).Split('/');
                var packageName = name[0];
                var realPath = this.packageMapping[packageName];
                return Path.Combine(realPath, Path.Combine(name.Skip(1).ToArray()));
            }
            return fileName;
        }

        public string LinkName { get => this.linkName; }

        private Color toColor(double[] rgba)
        {
            return Color.FromArgb((byte)Convert.ToUInt16(rgba[3] * 255), 
                (byte)Convert.ToUInt16(rgba[0] * 255),
                (byte)Convert.ToUInt16(rgba[1] * 255),
                (byte)Convert.ToUInt16(rgba[2] * 255));
        }

        public override void UpdateData()
        {
            if (this.CurrentData.Item2 != default)
            {
                var (cs, link) = this.CurrentData;
                if (this.model == null && !this.initialized)
                {
                    this.linkName = link.name;
                    foreach (var visual in link.visuals)
                    {
                        var visualTransform = new CoordinateSystem();
                        if (visual.origin != null)
                        {
                            var transform = SpatialExtensions.ConstructCoordinateSystem(
                                visual.origin.Xyz[0],
                                visual.origin.Xyz[1],
                                visual.origin.Xyz[2],
                                visual.origin.Rpy[0],
                                visual.origin.Rpy[1],
                                visual.origin.Rpy[2]);
                            visualTransform = visualTransform.TransformBy(transform);
                        }
                        // first figure out if they have color
                        Material linkMat = null;
                        if (visual?.material?.color != null)
                        {
                            linkMat = MaterialHelper.CreateMaterial(toColor(visual.material.color.rgba));
                        }

                        if (visual?.geometry?.mesh != null && visual.geometry.mesh.filename != "")
                        {
                            var fileName = visual.geometry.mesh.filename;
                            if (fileName.ToLower().EndsWith(".dae"))
                            {
                                // change the extenstion to STL
                                fileName = Path.ChangeExtension(fileName, "STL");
                            }
                            var filePath = this.resolvePackageName(fileName);
                            if (fileName.ToLower().EndsWith(".stl") && File.Exists(filePath))
                            {
                                HelixToolkit.Wpf.StLReader reader = new HelixToolkit.Wpf.StLReader();
                                var stlModel = reader.Read(filePath);
                                // now we try to add color
                                if (linkMat != null)
                                {
                                    foreach (GeometryModel3D child in stlModel.Children)
                                    {
                                        child.Material = linkMat;
                                        child.BackMaterial = linkMat;
                                    }
                                }
                                stlModel.Transform = new Win3D.MatrixTransform3D(this.ToMatrix3D(visualTransform));
                                this.model = new ModelVisual3D();
                                this.model.Content = stlModel;
                            }
                        }
                        if (visual.geometry.box != null)
                        {
                            var model = new BoxVisual3D();
                            model.Length = visual.geometry.box.size[0];
                            model.Width = visual.geometry.box.size[1];
                            model.Height = visual.geometry.box.size[2];
                            if (linkMat != null)
                            {
                                model.Material = linkMat;
                            }
                            model.Transform = new Win3D.MatrixTransform3D(this.ToMatrix3D(visualTransform));
                            this.model = model;
                        }
                    }
                    this.initialized = true;
                }
                if (this.model != null)
                {
                    this.model.Transform = new Win3D.MatrixTransform3D(this.ToMatrix3D(cs));
                    this.UpdateChildVisibility(this.model, this.Visible);
                }
            }
        }

        private void UpdateVisibility()
        {
            if (model != null)
            {
                this.UpdateChildVisibility(this.model, this.Visible);
            }
        }
    }
}
