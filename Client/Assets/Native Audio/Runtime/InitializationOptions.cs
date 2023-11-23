namespace E7.Native
{
    public static partial class NativeAudio
    {
        /// <summary>
        ///     <para>
        ///         An option for <see cref="NativeAudio.Initialize(InitializationOptions)"/>.
        ///         Because it is a <c>struct</c>, start making it from <see cref="defaultOptions"/> to get a good default values.
        ///     </para>
        ///     <para>
        ///         This class is currently only contains options for Android. iOS options are fixed.
        ///     </para>
        /// </summary>
        public struct InitializationOptions
        {
            /// <summary>
            ///     <para>
            ///         [Android] On using <see cref="NativeAudio.Initialize(InitializationOptions)"/> you could freely specify a custom
            ///         buffer size that is not device's native buffer size.
            ///     </para>
            ///     <para>
            ///         However a buffer size too low with an intention
            ///         to minimize latency may break audio or even crash, a buffer size too high with an intention
            ///         to fix a problematic phone that report its own native buffer size wrong may not be a
            ///         multiple of device's native buffer size causing jittering problem.
            ///     </para>
            ///     <para>
            ///         This method translate buffer size number into something potentially better for the device for you
            ///         to use in initialization. Therefore the return value is device dependent and only usable at runtime.
            ///     </para>
            ///     <list type="bullet">
            ///         <item>
            ///             <description>
            ///                 If <paramref name="bufferSize"/> is lower than device's native buffer size, it will be clamped to
            ///                 device's native buffer size.
            ///             </description>
            ///         </item>
            ///         <item>
            ///             <description>
            ///                 If <paramref name="bufferSize"/> is higher than device's native buffer size,
            ///                 you get back an even larger number which is the next nearest multiple of device's
            ///                 native buffer size. The reason is to reduce jitter
            ///                 as stated in <a href="https://developer.android.com/ndk/guides/audio/audio-latency#buffer-size">here</a>.
            ///             </description>
            ///         </item>
            ///     </list>
            ///     <para>
            ///         Example of larger case : Specified <c>256</c>
            ///     </para>
            ///     <list type="bullet">
            ///         <item>
            ///             <description>Xperia Z5 : Native buffer size : 192 -> what you get : 384</description>
            ///         </item>
            ///         <item>
            ///             <description>Lenovo A..something : Native buffer size : 620 -> what you get : 620</description>
            ///         </item>
            ///     </list>
            ///     <para>
            ///         [iOS] Returns the same buffer size. Note that iOS cannot specify custom buffer size as it shares the
            ///         same size as you selected for Unity. (e.g. Best Latency = 256, etc.) This is why the option
            ///         has been named as <see cref="androidBufferSize"/>.
            ///     </para>
            /// </summary>
            public static int OptimizeBufferSize(int bufferSize)
            {
#if UNITY_ANDROID
                var optimizedBufferSize = AndroidNativeAudio.CallStatic<int>(AndroidOptimizeBufferSize, bufferSize);
                return optimizedBufferSize;
#elif UNITY_IOS
                return bufferSize;
#else
                return bufferSize;
#endif
            }

            /// <summary>
            ///     A good starting values to create custom options.
            /// </summary>
            /// <remarks>
            ///     A <c>struct</c> cannot have default value on <c>new</c>, this statically allocated
            ///     <c>struct</c> is prepared for you to copy from instead.
            /// </remarks>
            public static readonly InitializationOptions defaultOptions = new InitializationOptions
            {
                androidAudioTrackCount = 3,
                androidBufferSize = -1,
                preserveOnMinimize = false,
            };

            /// <summary>
            ///     How many native sources to request for Android.
            ///     <b>Default to 3</b> when using <see cref="defaultOptions"/>.
            ///     It directly translates to maximum concurrency you can have while staying unmixed.
            /// </summary>
            /// <remarks>
            ///     Please read
            ///     <a href="https://exceed7.com/native-audio/theories/ways-around-latency.html#problems-on-number-of-native-sources">
            ///         Problems on number of native sources
            ///     </a>
            ///     if you would like to increase this and learn what risks you are getting into.
            /// </remarks>
            public int androidAudioTrackCount;

            /// <summary>
            ///     <para>
            ///         At native side playing an audio is essentially a little callback that ask for the next tiny bit
            ///         of audio data. This callback is called when the previous bit were all pushed out to speaker.
            ///     </para>
            ///     <para>
            ///         The size of that bit is buffer size. It affects latency and stability of audio as it determines
            ///         frequency of this callback, or amount of data hardware could push out at a time.
            ///     </para>
            ///     <list type="bullet">
            ///         <item>
            ///             <description>
            ///                 If zero or negative, it uses buffer size exactly equal to device's native buffer size.
            ///                 In most case you would use -1 to enable this default and probably good behaviour.
            ///             </description>
            ///         </item>
            ///         <item>
            ///             <description>If odd number, it gets +1 to the next even number.</description>
            ///         </item>
            ///         <item>
            ///             <description>If any other positive number, use that number.</description>
            ///         </item>
            ///     </list>
            ///     <para>
            ///         See <see cref="OptimizeBufferSize"/>, which is a helper static method to get a buffer size
            ///         that is more compatible with the device.
            ///     </para>
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         If small, the device have to work
            ///         harder to fill the audio buffer to sent out because the previous round was used up so fast.
            ///         If the device can't the audio will be garbled.
            ///     </para>
            ///     <para>
            ///         If large, the device have more time to leisurely
            ///         fill the buffer as the previous sent data has more to put out until the next round,
            ///         however it results in increased audio latency.
            ///     </para>
            /// </remarks>
            public int androidBufferSize;

            /// <summary>
            ///     <para>
            ///         [Android]
            ///     </para>
            ///     <list type="bullet">
            ///         <item>
            ///             <description>
            ///                 If <c>false</c> (a default on <see cref="defaultOptions"/>),
            ///                 on <see cref="Initialize()"/> the native side will remember the spec you request.
            ///                 On minimize it will dispose all the sources
            ///                 (and in turn stopping them). On coming back it will reinitialize with the same spec
            ///             </description>
            ///         </item>
            ///         <item>
            ///             <description>
            ///                 If <c>true</c> the allocated native sources will not be freed when minimize the app.
            ///                 (The Unity ones did free and request a new one on coming back)
            ///             </description>
            ///         </item>
            ///     </list>
            ///     <para>
            ///         [iOS] No effect, iOS's native sources is already minimize-compatible
            ///         but its playing-when-minimized is prevented by the app's build option.
            ///     </para>
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         [Android]
            ///     </para>
            ///     <para>
            ///         This make it possible for audio played with Native Audio to play while minimizing the app,
            ///         and also to not spend time disposing and allocating sources again.
            ///     </para>
            ///     <para>
            ///         However this is not good since it adds "wake lock" to your game.
            ///         With <c>adb shell dumpsys power</c> while your game is minimized after using Native Audio
            ///         you will see something like <c> PARTIAL_WAKE_LOCK 'AudioMix' ACQ=-27s586ms(uid= 1041 ws= WorkSource{ 10331})</c>.
            ///     </para>
            ///     <para>
            ///         Meaning that the OS have to keep the audio mix alive all the time.
            ///         Not to mention most games do not really want this behaviour.
            ///     </para>
            ///     <para>
            ///         Most gamers I saw also minimized the game and sometimes forgot to close them off.
            ///         This cause not only battery drain when there is a wake lock active,
            ///         but also when the lock turns into <c>LONG</c> state it will show up as a warning in Google Play Store,
            ///         as it could detect that an app has a
            ///         <a href="https://developer.android.com/topic/performance/vitals/wakelock">Stuck partial wake lock</a> or not.
            ///     </para>
            ///     <para>
            ///         [iOS]
            ///     </para>
            ///     <para>
            ///         If you want the audio to continue to be heard in minimize,
            ///         use "Behaviour in background" set as Custom - Audio in Unity Player Settings then
            ///         <a href="https://forum.unity.com/threads/how-do-i-get-the-audio-running-in-background-ios.319602/">follow this thread</a>
            ///         to setup the <c>AVAudioSession</c> to correct settings.
            ///     </para>
            /// </remarks>
            public bool preserveOnMinimize;
        }
    }
}