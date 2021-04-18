
namespace TBD.Psi.TransformationTree
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Psi;
    using Microsoft.Psi.Components;
    using MathNet.Spatial.Euclidean;

    public class TransformationTreeComponent : Timer, IProducer<TransformationTree<string>>
    {
        private Pipeline pipeline;
        private TransformationTree<string> tree;
        private int linkTotal = 0;

        public Emitter<TransformationTree<string>> Out { get; private set; }

        public TransformationTreeComponent(Pipeline p, uint publishInterval, TransformationTree<string> tree)
            : base(p, publishInterval)
        {
            this.pipeline = p;
            this.tree = tree;
            this.Out = p.CreateEmitter<TransformationTree<string>>(this, nameof(this.Out));
        }

        public TransformationTreeComponent(Pipeline p, uint publishInterval)
            : this(p, publishInterval, new TransformationTree<string>())
        {
        }

        public TransformationTreeComponent(Pipeline p, uint publishInterval, string pathToJSONFile)
            : this(p, publishInterval, TransformationTreeJSONParser.ParseJSONFile(pathToJSONFile))
        {
        }

        public bool UpdateTransformation(string parentKey, string childKey, CoordinateSystem transform)
        {
            return this.tree.UpdateTransformation(parentKey, childKey, transform);
        }

        public CoordinateSystem QueryTransformation(string parentKey, string childKey)
        {
            return this.tree.QueryTransformation(parentKey, childKey);
        }

        public void AddTransformationUpdateLink(IProducer<(string parentKey, string childKey, CoordinateSystem transform)> producer)
        {
            this.linkTotal++;
            var receiverName = $"receiver-{this.linkTotal}";
            var receiver = this.pipeline.CreateReceiver<(string parentKey, string childKey, CoordinateSystem transform)>(this, (m, e) =>
            {
                this.tree.UpdateTransformation(m.parentKey, m.childKey, m.transform);
            }, receiverName);
            producer.PipeTo(receiver);
        }

        protected override void Generate(DateTime absoluteTime, TimeSpan relativeTime)
        {
            this.Out.Post(this.tree, absoluteTime);
        }
    }
}
