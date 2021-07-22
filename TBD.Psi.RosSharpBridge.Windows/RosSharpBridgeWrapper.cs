
namespace TBD.Psi.RosSharpBridge
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Psi;
    using Microsoft.Psi.Components;
    using RosSharp.RosBridgeClient;

    public class RosSharpBridge : ISourceComponent
    {
        private Pipeline pipeline;
        private bool started = false;
        private RosSocket rosSocket;
        private List<string> subscriptionIds = new List<string>();
        public RosSharpBridge(Pipeline p, string bridge_ws_uri)
        {
            this.pipeline = p;
            rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(bridge_ws_uri));
        }

        public static DateTime ConvertStampToUTCDateTime(RosSharp.RosBridgeClient.MessageTypes.Std.Time msg)
        {
            return DateTimeOffset.FromUnixTimeSeconds(msg.secs).UtcDateTime + TimeSpan.FromTicks(msg.nsecs / 100);
        }

        public void Start(Action<DateTime> notifyCompletionTime)
        {
            notifyCompletionTime.Invoke(DateTime.MaxValue);
            this.started = true;
        }

        public void Stop(DateTime finalOriginatingTime, Action notifyCompleted)
        {
            this.subscriptionIds.ForEach(id => this.rosSocket.Unsubscribe(id));
            notifyCompleted.Invoke();
        }

        public Receiver<T> Publisher<T>(string topicName) where T : RosSharp.RosBridgeClient.Message
        {
            var publisherId = this.rosSocket.Advertise<T>(topicName);
            var receiver = this.pipeline.CreateReceiver<T>(this, (m,e) =>
            {
                this.rosSocket.Publish(publisherId, m);
            }, topicName);
            return receiver;
        }

        public Emitter<T> Subscribe<T>(string topicName) where T : RosSharp.RosBridgeClient.Message
        {
            var emitter = this.pipeline.CreateEmitter<T>(this, topicName);
            this.subscriptionIds.Add(rosSocket.Subscribe<T>(topicName, m =>
            {
                if (started)
                {
                    emitter.Post(m, this.pipeline.GetCurrentTime());
                }
            }));
            return emitter;
        }
    }
}
