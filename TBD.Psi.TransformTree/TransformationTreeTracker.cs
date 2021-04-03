using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using Microsoft.Psi;
using System;
using System.Collections.Generic;
using System.Text;

namespace TBD.Psi.TransformTree
{
    using System.IO;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Components;
    using Newtonsoft.Json;


    internal class TreeInnerJSONObject
    {
        [JsonProperty(PropertyName = "parent")]
        public string ParentName { get; set; }

        [JsonProperty(PropertyName = "child")]
        public string ChildName { get; set; }

        [JsonProperty(PropertyName = "matrix")]
        public double[,] TransformMatrix { get; set; }

    }

    internal class TreeJSONObject
    {
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "transformations")]
        public TreeInnerJSONObject[] Transformations { get; set; }
    }

    public class TransformationTreeTracker : TransformationTree<string>, ISourceComponent
    {
        private Pipeline p;
        private System.Threading.Timer timer;
        private TimeSpan rate;

        private void timerCallback(object state)
        {
            var frameList = new List<(string, CoordinateSystem)>();
            frameList.Add(("world", new CoordinateSystem()));
            this.traverseTree("world", new CoordinateSystem(), frameList);
            this.WorldFrameOutput.Post(frameList.Select(m => m.Item2).ToList(), this.p.GetCurrentTime());
        }

        public TransformationTreeTracker(Pipeline p, double seconds = 1, string pathToSettings = null)
            : base()
        {
            this.p = p;
            this.rate = TimeSpan.FromSeconds(seconds);
            this.WorldFrameOutput = p.CreateEmitter<List<CoordinateSystem>>(this, nameof(this.WorldFrameOutput));
            if (pathToSettings != null)
            {
                this.ReadFromFile(pathToSettings);
            }
        }

        public void ReadFromFile(string pathToFile)
        {
            var treeObject = JsonConvert.DeserializeObject<TreeJSONObject>(File.ReadAllText(pathToFile));
            // check version
            if (treeObject.Version != "0.0.1")
            {
                throw new NotSupportedException("Version Incorrect");
            }
            foreach(var mapping in treeObject.Transformations)
            {
                var cs = new CoordinateSystem(Matrix<double>.Build.DenseOfArray(mapping.TransformMatrix));
                this.UpdateTransformation(mapping.ParentName, mapping.ChildName, cs);
            }
        }

        public Emitter<List<CoordinateSystem>> WorldFrameOutput { private set; get; }


        public void Start(Action<DateTime> notifyCompletionTime)
        {
            // start the timer that generators the output
            this.timer = new System.Threading.Timer(this.timerCallback, null, TimeSpan.Zero, this.rate);
            notifyCompletionTime.Invoke(DateTime.MaxValue);
        }

        public void Stop(DateTime finalOriginatingTime, Action notifyCompleted)
        {
            this.timer.Dispose();
            notifyCompleted();
        }
    }
}
