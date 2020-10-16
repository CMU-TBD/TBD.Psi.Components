# TBD.Psi.RosBagStreamReader
COPYRIGHT(C) 2020 - Transportation, Bots, and Disability Lab - CMU
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

You can also add the `dll` to PsiStudio to enable direct reading of ROSBAGs in PsiStudio. Add the path to the built dll under `AdditionalAssemblies` in `PsiStudioSettings.xml`.

## Supported Messages
|ROS Message Type | Deserialized Psi Types|Notes|
|---|---|---|
|std_msgs/String| `string` ||
|std_msgs/Bool| `bool` ||
|sensor_msgs/Image| `Shared<Image>` |Only some formats are supported|
|sensor_msgs/CompressedImage| `Shared<Image>` |Only some formats are supported|
|geometry_msgs/PoseStamped| `Coordinate Systems` ||
|audio_common_msgs/AudioData| `AudioBuffer` ||

Contributions for more Deserializers are welcomed!

## Build your own Deserializers
Building your own deserializer for any ROS Message is pretty straightforward.
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
var seq = BitConverter.ToUInt32(data, offset);
offset += 4;
// time
var seconds = BitConverter.ToUInt32(timeBytes, offset);
offset += 4;
var nanoSeconds = BitConverter.ToUInt32(timeBytes, offset);
offset += 4;
// frame_id
// get string length
var strlen = (int)BitConverter.ToUInt32(data, offset);
// get the string
var str = Encoding.UTF8.GetString(data, offset + 4, strlen);
 ```
You can find Helper methods to decode in `Helper.cs`.

3. Add the deserializer to the `loadDeserializers` method in `RosBagReader.cs`. We are looking into automating this in the future.
```csharp
private void loadDeserializers()
{
    this.loadDeserializer(new StdMsgsStringDeserializer());
```

## Known Limitations
1. Currently, the deserializers does not handle encrypted ROS Bags.
