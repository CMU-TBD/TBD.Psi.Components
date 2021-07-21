

namespace TBD.Psi.RosBagTool
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Psi;
    using TBD.Psi.RosBagStreamReader;
    using CommandLine;
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("RosBag to PsiStore Converter");

                Parser.Default.ParseArguments<Verbs.ConvertOptions>(args)
                    .MapResult(
                        (Verbs.ConvertOptions opts) => ConvertBag(opts),
                        errs => 1
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
            }
        }

        private static int ConvertBag(Verbs.ConvertOptions opts)
        {
            using (var p = Pipeline.Create())
            {
                // Open rosbag store
                var name = Path.GetFileName(opts.Input);
                var path = Path.GetDirectoryName(opts.Input);
                var rosbag = RosBagStore.Open(p, name, path);
                // new PsiStore 
                var output = PsiStore.Create(p, opts.Name, opts.Output);
                // copy streams 
                foreach(var stream in rosbag.AvailableStreams)
                {
                    if (opts.ExcludedTopics.Contains(stream.Name))
                    {
                        continue;
                    }
                    if (opts.IncludedTopics.Count() > 0 && !opts.IncludedTopics.Contains(stream.Name))
                    {
                        continue;
                    }
                    // copy the stream
                }
                // run pipeline
                p.Run();
            };
            return 1;


            // for all topic 


            
            
            
            // var watch = System.Diagnostics.Stopwatch.StartNew();
            // var bag = new RosBag(opts.Input.ToList());
            // var topicList = opts.Topics.Count() > 0 ? opts.Topics : bag.TopicList;

            // TimeSpan offset = TimeSpan.Zero;
            // // check if we should should restamp the time as current time.
            // if (opts.RestampTime)
            // {
            //     // This is a hack for now, we don't know the actual pipeline startime until it start
            //     // This is obviously break if the pipeline starts later etc.
            //     // One idea is to pass the bag.StartTime around, but this might be good enough.
            //     offset = DateTime.UtcNow - bag.StartTime;
            // }

            // // create a psi store
            // using (var pipeline = Pipeline.Create(true)) //enable diagnostic
            // {
            //     // create a 


            //     var store = 

            //     // We cannot write the diagnostics information because PsiStudio cannot
            //     // handle weirdly out of wack times.
            //     // store.Write(pipeline.Diagnostics, "ReaderDiagnostics");

            //     var dynamicSerializer = new DynamicSerializers(bag.KnownRosMessageDefinitions, opts.useHeaderTime, offset, useCustomSerializer: opts.useCustomSerializer);
            //     foreach (var topic in topicList)
            //     {
            //         if (opts.ExcludeTopic.Contains(topic))
            //         {
            //             continue;
            //         }
            //         var messageDef = bag.GetMessageDefinition(topic);
            //         var messages = bag.ReadTopic(topic);
            //         dynamicSerializer.SerializeMessages(pipeline, store, topic, messageDef.Type, messages);
            //     }
            //     pipeline.Run();
            // }
            // watch.Stop();
            // Console.WriteLine($"Total elapsed time:{watch.ElapsedMilliseconds / 1000.0} secs");

            // return 1;
        }

/*        private static int InfoOnBag(Verbs.InfoOption opts)
        {
            // create bag object
            var bag = new RosBag(opts.Input.ToList());

            Console.WriteLine("---------------------");
            Console.WriteLine("Info for Bags");
            Console.WriteLine("---------------------");
            Console.WriteLine($"Earliest Message Time:{bag.StartTime}");
            Console.WriteLine($"Latest Message Time:{bag.EndTime}");
            Console.WriteLine(string.Format("{0,-50}{1,-30}{2, -20}", "Name", "Type", "Counts"));
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            var messageCounter = bag.MessageCounts;
            foreach (var topic in bag.TopicTypeList)
            {
                Console.WriteLine(string.Format("{0,-50}{1,-30}{2,-20}", topic.Item1.Substring(0, Math.Min(50, topic.Item1.Length)), topic.Item2, messageCounter[topic.Item1]));
            }
            return 1;
        }*/
    }
}