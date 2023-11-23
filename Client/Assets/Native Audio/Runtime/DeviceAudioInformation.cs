using System.Linq;
using System.Runtime.InteropServices;
#if UNITY_IOS
using System;
#endif
#if UNITY_ANDROID
using UnityEngine;
#endif

namespace E7.Native
{
    /// <summary>
    ///     <para>
    ///         Several properties about the device asked from the native side that might help you.
    ///         Returned from <see cref="NativeAudio.GetDeviceAudioInformation"/>
    ///     </para>
    ///     <para>
    ///         The content of this <c>struct</c> changes completely depending on active build platform.
    ///         You will want to use a preprocessor directive wrapping it.
    ///     </para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceAudioInformation
    {
#if UNITY_IOS
        /// <summary>
        ///     It is from
        ///     <a href="https://developer.apple.com/documentation/avfoundation/avaudiosessionportdescription">
        ///         <c>AVAudioSessionPortDescription</c>
        ///     </a>
        ///     .
        /// </summary>
        public enum IosAudioPortType
        {
            //---Output---

            /// <summary>
            ///     Line-level output to the dock connector.
            /// </summary>
            LineOut = 0,

            /// <summary>
            ///     Output to a wired headset.
            /// </summary>
            Headphones = 1,

            /// <summary>
            ///     Output to a speaker intended to be held near the ear.
            /// </summary>
            BuiltInReceiver = 2,

            /// <summary>
            ///     Output to the device’s built-in speaker.
            /// </summary>
            BuiltInSpeaker = 3,

            /// <summary>
            ///     Output to a device via the High-Definition Multimedia Interface (HDMI) specification.
            /// </summary>
            HDMI = 4,

            /// <summary>
            ///     Output to a remote device over AirPlay.
            /// </summary>
            AirPlay = 5,

            /// <summary>
            ///     Output to a Bluetooth Low Energy (LE) peripheral.
            /// </summary>
            BluetoothLE = 6,

            /// <summary>
            ///     Output to a Bluetooth A2DP device.
            /// </summary>
            BluetoothA2DP = 7,

            //---Input---

            /// <summary>
            ///     Line-level input from the dock connector.
            /// </summary>
            LineIn = 8,

            /// <summary>
            ///     The built-in microphone on a device.
            /// </summary>
            BuiltInMic = 9,

            /// <summary>
            ///     A microphone that is built-in to a wired headset.
            /// </summary>
            HeadsetMic = 10,

            //---Input-Output---

            /// <summary>
            ///     Input or output on a Bluetooth Hands-Free Profile device.
            /// </summary>
            BluetoothHFP = 11,

            /// <summary>
            ///     Input or output on a Universal Serial Bus device.
            /// </summary>
            UsbAudio = 12,

            /// <summary>
            ///     Input or output via Car Audio.
            /// </summary>
            CarAudio = 13,
        }

        /// <summary>
        ///     [iOS] All OUTPUT audio devices currently active.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The returned <c>enum</c> is native, so on an other platform the available choice completely changes.
        ///         Use <c>#if</c> directive to make this property compile on multiple platforms.
        ///     </para>
        ///     <para>
        ///         This is the return value from
        ///         <c>[[AVAudioSession sharedInstance] currentRoute]</c> -> <c>outputs</c> -> each item.
        ///     </para>
        /// </remarks>
        public IosAudioPortType[] audioDevices { get; }

        /// <summary>
        ///     [iOS] The latency for audio output, measured in seconds.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is shared with Unity, not just for Native Audio,
        ///         because <c>AVAudioSession</c> is a singleton and is shared.
        ///     </para>
        ///     <para>
        ///         A result from
        ///         <a href="https://developer.apple.com/documentation/avfoundation/avaudiosession/1616500-outputlatency">
        ///             this <c>AVAudioSession</c> instance property
        ///         </a>
        ///         .
        ///     </para>
        ///     <para>
        ///         For reference on my iPhone SE with device speaker, it is 0.0128750000149012.
        ///         (Regardless of Project Settings > Audio option selected)
        ///     </para>
        ///     <para>
        ///         Value is always specified in seconds; it yields sub-millisecond precision over a range of 10,000 years.
        ///     </para>
        ///     <para>
        ///         I don't know how could iOS know its own latency based on current connected device,
        ///         but I got curious and went to audio shop and debug this number on all the things they let me.
        ///     </para>
        ///     <code>
        /// Hardware Name String    Output Latency      Brand
        /// ---------------------------------------------------------
        /// JBL T110BT	            0.0825396850705147	JBL
        /// JBL Reflect Mini2	    0.0825396850705147	JBL
        /// NW WS-623	            0.0149886617437005	Sony Walkman
        /// JBL E45BT	            0.111564628779888	JBL
        /// MAJOR III BLUETOOTH	    0.149297058582306	Marshall
        /// JBL Flip 4	            0.111564628779888	JBL
        /// JBL GO 2	            0.0825396850705147	JBL
        /// JBL GO	                0.0825396850705147	JBL
        /// JBL JR POP	            0.111564628779888	JBL
        /// JBL Charge 4	        0.111564628779888	JBL
        /// </code>
        ///     <para>
        ///         Looks like the measurement got several suspicious number, like exact same number across devices,
        ///         or even some that looks like 10x of the other.
        ///         So I think it must be calculated from something rather than measured live with fancy technique.
        ///     </para>
        ///     <para>
        ///         How is this useful? You could for example, display a warning in the game that your player's device
        ///         is not suitable to play the game due to high latency.
        ///     </para>
        ///     <para>
        ///         However when I test them, most sounds <b>almost</b> equal in latency (but all are bad for music games anyways),
        ///         but that Marshall MAJOR III  has obviously much higher latency (almost 3x) than others.
        ///         Feels much higher than what you see in the data.
        ///     </para>
        ///     <para>
        ///         So it shows this number is somewhat reliable, but may not be 100% accurate of the real latency.
        ///     </para>
        /// </remarks>
        public double outputLatency { get; }

        /// <summary>
        ///     [iOS] The current audio sample rate, in hertz.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is shared with Unity, not just for Native Audio, because <c>AVAudioSession</c>
        ///         is a singleton and is shared.
        ///     </para>
        ///     <para>
        ///         A result from
        ///         <a href="https://developer.apple.com/documentation/avfoundation/avaudiosession/1616499-samplerate">
        ///             this <c>AVAudioSession</c> instance property
        ///         </a>
        ///         .
        ///     </para>
        ///     <para>
        ///         The available range for hardware sample rate is device dependent. It typically ranges from 8000 through 48000 hertz.
        ///     </para>
        /// </remarks>
        public double sampleRate { get; }

        /// <summary>
        ///     [iOS] The preferred sample rate, in hertz.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is shared with Unity, not just for Native Audio, because <c>AVAudioSession</c>
        ///         is a singleton and is shared.
        ///     </para>
        ///     <para>
        ///         At native side this is freely specifiable, but iOS might give you something else
        ///         that becomes <see cref="sampleRate"/>.
        ///     </para>
        ///     <para>
        ///         A result from
        ///         <a href="https://developer.apple.com/documentation/avfoundation/avaudiosession/1616543-preferredsamplerate">
        ///             this <c>AVAudioSession</c> instance property
        ///         </a>
        ///         .
        ///     </para>
        /// </remarks>
        public double preferredSampleRate { get; }

        /// <summary>
        ///     [iOS] The current I/O buffer duration, in seconds.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         A result from
        ///         <a href="https://developer.apple.com/documentation/avfoundation/avaudiosession/1616498-iobufferduration">
        ///             this <c>AVAudioSession</c> instance property
        ///         </a>
        ///         .
        ///     </para>
        ///     <para>
        ///         This could be viewed as an another representation of "buffer size" on Android.
        ///         This is instead in time unit rather than size. And it depends on the current sample rate in order
        ///         to calculate the resulting buffer size.
        ///     </para>
        ///     <para>
        ///         Value is always specified in seconds; it yields sub-millisecond precision over a range of 10,000 years.
        ///     </para>
        ///     <para>
        ///         The audio I/O buffer duration is the number of seconds for a single audio input/output cycle.
        ///         For example, with an I/O buffer duration of 0.005 s, on each audio I/O cycle:
        ///     </para>
        ///     <para>
        ///         You receive 0.005 s of audio if obtaining input.
        ///         You must provide 0.005 s of audio if providing output.
        ///     </para>
        ///     <para>
        ///         The typical maximum I/O buffer duration is 0.93 s
        ///         (corresponding to 4096 sample frames at a sample rate of 44.1 kHz).
        ///         The minimum I/O buffer duration is at least 0.005 s(256 frames)
        ///         but might be lower depending on the hardware in use.
        ///     </para>
        ///     <para>
        ///         For example if this is 0.01s, at sample rate 24000Hz (What Unity use)
        ///         it would have to get 0.01s of audio. But compared to 44000Hz rate, that
        ///         0.01s of audio is of much less data. By using higher fidelity 44000Hz,
        ///         the same 0.01s could cause buffer underrun if the device is not fast enough.
        ///     </para>
        ///     <para>
        ///         This is shared with Unity, not just for Native Audio, because
        ///         <c>AVAudioSession</c> is a singleton and is shared.
        ///     </para>
        ///     <para>
        ///         Here's some behaviour of this number based on my research.
        ///     </para>
        ///     <para>
        ///         For reference, with varying Project Settings > Audio options :
        ///         <code>
        /// Best Latency     : 0.0106666665524244
        /// Good Latency     : 0.0213333331048489
        /// Best Performance : 0.0426666662096977
        /// </code>
        ///     </para>
        ///     <para>
        ///         When connected to external audio device like a bluetooth headphone, the number on Best Latency
        ///         drops to 0.005 but sampling rate moved from 24000 to 44100.
        ///         This can be interpret as Unity try to make sampling rate compatible with external device,
        ///         but now must write half less audio seconds because it now have twice as many data.
        ///     </para>
        ///     <para>
        ///         By setting <see cref="preferredIOBufferDuration"/> at native side to 0.005
        ///         (the limit mentioned in the documentation) this became 0.005333.
        ///     </para>
        ///     <para>
        ///         When set <see cref="preferredIOBufferDuration"/> back to 0, this became 0.021333.
        ///         (The same as Good Latency, even though Unity is currently in Best Latency.)
        ///         However by benchmarking sometimes 0.005 duration do produce worse latency than 0.01, I wonder why..
        ///     </para>
        /// </remarks>
        public double ioBufferDuration { get; }

        /// <summary>
        ///     [iOS] The preferred I/O buffer duration, in seconds.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         At native side this is freely specifiable, but iOS might give you something else
        ///         that becomes <see cref="ioBufferDuration"/>.
        ///     </para>
        ///     <para>
        ///         This is shared with Unity, not just for Native Audio, because <c>AVAudioSession</c>
        ///         is a singleton and is shared. This seems to be always 0 in Unity games by default.
        ///     </para>
        ///     <para>
        ///         A result from
        ///         <a href="https://developer.apple.com/documentation/avfoundation/avaudiosession/1616464-preferrediobufferduration">
        ///             this <c>AVAudioSession</c> instance property
        ///         </a>
        ///         .
        ///     </para>
        ///     <para>
        ///         Value is always specified in seconds; it yields sub-millisecond precision over a range of 10,000 years.
        ///     </para>
        /// </remarks>
        public double preferredIOBufferDuration { get; }

        internal const int interopArrayLength = 5;

        public DeviceAudioInformation(double[] interopDoubleArray, IosAudioPortType[] portArray)
        {
            if (interopDoubleArray.Length != interopArrayLength)
            {
                throw new ArgumentException("The array that fetched data from iOS should be of length " +
                                            interopArrayLength);
            }

            outputLatency = interopDoubleArray[0];
            sampleRate = interopDoubleArray[1];
            preferredSampleRate = interopDoubleArray[2];
            ioBufferDuration = interopDoubleArray[3];
            preferredIOBufferDuration = interopDoubleArray[4];

            audioDevices = portArray;
        }

        public override string ToString()
        {
            return string.Format(
                "Audio devices : {0} Output Latency : {1} Sample Rate : {2} Preferred Sample Rate : {3} IO Buffer Duration : {4} Preferred IO Buffer Duration : {5}",
                string.Join(", ", audioDevices.Select(x => x.ToString()).ToArray()),
                outputLatency,
                sampleRate,
                preferredSampleRate,
                ioBufferDuration,
                preferredIOBufferDuration
            );
        }
#endif

#if UNITY_ANDROID
        /// <summary>
        ///     [Android] Only audio matching this sampling rate on a native <c>AudioTrack</c> created with this
        ///     sampling rate is eligible for fast track playing.
        /// </summary>
        /// <remarks>
        ///     This is NOT the sample rate that Native Audio might be currently working with,
        ///     nor what Unity is using for their own audio source.
        ///     Just a per-device attribute.
        /// </remarks>
        public int nativeSamplingRate { get; }

        /// <summary>
        ///     [Android] How large of a buffer that your phone wants to work with.
        /// </summary>
        /// <remarks>
        ///     This is NOT the buffer size that Native Audio might be currently working with,
        ///     nor what Unity is using for their own audio source.
        ///     Just a per-device attribute.
        /// </remarks>
        public int optimalBufferSize { get; }

        /// <summary>
        ///     [Android] Indicates a continuous output latency of 45 ms or less.
        ///     Only valid if the device API >= 23 (6.0, Marshmallow), or else it is always <c>false</c>.
        /// </summary>
        public bool lowLatencyFeature { get; }

        /// <summary>
        ///     [Android] Indicates a continuous round-trip latency of 20 ms or less.
        ///     Only valid if the device API >= 23 (6.0, Marshmallow), or else it is always <c>false</c>.
        /// </summary>
        public bool proAudioFeature { get; }

        /// <summary>
        ///     [Android] All OUTPUT devices currently active.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The returned <c>enum</c> is native, so on an other platform the available choice completely changes.
        ///         Use <c>#if</c> directive to make this property compile on multiple platforms.
        ///     </para>
        ///     <para>
        ///         These are returned from <c>audioManager.getDevices(AudioManager.GET_DEVICES_OUTPUTS);</c> API.
        ///         Only valid if the device API >= 23 (6.0, Marshmallow), or else it is always <c>null</c>.
        ///     </para>
        /// </remarks>
        public AndroidAudioDeviceType[] audioDevices { get; }

        /// <summary>
        ///     This is everything from
        ///     <a href="https://developer.android.com/reference/android/media/AudioDeviceInfo.html#constants_2">
        ///         <c>AudioDeviceInfo</c>
        ///     </a>
        ///     .
        /// </summary>
        public enum AndroidAudioDeviceType
        {
            TYPE_AUX_LINE = 19,
            TYPE_BLUETOOTH_A2DP = 8,
            TYPE_BLUETOOTH_SCO = 7,
            TYPE_BUILTIN_EARPIECE = 1,
            TYPE_BUILTIN_MIC = 15,
            TYPE_BUILTIN_SPEAKER = 2,
            TYPE_BUS = 21,
            TYPE_DOCK = 13,
            TYPE_FM = 14,
            TYPE_FM_TUNER = 16,
            TYPE_HDMI = 9,
            TYPE_HDMI_ARC = 10,
            TYPE_HEARING_AID = 23,
            TYPE_IP = 20,
            TYPE_LINE_ANALOG = 5,
            TYPE_LINE_DIGITAL = 6,
            TYPE_TELEPHONY = 18,
            TYPE_TV_TUNER = 17,
            TYPE_UNKNOWN = 0,
            TYPE_USB_ACCESSORY = 12,
            TYPE_USB_DEVICE = 11,
            TYPE_USB_HEADSET = 22,
            TYPE_WIRED_HEADPHONES = 4,
            TYPE_WIRED_HEADSET = 3,
        }

        public DeviceAudioInformation(AndroidJavaObject jo)
        {
            var versionClass = new AndroidJavaClass("android/os/Build$VERSION");
            var sdkLevel = versionClass.GetStatic<int>("SDK_INT");

            nativeSamplingRate = jo.Get<int>("nativeSamplingRate");
            optimalBufferSize = jo.Get<int>("optimalBufferSize");
            lowLatencyFeature = jo.Get<bool>("lowLatencyFeature");
            proAudioFeature = jo.Get<bool>("proAudioFeature");

            if (sdkLevel >= 23)
            {
                //This one is a Java array, we need to do JNI manually to each elements
                var outputDevicesJo = jo.Get<AndroidJavaObject>("outputDevices");

                var outputDevicesRaw = outputDevicesJo.GetRawObject();
                var outputDeviceAmount = AndroidJNI.GetArrayLength(outputDevicesRaw);

                audioDevices = new AndroidAudioDeviceType[outputDeviceAmount];

                for (var i = 0; i < outputDeviceAmount; i++)
                {
                    var outputDevice = AndroidJNI.GetObjectArrayElement(outputDevicesRaw, i);
                    var audioDeviceInfoClass = AndroidJNI.GetObjectClass(outputDevice);
                    var getTypeMethod = AndroidJNIHelper.GetMethodID(audioDeviceInfoClass, "getType");
                    var type = AndroidJNI.CallIntMethod(outputDevice, getTypeMethod, new jvalue[] { });
                    audioDevices[i] = (AndroidAudioDeviceType) type;
                }
            }
            else
            {
                audioDevices = new AndroidAudioDeviceType[0];
            }

            //Debug.Log(this.ToString());
        }

        public override string ToString()
        {
            return string.Format(
                "Native Sampling Rate: {0} | Optimal Buffer Size: {1} | Low Latency Feature: {2} | Pro Audio Feature: {3} | Output devices : {4}",
                nativeSamplingRate, optimalBufferSize, lowLatencyFeature, proAudioFeature,
                string.Join(", ", audioDevices.Select(x => x.ToString()).ToArray()));
        }
#endif
    }
}