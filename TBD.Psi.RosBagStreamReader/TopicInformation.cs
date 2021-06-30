

namespace TBD.Psi.RosBagStreamReader
{
    using Microsoft.Psi;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    public class TopicInformation
    {
        public TopicInformation(Dictionary<string, byte[]> header, byte[] data)
        {
            // generate all the fields
            var fields = Helper.ParseHeaderData(data);

            this.Name = Encoding.UTF8.GetString(header["topic"]);
            this.PublishedName = this.Name;
            if (fields.ContainsKey("topic"))
            {
                this.PublishedName = Encoding.UTF8.GetString(fields["topic"]);
            }
            
            this.Type = Encoding.UTF8.GetString(fields["type"]);
            this.Md5Sum = Encoding.UTF8.GetString(fields["md5sum"]);
            this.typeDefinitionText = Encoding.UTF8.GetString(fields["message_definition"]);
            // decode the type definition text
            this.TopicDependencyTable = this.ParseMessageTextDefinition(this.Name, this.typeDefinitionText);
            this.TopicFields = this.TopicDependencyTable[this.Name];


            if (fields.ContainsKey("callerid"))
            {
                this.CallerId = Encoding.UTF8.GetString(fields["callerid"]);
            }

            if (fields.ContainsKey("latching"))
            {
                this.Latching = BitConverter.ToBoolean(fields["latching"], 0);
            }
        }

        // Name of the Topic
        public string Name;

        // Type of ROS Message
        public string Type;

        // The timestamp from the first message
        public DateTime StartTime = DateTime.MaxValue;

        // The timestamp from the last message
        public DateTime EndTime = DateTime.MinValue;

        // Number of messages
        public int MessageCount = 0;

        // Tuple of name and assembly type of the fields.
        public List<(string name, string type)> TopicFields;

        // Table of known topic definitions from the text.
        public Dictionary<string, List<(string, string)>> TopicDependencyTable;

        // topic connection Ids. The first is the id of the bag
        public Dictionary<int, List<int>> ConnectionIds = new Dictionary<int, List<int>>();

        // A dictionary that stores the list of chunk position for each bags. 
        // The key is the index to the bag list.
        public Dictionary<int , List<long>> ChunkPointerList = new Dictionary<int, List<long>>();

        public int readCounter = 0;
        public int bagIndex = 0;
        public int ChunkIndex = 0;
        public int ChunkMsgIndex = 0;
        public int sourceId;

        // type of message
        public string typeDefinitionText;

        // published name
        public string PublishedName;
        public string Md5Sum { get; private set; }
        public string CallerId { get; private set; } = String.Empty;
        public bool? Latching { get; private set; } = null;
        
        public MsgDeserializer deserializer;

        internal List<(string, string)> ParseIndividualDefinitionText(IEnumerable<string> sentences)
        {
            var definitionPair = new List<(string, string)>();
            foreach(var rawSentence in sentences)
            {
                // trim the sentence.
                var sentence = rawSentence.Trim();
                // If the sentence starts with a '#' or is empty then move onto next line
                if (sentence.StartsWith("#") || sentence.Length == 0)
                {
                    continue;
                }
                // split the sentence by space and ignore all items that are only spaces.
                var sentenceArr = sentence.Split(' ').Where(m => m.Length > 0).ToArray();
                // check if this sentence is an constant
                bool valid = true;
                foreach(var s in sentenceArr)
                {
                    // ignore if its a comment
                    if (s.Contains('#'))
                    {
                        break;
                    }
                    // if there is an equal sign showing up before any comments
                    // it is constant and should be ignored.
                    if (s.Contains('='))
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid)
                {
                    continue;
                }
                definitionPair.Add((sentenceArr[1],sentenceArr[0]));
            }
            return definitionPair;
        }

        internal Dictionary<string, List<(string, string)>> ParseMessageTextDefinition(string msgName, string definitionText)
        {
            // create the default object
            var result = new Dictionary<string, List<(string, string)>>();

            // parse the message
            string definitionSplit = "================================================================================";
            var sentences = new List<string>(definitionText.Split('\n'));
            if (sentences.Contains(definitionSplit))
            {
                // This means there are multiple definitions
                // The first one is the definition of this message type
                result[msgName] = this.ParseIndividualDefinitionText(sentences.Take(sentences.IndexOf(definitionSplit)).ToList());
                sentences.RemoveRange(0, sentences.IndexOf(definitionSplit) + 1);

                // loops through all the dependencies definitions
                while (sentences.Contains(definitionSplit))
                {
                    var subSentences = sentences.Take(sentences.IndexOf(definitionSplit)).ToList();
                    // the first line give us the type name
                    var name = subSentences[0].Substring(5, subSentences[0].Length - 5);
                    // parse the definition text
                    result[name] = this.ParseIndividualDefinitionText(subSentences.Skip(1));
                    // remove the parsed text.
                    sentences.RemoveRange(0, sentences.IndexOf(definitionSplit) + 1);
                }
            }
            else
            {
                result[msgName] = this.ParseIndividualDefinitionText(sentences);
            }   

            return result;

        }

    }
}
