using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TBD.Psi.RosBagStreamReader
{
    public class Helper
    {
        /// <summary>
        /// Validate whether the given filestream is a ROS Bag or not
        /// </summary>
        /// <param name="fs"></param>
        internal static void validateRosBag(FileStream fs)
        {
            byte[] headerBytes = new byte[13];
            // get the first 13 bytes and see whether it's the header
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(headerBytes, 0, 13);
            // get the version of the ROS BAG
            var rosbagVersion = Encoding.UTF8.GetString(headerBytes);
            if (rosbagVersion.Trim() != "#ROSBAG V2.0")
            {
                throw new NotImplementedException(String.Format("Unable to Handle ROSBAG Version {0}", rosbagVersion));
            }
        }

        /// <summary>
        /// Read the next record and return the offset of the beginning of the data and also datalen
        /// </summary>
        /// <param name="bagStream">Bag filestream to read</param>
        /// <param name="offset">Where the record is in the stream</param>
        /// <returns>A tuple of the header, offset to the data and length of the data</returns>
        internal static (Dictionary<string, byte[]>, long, int) ReadNextRecord(FileStream bagStream, long offset)
        {
            // reinitialize the filestream header from the beginning to the header.
            bagStream.Seek(offset, SeekOrigin.Begin);

            // read and parse the header
            var recordFieldProperties = ReadRecordHeader(bagStream);

            // now we read the datalen
            byte[] intBytes = new byte[4];
            bagStream.Read(intBytes, 0, 4);
            var recordDataLen = BitConverter.ToInt32(intBytes, 0);

            // Return all the information
            return (recordFieldProperties, bagStream.Position, recordDataLen);
        }

        /// <summary>
        /// Read the the header that is located at the current reading point of the bagstream.
        /// This operations changes the reading head of the File stream.
        /// </summary>
        /// <param name="bagStream">Filestream to read from.</param>
        /// <returns></returns>
        internal static Dictionary<string, byte[]> ReadRecordHeader(FileStream bagStream)
        {
            // read the headerlen
            byte[] intBytes = new byte[4];
            bagStream.Read(intBytes, 0, 4);
            var recordHeaderLen = BitConverter.ToInt32(intBytes, 0);

            // now create bit array for the header
            byte[] headerDataBytes = new byte[recordHeaderLen];
            bagStream.Read(headerDataBytes, 0, recordHeaderLen);

            // parse & return the header field
            return ParseHeaderData(headerDataBytes);
        }

        internal static Dictionary<string, byte[]> ParseHeaderData(byte[] headerBytes)
        {
            var fieldProperties = new Dictionary<string, byte[]>();

            // TODO: Rewrite this to use offsets instead of copying arrays.
            while (headerBytes.Length > 0)
            {
                var fieldLen = BitConverter.ToInt32(headerBytes, 0);
                var fieldValuePair = headerBytes.Skip(4).Take(fieldLen).ToArray();
                int cutoffIndex = Array.IndexOf(fieldValuePair, (byte)61);
                var fieldName = Encoding.UTF8.GetString(fieldValuePair, 0, cutoffIndex);
                fieldProperties.Add(fieldName, fieldValuePair.Skip(cutoffIndex + 1).ToArray());

                headerBytes = headerBytes.Skip(4 + fieldLen).ToArray();
            }
            return fieldProperties;
        }

        internal static int PostHeaderPosition(byte[] data , int offset = 0)
        {
            // first 4 bytes is Uint32 sequence
            // next 8 bytes is time.
            // then it's the frame_id string
            return (int)BitConverter.ToUInt32(data, 12 + offset) + 16 + offset;
        }

        internal static DateTime FromBytesToDateTime(byte[] timeBytes, int offset = 0)
        {
            var seconds = BitConverter.ToUInt32(timeBytes, offset);
            var nanoSeconds = BitConverter.ToUInt32(timeBytes, offset + 4);

            return DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime + TimeSpan.FromTicks(nanoSeconds / 100);
        }
    }
}