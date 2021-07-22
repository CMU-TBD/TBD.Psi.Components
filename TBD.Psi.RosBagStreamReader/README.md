# TBD.Psi.RosBagStreamReader
COPYRIGHT(C) 2021 - Transportation, Bots, and Disability Lab - CMU  
Code released under MIT.

## Funding
This work is partially funded under a grant from the National Institute on Disability, Independent Living, and Rehabilitation Research (NIDILRR grant number 90DPGE0003). NIDILRR is a Center within the Administration for Community Living (ACL), Department of Health and Human Services (HHS). The contents of this website do not necessarily represent the policy of NIDILRR, ACL, HHS, and you should not assume endorsement by the Federal Government.

## Overview
This Psi components enable you to read from any ROSBAG 2.0 formats. The component reads the bag and use predefined deserializers to decode the message into Psi formats. List of supported ROS Message types are listed below, you can also easily add message type using the `MsgDeserializer.cs` as base class. After adding reference to this package, you can read ROS bags following the Psi pattern
```csharp
using (var p = Pipeline.Create())
{
    var store = RosBagStore.Open(p,"test-01.bag", @"C:\store-folder");
    var imageStream = store.OpenStream<Shared<Image>>("image_raw");
}
```
The component can handle ROS bag splited into multiple files, but they have to be by themselves and have the same prefixes. 

## How to Install
### For using in PsiStudio
1. Clone this repository
2. Build the `TBD.Psi.RosBagStreamReader.Windows` project.
    - In Visual Studio, hover over the project in the solution explorer, right-click on the project, and build.
3. Find the path for the built dll.
    - The path should look something like this `C:\Users\Zhi\source\repos\CMU-TBD\TBD.Psi.Components\TBD.Psi.RosBagStreamReader.Windows\bin\Debug\net472\TBD.Psi.RosBagStreamReader.Windows.dll`
4. Add the dll as an `AdditionalAssemblies` in the `PsiStudioSettings.xml`. More detail here: [3rd Part Visualizer](https://github.com/microsoft/psi/wiki/3rd-Party-Visualizers)
    - If the file doesn't exist, start and quit PsiStudio once to generate it.
### To use it in application
1. Clone this repository
2. Reference either the .NET 5.0 version (`TBD.Psi.RosBagStreamReader.NET`) for Linux/Mac applications OR .NET framework (`TBD.Psi.RosBagStreamReader.Windows`) for Windows applications.
P

## Supported Messages
| ROS Message Type               | Deserialized Psi/C# Types                                                | Name Match | Windows/Linux/Mac Support | Notes                                                                      |
| ------------------------------ | ------------------------------------------------------------------------ | ---------- | ------------------------- | -------------------------------------------------------------------------- |
| std_msgs/String                | `string`                                                                 |            | All                       |                                                                            |
| std_msgs/Bool                  | `bool`                                                                   |            | All                       |                                                                            |
| std_msgs/UInt8                 | `Byte`                                                                   |            | All                       |                                                                            |
| std_msgs/Int8                  | `SByte`                                                                  |            | All                       |                                                                            |
| std_msgs/UInt16                | `UInt16`                                                                 |            | All                       |                                                                            |
| std_msgs/Int16                 | `Int16`                                                                  |            | All                       |                                                                            |
| std_msgs/UInt32                | `uint`                                                                   |            | All                       |                                                                            |
| std_msgs/Int32                 | `int`                                                                    |            | All                       |                                                                            |
| std_msgs/UInt64                | `UInt64`                                                                 |            | All                       |                                                                            |
| std_msgs/Int64                 | `Int64`                                                                  |            | All                       |                                                                            |
| std_msgs/Float32               | `float`                                                                  |            | All                       |                                                                            |
| std_msgs/Float64               | `double`                                                                 |            | All                       |                                                                            |
| std_msgs/Time                  | `DateTime`                                                               |            | All                       |                                                                            |
| std_msgs/Duration              | `TimeSpan`                                                               |            | All                       |                                                                            |
| std_msgs/ColorRGBA             | `double[]`                                                               |            | All                       | Return as a length 4 double array. Values are in the order of R,G,B,and A. |
| sensor_msgs/Image              | `Shared<Image>`                                                          |            | All                       | Only some formats are supported.                                           |
| sensor_msgs/Image              | `Shared<DepthImage>`                                                     | `depth`    | All                       | Only some formats are supported.                                           |
| sensor_msgs/CompressedImage    | `Shared<Image>`                                                          |            | All                       | Only some formats are supported.                                           |
| sensor_msgs/CompressedImage    | `Shared<EncodedImage>`                                                          |`compressed`            | Windows                       | Only some formats are supported.                                           |
| sensor_msgs/JointState         | `(string[] name, double[] position, double[] velocity, double[] effort)` |            | All                       |                                                                            |
| geometry_msgs/PoseStamped      | `MathNet.Spatial.Euclidean.CoordinateSystem`                             |            | All                       |                                                                            |
| geometry_msgs/Pose             | `MathNet.Spatial.Euclidean.CoordinateSystem`                             |            | All                       |                                                                            |
| geometry_msgs/Transform        | `MathNet.Spatial.Euclidean.CoordinateSystem`                             |            | All                       |                                                                            |
| geometry_msgs/TransformStamped | `MathNet.Spatial.Euclidean.CoordinateSystem`                             |            | All                       |                                                                            |
| geometry_msgs/Quaternion       | `MathNet.Spatial.Euclidean.Quaternion`                                   |            | All                       |                                                                            |
| geometry_msgs/Vector3          | `MathNet.Spatial.Euclidean.Vector3D`                                     |            | All                       |                                                                            |
| audio_common_msgs/AudioData    | `AudioBuffer`                                                            |            | All                       |                                                                            |

For message types with multiple deserializers (e.g. `sensor/Image`), the application will try to match deserializers by name according to the `Name Match` column first before moving on to the default (no name match deserializers). For example, a ros topic of type `sensor_msgs/Image` with name `depth_image` will be converted into `Shared<DepthImage>` instead of `Shared<Image>`. The name match is **case insensetive**.


<!--                           | tf2/TFmessage                                                            | `tbd.psi.TransformationTree` |                                 | --> 

Contributions for more Deserializers are welcomed!

## Build your own Deserializers
Building your own deserializer for any ROS Message is pretty straightforward.
### Create A Deserializer
1. Create a deserializer with `MsgDeserializer.cs` as its parent. Pass to the parent the type of object it should be converted to (`AudioBuffer` in the example) and the name of the ROS Message it is trying to decode. 
```csharp
    class AudioCommonMsgsAudioDataDeserializer : MsgDeserializer
    {
        public AudioCommonMsgsAudioDataDeserializer()
            : base(typeof(AudioBuffer).AssemblyQualifiedName, "audio_common_msgs/AudioData")
    }
```
2. Implement the ` T Deserialize<T>` method that decodes the byte array into the object you want. The method is given raw bytes for a single message to decode.
```csharp
    public override T Deserialize<T>(byte[] data, ref Envelope envelop)
    {
        return (T) (object) new AudioBuffer(data.Skip(4).ToArray(), WaveFormat.Create16kHz1Channel16BitPcm());
    }
```
You can also change the envelop property based on the ROS Message data. This is used when there is a header to change the message originating time to being the header's time.

ROS Messages are just an array of bytes with the reading location define by the ROS Message Definition. For example, `audio_common_msgs/AudioData` message states it has a `data` with the type `uint8[]`, the byte array is simply an array of `uint8` with the first 4 bytes being the length of the array. A more complex message type like `std_msgs/Header` can be decoded as following
 ```csharp
// sequence
var offset = 0;
var seq = Helper.ReadRosBaseType<uint>(data, out offset, offset); // helper functions returns the next offset.
// time
var seconds = Helper.ReadRosBaseType<uint>(data, out offset, offset);
var nanoSeconds = Helper.ReadRosBaseType<uint>(data, out offset, offset);
// frame_id
var str = Helper.ReadRosBaseType<string>(data, out offset, offset);
 ```
You can find Helper methods to decode base types in `Helper.cs`. Some message deserializers also comes with a strongly-typed `Deserialize` function that allows you to use it to deserialize parts of a message. Here's an example from `geometry_msgs/Transform`
```csharp
public static CoordinateSystem Deserialize(byte[] data, ref int offset)
{
    // get the translation vector
    var translation = GeometrymsgsVector3Deserializer.Deserialize(data, ref offset);
    // get the rotation quaterion
    var quaternion = GeometrymsgsQuaternionDeserializer.Deserialize(data, ref offset);
    // combine both into a coordinate system
    return ConvertToCoordinateSystem(quaternion, translation);
}

public override T Deserialize<T>(byte[] data, ref Envelope env)
{
    // convert to coordinate systems
    int offset = 0;
    var cs = Deserialize(data, ref offset);

    return (T)(object)cs;
}
```
### Add deserializer
Depending on your usecase, you have two options. 

If the deserializer is not framework specific, you can add it straight into the base package. Add the deserializer to the `loadDefaultDeserializers` method in `RosBagReader.cs`. If you want to match it with specific names, you can also pass in an optional topic name.
```csharp
private void loadDefaultDeserializers()
{
    this.AddDeserializer(new StdMsgsStringDeserializer());
    this.AddDeserializer(new SensorMsgsImageDeserializer(), "image");
    this.AddDeserializer(new SensorMsgsImageDeserializer(), "/robot/front_camera/image");
```

If the deserializer is framework specific, you can add it to either `TBD.Psi.RosBagStreamReader.NET` for Linux/Mac and `TBD.Psi.RosBagStreamReader.Windows` for Windows. For those packages. it is added to the constructor of either `RosBagStreamReaderNET` or `RosBagStreamReaderWindows`.
```csharp
    public RosBagReaderNET()
        : base()
        {
            this.AddDeserializer(new UniquelyWindowsDeserializers());
        }
```


## Known Limitations
1. Currently, the deserializers does not handle encrypted ROS Bags.
2. `tf` and `tf_static` will be parsed differently.

## Changelog
#### 2021-07-20
- Split into Windows and .NET 5.
- Remove TF due to it being not stable.
- Cleaned up the code.

#### 2021-06-29
- Added a default generic message reader for unknown class. It tries to read the header and use the header time if possible.
#### 2021-06-24
- Added `Point` and `ColorRGBA`.
- Code cleanup.
#### 2021-06-22
- Added a few direct OpenCV image encodings.
#### 2021-06-21
- Fixed the message count was being set as average message size.
- Cleaned up the parsing of bytes and created templated helper methods for all ros base types.
- display /tf message using TransformationTree.
- published in nuget.