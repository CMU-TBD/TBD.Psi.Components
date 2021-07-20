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

        internal static int GetRosBaseTypeByteLength(string type)
        {
            switch (type)
            {
                case "bool":
                case "uint8":
                case "int8":
                case "char":
                case "byte":
                    return 1;
                case "uint16":
                case "int16":
                    return 2;
                case "uint32":
                case "int32":
                case "float32":
                    return 4;
                case "uint64":
                case "int64":
                case "float64":
                case "time":
                case "duration":
                    return 8;
                case "string":
                    throw new InvalidCastException("Length of string can only be known at runtime.");
                default:
                    throw new InvalidCastException($"{type} is not a ROS primitive type.");
            }
        }

        public static T ReadRosBaseType<T>(byte[] data, out int nextOffset, int offset = 0)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Boolean:
                    var boolVal = BitConverter.ToBoolean(data, offset);
                    nextOffset = offset + 1;
                    return (T)(object)boolVal;
                case TypeCode.Byte:
                    nextOffset = offset + 1;
                    return (T)(object)data[offset];
                case TypeCode.SByte:
                    nextOffset = offset + 1;
                    return (T)(object)(sbyte)data[offset];
                case TypeCode.Int16:
                    var int16Val = BitConverter.ToInt16(data, offset);
                    nextOffset = offset + 2;
                    return (T)(object)int16Val;
                case TypeCode.UInt16:
                    var uInt16Val = BitConverter.ToUInt16(data, offset);
                    nextOffset = offset + 2;
                    return (T)(object)uInt16Val;
                case TypeCode.Int32:
                    var int32Val = BitConverter.ToInt32(data, offset);
                    nextOffset = offset + 4;
                    return (T)(object)int32Val;
                case TypeCode.UInt32:
                    var uInt32Val = BitConverter.ToUInt32(data, offset);
                    nextOffset = offset + 4;
                    return (T)(object)uInt32Val;
                case TypeCode.Int64:
                    var int64Val = BitConverter.ToInt64(data, offset);
                    nextOffset = offset + 8;
                    return (T)(object)int64Val;
                case TypeCode.UInt64:
                    var uInt64Val = BitConverter.ToUInt64(data, offset);
                    nextOffset = offset + 8;
                    return (T)(object)uInt64Val;
                case TypeCode.Single:
                    var floatVal = BitConverter.ToSingle(data, offset);
                    nextOffset = offset + 4;
                    return (T)(object)floatVal;
                case TypeCode.Double:
                    var doubleVal = BitConverter.ToDouble(data, offset);
                    nextOffset = offset + 8;
                    return (T)(object)doubleVal;
                case TypeCode.String:
                    // get length
                    var strlen = (int)BitConverter.ToUInt32(data, offset);
                    // get the string
                    var str = Encoding.UTF8.GetString(data, offset + 4, strlen);
                    nextOffset = offset + 4 + strlen;
                    return (T)(object)str;
                case TypeCode.DateTime:
                    var seconds = BitConverter.ToUInt32(data, offset);
                    var nanoSeconds = BitConverter.ToUInt32(data, offset + 4);
                    nextOffset = offset + 8;
                    return (T)(object)(DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime + TimeSpan.FromTicks(nanoSeconds / 100));
                case TypeCode.Object:
                    if (typeof(T) == typeof(TimeSpan))
                    {
                        var durationSeconds = BitConverter.ToInt32(data, offset);
                        var durationNanoSeconds = BitConverter.ToInt32(data, offset + 4);
                        nextOffset = offset + 8;
                        return (T)(Object)(TimeSpan.FromSeconds(durationSeconds) + TimeSpan.FromTicks(durationNanoSeconds / 100));
                    }
                    throw new InvalidCastException($"Unable to convert to {typeof(T)}");
                default:
                    throw new InvalidCastException($"Unable to convert to {typeof(T)}");
            }
        }

        public static T ReadRosBaseType<T>(byte[] data, int offset = 0)
        {
            return ReadRosBaseType<T>(data, out int temp, offset);
        }

        public static T[] ReadRosBaseTypeArray<T>(byte[] data, out int nextOffset, int offset = 0)
        {
            // get the length of array 
            var length = BitConverter.ToUInt32(data, offset);
            nextOffset = offset + 4;
            var arr = new T[length];
            // parse each item.
            for(var i = 0; i < length; i++)
            {
                arr[i] = ReadRosBaseType<T>(data, out nextOffset, nextOffset);
            }
            return arr;
        }

        public static T[] ReadRosBaseTypeArray<T>(byte[] data, int offset = 0)
        {
            return ReadRosBaseTypeArray<T>(data, out int temp, offset);
        }
        

        internal static (uint, DateTime, string) ReadStdMsgsHeader(byte[] data, out int nextOffset, int offset = 0)
        {
            var seq = ReadRosBaseType<uint>(data, out nextOffset, offset);
            var originTime = ReadRosBaseType<DateTime>(data, out nextOffset, nextOffset);
            var frameId = ReadRosBaseType<string>(data, out nextOffset, nextOffset);
            return (seq, originTime, frameId);
        }
    }
}