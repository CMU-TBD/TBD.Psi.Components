

namespace TBD.Psi.RosBagTool
{
    using CommandLine;
    using System.Collections.Generic;

    internal class Verbs
    {
        [Verb("convert", HelpText = "Convert the ROS Bag into a PsiStore")]
        internal class ConvertOptions
        {
            [Option('f', "file", Required = true, HelpText = "Path to the First RosBag")]
            public string Input { get; set; }

            [Option('o', "output", Required = true, HelpText = "Path to where to store PsiStore")]
            public string Output { get; set; }

            [Option('n', "name", Required = true, HelpText = "Name of the PsiStores")]
            public string Name { get; set; }

            [Option('h', HelpText = "Whether to use header time (default = false)")]
            public bool useHeaderTime { get; set; } = false;

            [Option('t', "topics", HelpText = "List of topics to be included when converting to PsiStore")]
            public IEnumerable<string> IncludedTopics { get; set; }

            [Option('x', "topics", HelpText = "List of topics to be excluded when converting to PsiStore")]
            public IEnumerable<string> ExcludedTopics { get; set; }

        }
    }
}
