using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_IOS
using System.Linq;
#endif

namespace E7.Native
{
    /// <summary>
    ///     The most important class, contains <c>static</c> methods that are used to command the native side.
    /// </summary>
    public static partial class NativeAudio
    {
        /// <summary>
        ///     <para>
        ///         Returns <c>true</c> after calling <see cref="Initialize()"/> successfully, meaning that
        ///         we have a certain amount of native sources ready for use at native side.
        ///     </para>
        ///     <para>
        ///         It is able to turn back to <c>false</c> if you call <see cref="NativeAudio.Dispose"/>
        ///         to return native sources back to the OS.
        ///     </para>
        /// </summary>
        public static bool Initialized { get; private set; }

        /// <summary>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 If in Editor, it is instantly unsupported (<c>false</c>) no matter what build platform selected.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>If not in Editor, it is <c>true</c> only on Android and iOS.</description>
        ///         </item>
        ///     </list>
        /// </summary>
        public static bool OnSupportedPlatform
        {
            get
            {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
                return true;
#else
                return false;
#endif
            }
        }

        private static void AssertInitialized()
        {
            if (!Initialized)
            {
                throw new InvalidOperationException("You cannot use Native Audio while in uninitialized state.");
            }
        }

        /// <summary>
        ///     <para>
        ///         [iOS] Initializes OpenAL. 15 OpenAL native sources will be allocated all at once.
        ///         It is not possible to initialize again on iOS. (Nothing will happen)
        ///     </para>
        ///     <para>
        ///         [Android] Initializes OpenSL ES. 1 OpenSL ES "Engine" and a number of native sources <c>AudioPlayer</c> object
        ///         (and in turn native <c>AudioTrack</c>) will be allocated all at once.
        ///     </para>
        ///     <para>
        ///         See <see cref="NativeAudio.Initialize(InitializationOptions)"/> overload
        ///         how to customize your initialization.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 More about this limit : https://developer.android.com/ndk/guides/audio/opensl/opensl-for-android
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 And my own research here : https://gametorrahod.com/android-native-audio-primer-for-unity-developers/
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        public static void Initialize()
        {
            Initialize(InitializationOptions.defaultOptions);
        }

        internal static NotSupportedException NotSupportedThrow()
        {
            return new NotSupportedException(
                "You cannot use Native Audio on unsupported platform, including in editor which counts as Windows or macOS.");
        }

        /// <summary>
        ///     <para>
        ///         [iOS] Initializes OpenAL. 15 OpenAL native sources will be allocated all at once.
        ///         It is not possible to initialize again on iOS. (Nothing will happen)
        ///     </para>
        ///     <para>
        ///         [Android] Initializes OpenSL ES. 1 OpenSL ES "Engine" and a number of native sources <c>AudioPlayer</c> object
        ///         (and in turn native <c>AudioTrack</c>) will be allocated all at once.
        ///     </para>
        ///     <para>
        ///         See <see cref="NativeAudio.Initialize(InitializationOptions)"/> overload
        ///         how to customize your initialization.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 More about this limit : https://developer.android.com/ndk/guides/audio/opensl/opensl-for-android
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 And my own research here : https://gametorrahod.com/android-native-audio-primer-for-unity-developers/
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <param name="initializationOptions">
        ///     Customize your initialization.
        ///     Start making it from <see cref="InitializationOptions.defaultOptions"/>
        /// </param>
        /// <exception cref="NotSupportedException">
        ///     Thrown when you initialize in Editor or something other than
        ///     iOS or Android at runtime.
        /// </exception>
        public static void Initialize(InitializationOptions initializationOptions)
        {
            if (!OnSupportedPlatform)
            {
                throw NotSupportedThrow();
            }
            //Now it is possible to initialize again with different option on Android. It would dispose and reallocate native sources.
#if UNITY_IOS
            if (Initialized)
            {
                return;
            }
#endif

#if UNITY_IOS
            var errorCode = _Initialize();
            if (errorCode == -1)
            {
                throw new Exception("There is an error initializing Native Audio occured at native side.");
            }

            //There is also a check at native side but just to be safe here.
            Initialized = true;
#elif UNITY_ANDROID
            var errorCode = AndroidNativeAudio.CallStatic<int>(AndroidInitialize,
                initializationOptions.androidAudioTrackCount, initializationOptions.androidBufferSize,
                initializationOptions.preserveOnMinimize);
            if (errorCode == -1)
            {
                throw new Exception("There is an error initializing Native Audio occured at native side.");
            }

            Initialized = true;
#endif
        }

        /// <summary>
        ///     <para>
        ///         [Android] Undo the <see cref="Initialize()"/>.
        ///         It doesn't affect any loaded audio, just dispose all the native sources returning them to OS and make them
        ///         available for other applications.
        ///     </para>
        ///     <para>
        ///         You still have to unload each audio.
        ///         Disposing twice is safe, it does nothing.
        ///     </para>
        ///     <para>
        ///         [iOS] Disposing doesn't work.
        ///     </para>
        ///     <para>
        ///         [Editor] This is a no-op. It is safe to call and nothing will happen.
        ///     </para>
        /// </summary>
        public static void Dispose()
        {
#if UNITY_ANDROID
            if (Initialized)
            {
                AndroidNativeAudio.CallStatic(AndroidDispose);
                Initialized = false;
            }
#elif UNITY_IOS
#else
            throw NotSupportedThrow();
#endif
        }

        /// <summary>
        ///     <para>
        ///         Loads by copying Unity-imported <see cref="AudioClip"/>'s raw audio memory to native side.
        ///         You are free to unload the <see cref="AudioClip"/>'s audio data without affecting
        ///         what's loaded at the native side after this.
        ///     </para>
        ///     <para>
        ///         [Editor] This method is a stub and returns <c>null</c>.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If you did not <see cref="Initialize()"/> yet, it will initialize with no <see cref="InitializationOptions"/>.
        ///         You cannot load audio while uninitialized.
        ///     </para>
        ///     <para>
        ///         Hard requirements :
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from
        ///                 your compressed audio.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 If you use Load In Background, you must call <see cref="AudioClip.LoadAudioData"/> beforehand
        ///                 and ensure that <see cref="AudioClip.loadState"/> is <see cref="AudioDataLoadState.Loaded"/> before
        ///                 calling <see cref="Load(UnityEngine.AudioClip)"/>. Otherwise it would throw an exception.
        ///                 If you are not using <see cref="AudioClip.loadInBackground"/> but also not using
        ///                 <see cref="AudioClip.preloadAudioData"/>, Native Audio can load for you if not yet loaded.
        ///             </description>
        ///         </item>
        ///         <item>Must not be <see cref="AudioClip.ambisonic"/>.</item>
        ///     </list>
        ///     <para>
        ///         It supports all compression format, force to mono, overriding to any sample rate, and quality slider.
        ///     </para>
        ///     <para>
        ///         [iOS] Loads an audio into OpenAL's output audio buffer. (Max 256)
        ///         This buffer will be paired to one of 15 OpenAL source when you play it.
        ///     </para>
        ///     <para>
        ///         [Android] Loads an audio into a <c>short*</c> array at unmanaged native side.
        ///         This array will be pushed into one of available <c>SLAndroidSimpleBufferQueue</c> when you play it.
        ///     </para>
        ///     <para>
        ///         The resampling of audio will occur at this moment to match your player's device native rate.
        ///     </para>
        ///     <para>
        ///         The SLES audio player must be created to match the device rate
        ///         to enable the special "fast path" audio.
        ///         What's left is to make our audio compatible with that fast path player,
        ///         which the resampler will take care of.
        ///     </para>
        ///     <para>
        ///         You can change the sampling quality of SRC (<c>libsamplerate</c>) library on a
        ///         per-audio basis with the <see cref="NativeAudio.Load(AudioClip, LoadOptions)"/> overload.
        ///     </para>
        /// </remarks>
        /// <param name="audioClip">
        ///     <para>
        ///         Hard requirements :
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from your
        ///                 compressed audio.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 If you use Load In Background, you must call <see cref="AudioClip.LoadAudioData"/> beforehand
        ///                 and ensure that <see cref="AudioClip.loadState"/> is <see cref="AudioDataLoadState.Loaded"/> before
        ///                 calling <see cref="Load(UnityEngine.AudioClip)"/>. Otherwise it would throw an exception.
        ///                 If you are not using <see cref="AudioClip.loadInBackground"/> but also not using
        ///                 <see cref="AudioClip.preloadAudioData"/>, Native Audio can load for you if not yet loaded.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>Must not be <see cref="AudioClip.ambisonic"/>.</description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <returns>
        ///     An audio buffer pointer for use with <see cref="NativeSource.Play(NativeAudioPointer)"/>.
        ///     Get the source from <see cref="NativeAudio.GetNativeSource(int)"/>
        /// </returns>
        /// <exception cref="Exception">Thrown when some unexpected exception at native side loading occurs.</exception>
        /// <exception cref="NotSupportedException">
        ///     Thrown when you have
        ///     prohibited settings on your <see cref="AudioClip"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when you didn't manually
        ///     load your <see cref="AudioClip"/> when it is not set to load in background.
        /// </exception>
        public static NativeAudioPointer Load(AudioClip audioClip)
        {
            return Load(audioClip, LoadOptions.defaultOptions);
        }

        /// <summary>
        ///     <para>
        ///         Loads by copying Unity-imported <see cref="AudioClip"/>'s raw audio memory to native side.
        ///         You are free to unload the <see cref="AudioClip"/>'s audio data without affecting
        ///         what's loaded at the native side after this.
        ///     </para>
        ///     <para>
        ///         [Editor] This method is a stub and returns <c>null</c>.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If you did not <see cref="Initialize()"/> yet, it will initialize with no <see cref="InitializationOptions"/>.
        ///         You cannot load audio while uninitialized.
        ///     </para>
        ///     <para>
        ///         Hard requirements :
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from
        ///                 your compressed audio.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 If you use Load In Background, you must call <see cref="AudioClip.LoadAudioData"/> beforehand
        ///                 and ensure that <see cref="AudioClip.loadState"/> is <see cref="AudioDataLoadState.Loaded"/> before
        ///                 calling <see cref="Load(UnityEngine.AudioClip)"/>. Otherwise it would throw an exception.
        ///                 If you are not using <see cref="AudioClip.loadInBackground"/> but also not using
        ///                 <see cref="AudioClip.preloadAudioData"/>, Native Audio can load for you if not yet loaded.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>Must not be <see cref="AudioClip.ambisonic"/>.</description>
        ///         </item>
        ///     </list>
        ///     <para>
        ///         It supports all compression format, force to mono, overriding to any sample rate, and quality slider.
        ///     </para>
        ///     <para>
        ///         [iOS] Loads an audio into OpenAL's output audio buffer. (Max 256)
        ///         This buffer will be paired to one of 15 OpenAL source when you play it.
        ///     </para>
        ///     <para>
        ///         [Android] Loads an audio into a <c>short*</c> array at unmanaged native side.
        ///         This array will be pushed into one of available <c>SLAndroidSimpleBufferQueue</c> when you play it.
        ///     </para>
        ///     <para>
        ///         The resampling of audio will occur at this moment to match your player's device native rate.
        ///     </para>
        ///     <para>
        ///         The SLES audio player must be created to match the device rate
        ///         to enable the special "fast path" audio.
        ///         What's left is to make our audio compatible with that fast path player,
        ///         which the resampler will take care of.
        ///     </para>
        ///     <para>
        ///         You can change the sampling quality of SRC (<c>libsamplerate</c>) library on a
        ///         per-audio basis with the <see cref="NativeAudio.Load(AudioClip, LoadOptions)"/> overload.
        ///     </para>
        /// </remarks>
        /// <param name="audioClip">
        ///     <para>
        ///         Hard requirements :
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 Load type MUST be Decompress On Load so Native Audio could read raw PCM byte array from your
        ///                 compressed audio.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 If you use Load In Background, you must call <see cref="AudioClip.LoadAudioData"/> beforehand
        ///                 and ensure that <see cref="AudioClip.loadState"/> is <see cref="AudioDataLoadState.Loaded"/> before
        ///                 calling <see cref="Load(UnityEngine.AudioClip, LoadOptions)"/>. Otherwise it would throw an exception.
        ///                 If you are not using <see cref="AudioClip.loadInBackground"/> but also not using
        ///                 <see cref="AudioClip.preloadAudioData"/>, Native Audio can load for you if not yet loaded.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>Must not be <see cref="AudioClip.ambisonic"/>.</description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <param name="loadOptions">
        ///     Customize your load.
        ///     Start creating your option from <see cref="LoadOptions.defaultOptions"/>.
        /// </param>
        /// <returns>
        ///     An audio buffer pointer for use with <see cref="NativeSource.Play(NativeAudioPointer)"/>.
        ///     Get the source from <see cref="NativeAudio.GetNativeSource(int)"/>
        /// </returns>
        /// <exception cref="Exception">Thrown when some unexpected exception at native side loading occurs.</exception>
        /// <exception cref="NotSupportedException">
        ///     Thrown when you have
        ///     prohibited settings on your <see cref="AudioClip"/>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when you didn't manually
        ///     load your <see cref="AudioClip"/> when it is not set to load in background.
        /// </exception>
        public static NativeAudioPointer Load(AudioClip audioClip, LoadOptions loadOptions)
        {
            AssertAudioClip(audioClip);
            AssertInitialized();

#if UNITY_IOS || UNITY_ANDROID
            //We have to wait for GC to collect this big array, or you could do `GC.Collect()` immediately after.
            var shortArray = AudioClipToShortArray(audioClip);
            var shortArrayPinned = GCHandle.Alloc(shortArray, GCHandleType.Pinned);
#endif


#if UNITY_IOS
            var startingIndex =
                _SendByteArray(shortArrayPinned.AddrOfPinnedObject(), shortArray.Length * 2, audioClip.channels,
                    audioClip.frequency, loadOptions.resamplingQuality);
            shortArrayPinned.Free();

            if (startingIndex == -1)
            {
                throw new Exception("Error loading NativeAudio with AudioClip named : " + audioClip.name);
            }

            var length = _LengthByAudioBuffer(startingIndex);
            return new NativeAudioPointer(audioClip.name, startingIndex, length);
#elif UNITY_ANDROID
            //The native side will interpret short array as byte array, thus we double the length.
            var startingIndex = sendByteArray(shortArrayPinned.AddrOfPinnedObject(), shortArray.Length * 2,
                audioClip.channels, audioClip.frequency, loadOptions.resamplingQuality);
            shortArrayPinned.Free();

            if (startingIndex == -1)
            {
                throw new Exception("Error loading NativeAudio with AudioClip named : " + audioClip.name);
            }

            var length = lengthByAudioBuffer(startingIndex);
            return new NativeAudioPointer(audioClip.name, startingIndex, length);
#else
            throw NotSupportedThrow();
#endif
        }

        /// <summary>
        ///     <para>
        ///         (<b>ADVANCED</b>) Loads an audio from <c>StreamingAssets</c> folder's destination at runtime.
        ///         Most of the cases you should use the <see cref="NativeAudio.Load(AudioClip)"/> overload instead.
        ///     </para>
        ///     <para>
        ///         It only supports <c>.wav</c> PCM 16-bit format, stereo or mono,
        ///         in any sampling rate since it will be resampled to fit the device.
        ///     </para>
        /// </summary>
        /// <param name="streamingAssetsRelativePath">
        ///     If the file is <c>SteamingAssets/Hit.wav</c> use "Hit.wav"
        ///     (WITH the extension).
        /// </param>
        /// <exception cref="System.IO.FileLoadException">
        ///     Thrown when some unexpected exception at native side
        ///     loading occurs.
        /// </exception>
        /// <returns>
        ///     An audio buffer pointer for use with <see cref="NativeSource.Play(NativeAudioPointer)"/>.
        ///     Get the source from <see cref="NativeAudio.GetNativeSource(int)"/>
        /// </returns>
        public static NativeAudioPointer Load(string streamingAssetsRelativePath)
        {
            return Load(streamingAssetsRelativePath, LoadOptions.defaultOptions);
        }

        /// <summary>
        ///     <para>
        ///         (<b>ADVANCED</b>) Loads an audio from <c>StreamingAssets</c> folder's destination at runtime.
        ///         Most of the cases you should use the <see cref="NativeAudio.Load(AudioClip)"/> overload instead.
        ///     </para>
        ///     <para>
        ///         It only supports <c>.wav</c> PCM 16-bit format, stereo or mono,
        ///         in any sampling rate since it will be resampled to fit the device.
        ///     </para>
        /// </summary>
        /// <param name="streamingAssetsRelativePath">
        ///     If the file is <c>SteamingAssets/Hit.wav</c> use "Hit.wav"
        ///     (WITH the extension).
        /// </param>
        /// <param name="loadOptions">
        ///     Customize your load.
        ///     Start creating your option from <see cref="LoadOptions.defaultOptions"/>.
        /// </param>
        /// <exception cref="System.IO.FileLoadException">
        ///     Thrown when some unexpected exception at native side
        ///     loading occurs.
        /// </exception>
        /// <returns>
        ///     An audio buffer pointer for use with <see cref="NativeSource.Play(NativeAudioPointer)"/>.
        ///     Get the source from <see cref="NativeAudio.GetNativeSource(int)"/>
        /// </returns>
        public static NativeAudioPointer Load(string streamingAssetsRelativePath, LoadOptions loadOptions)
        {
            AssertInitialized();

            if (Path.GetExtension(streamingAssetsRelativePath).ToLower() == ".ogg")
            {
                throw new NotSupportedException(
                    "Loading via StreamingAssets does not support OGG. Please use the AudioClip overload and set the import settings to Vorbis.");
            }

#if UNITY_IOS
            var startingIndex = _LoadAudio(streamingAssetsRelativePath, (int) loadOptions.resamplingQuality);
            if (startingIndex == -1)
            {
                throw new FileLoadException(
                    "Error loading audio at path : " + streamingAssetsRelativePath +
                    " Please check if that audio file really exist relative to StreamingAssets folder or not. Remember that you must include the file's extension as well.",
                    streamingAssetsRelativePath);
            }

            var length = _LengthByAudioBuffer(startingIndex);
            return new NativeAudioPointer(streamingAssetsRelativePath, startingIndex, length);
#elif UNITY_ANDROID
            var startingIndex = AndroidNativeAudio.CallStatic<int>(AndroidLoadAudio, streamingAssetsRelativePath,
                (int) loadOptions.resamplingQuality);

            if (startingIndex == -1)
            {
                throw new FileLoadException(
                    "Error loading audio at path : " + streamingAssetsRelativePath +
                    " Please check if that audio file really exist relative to StreamingAssets folder or not. Remember that you must include the file's extension as well.",
                    streamingAssetsRelativePath);
            }

            var length = lengthByAudioBuffer(startingIndex);
            return new NativeAudioPointer(streamingAssetsRelativePath, startingIndex, length);
#else
            throw NotSupportedThrow();
#endif
        }

        private static void AssertAudioClip(AudioClip audioClip)
        {
            if (audioClip.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                throw new NotSupportedException(string.Format(
                    "Your audio clip {0} load type is not Decompress On Load but {1}. " +
                    "Native Audio needs to read the raw PCM data by that import mode.",
                    audioClip.name, audioClip.loadType));
            }

            if (audioClip.channels != 1 && audioClip.channels != 2)
            {
                throw new NotSupportedException(string.Format(
                    "Native Audio only supports mono or stereo. Your audio {0} has {1} channels", audioClip.name,
                    audioClip.channels));
            }

            if (audioClip.ambisonic)
            {
                throw new NotSupportedException("Native Audio does not support ambisonic audio!");
            }

            if (audioClip.loadState != AudioDataLoadState.Loaded && audioClip.loadInBackground)
            {
                throw new InvalidOperationException(
                    "Your audio is not loaded yet while having the import settings Load In Background. " +
                    "Native Audio cannot wait for loading asynchronously for you and it would results in an empty audio. " +
                    "To keep Load In Background import settings, call `audioClip.LoadAudioData()` " +
                    "beforehand and ensure that `audioClip.loadState` is `AudioDataLoadState.Loaded` " +
                    "before calling `NativeAudio.Load`, or remove Load In Background then Native Audio " +
                    "could load it for you.");
            }
        }

        private static short[] AudioClipToShortArray(AudioClip audioClip)
        {
            if (audioClip.loadState != AudioDataLoadState.Loaded)
            {
                if (!audioClip.LoadAudioData())
                {
                    throw new Exception(string.Format("Loading audio {0} failed!", audioClip.name));
                }
            }

            var data = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(data, 0);

            //Convert to 16-bit PCM
            var shortArray = new short[audioClip.samples * audioClip.channels];
            for (var i = 0; i < shortArray.Length; i++)
            {
                shortArray[i] = (short) (data[i] * short.MaxValue);
            }

            return shortArray;
        }

        /// <summary>
        ///     <para>
        ///         Get a native source in order to play an audio or control an audio currently played on it.
        ///         You can keep and cache the returned native source reference and keep using it.
        ///     </para>
        ///     <para>
        ///         This method is for when you want a specific index of native source you would like to play on.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         It checks with the native side if
        ///         a specified <paramref name="nativeSourceIndex"/> is valid or not before returning a native source
        ///         interfacing object to you. If not, it has a fallback to round-robin native source selection.
        ///     </para>
        ///     <para>
        ///         Refer to
        ///         <a href="https://exceed7.com/native-audio/how-to-use/selecting-native-sources.html">
        ///             Selecting native sources
        ///         </a>
        ///         on how to strategize your native source index usage depending on your audio.
        ///     </para>
        /// </remarks>
        /// <param name="nativeSourceIndex">
        ///     <para>
        ///         Specify a zero-indexed native source that you want. If at <see cref="Initialize()"/> you
        ///         requested 3, then valid numbers here are : 0, 1, and 2.
        ///     </para>
        ///     <para>
        ///         If this index turns out to be an invalid index at native side, it has a fallback to round-robin
        ///         native source selection.
        ///     </para>
        /// </param>
        /// <returns>
        ///     <para>
        ///         Native source representation you can use it to play audio.
        ///     </para>
        ///     <para>
        ///         If <paramref name="nativeSourceIndex"/> used was invalid,
        ///         then this is a result of fallback round-robin native source selection.
        ///     </para>
        /// </returns>
        public static NativeSource GetNativeSource(int nativeSourceIndex)
        {
            AssertInitialized();
#if UNITY_ANDROID
            return new NativeSource(getNativeSource(nativeSourceIndex));
#elif UNITY_IOS
            return new NativeSource(_GetNativeSource(nativeSourceIndex));
#else
            throw NotSupportedThrow();
#endif
        }

        /// <summary>
        ///     <para>
        ///         Get a native source in order to play an audio or control an audio currently played on it.
        ///         You can keep and cache the returned native source reference and keep using it.
        ///     </para>
        ///     <para>
        ///         Unlike <see cref="GetNativeSource(int)"/>,
        ///         this method is for when you just want to play an audio without much care about stopping
        ///         a previously played audio on any available native source.
        ///     </para>
        ///     <para>
        ///         It selects a native source by round-robin algorithm, just select the next index
        ///         from the previous play.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     Refer to
        ///     <a href="https://exceed7.com/native-audio/how-to-use/selecting-native-sources.html">
        ///         Selecting native sources
        ///     </a>
        ///     on how to strategize your native source index usage depending on your audio.
        /// </remarks>
        /// <returns>
        ///     Native source representation you can use it to play audio resulting from round-robin selection.
        /// </returns>
        public static NativeSource GetNativeSourceAuto()
        {
            AssertInitialized();
#if UNITY_ANDROID
            return new NativeSource(getNativeSource(-1));
#elif UNITY_IOS
            return new NativeSource(_GetNativeSource(-1));
#else
            throw NotSupportedThrow();
#endif
        }

        /// <summary>
        ///     <para>
        ///         Get a native source in order to play an audio or control an audio currently played on it.
        ///         You can keep and cache the returned native source reference and keep using it.
        ///     </para>
        ///     <para>
        ///         Like <see cref="GetNativeSource(int)"/>, this method is for when you want a specific index
        ///         of native source to play. But unlike that, you can create your own "index returning object"
        ///         that implements <see cref="INativeSourceSelector"/>. Making it more systematic for you.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         You can have internal state inside it if it is a <c>class</c>, you can emulate the default
        ///         round-robin native source selection, for example.
        ///     </para>
        ///     <para>
        ///         Refer to
        ///         <a href="https://exceed7.com/native-audio/how-to-use/selecting-native-sources.html">
        ///             Selecting native sources
        ///         </a>
        ///         on how to strategize your native source index usage depending on your audio.
        ///     </para>
        /// </remarks>
        /// <returns>
        ///     Native source representation you can use it to play audio, resulting from an index that
        ///     Native Audio got from calling <see cref="INativeSourceSelector.NextNativeSourceIndex"/> on
        ///     <paramref name="nativeSourceSelector"/>.
        /// </returns>
        public static NativeSource GetNativeSourceAuto(INativeSourceSelector nativeSourceSelector)
        {
            AssertInitialized();
#if UNITY_ANDROID
            var index = nativeSourceSelector.NextNativeSourceIndex();
            return new NativeSource(getNativeSource(index));
#elif UNITY_IOS
            var index = nativeSourceSelector.NextNativeSourceIndex();
            return new NativeSource(_GetNativeSource(index));
#else
            throw NotSupportedThrow();
#endif
        }

        /// <summary>
        ///     <para>
        ///         Get number of usable native sources. Returns 0 if called before initialization.
        ///     </para>
        ///     <para>
        ///         [Android] It returns as many as you actually got from initialization.
        ///         (e.g. if you requested 999 and actually get 15, then this returns 15)
        ///     </para>
        ///     <para>
        ///         [iOS] It is currently fixed to 15 as that is how OpenAL works. It gives you a set amount of sources.
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This is useful in a case like you want to completely stop all audio played with Native Audio
        ///         before unloading audio to prevent crash where the play head now play unloaded memory.
        ///     </para>
        ///     <para>
        ///         Therefore you need to get <b>all</b> native sources and stop each one.
        ///         With this count you could do so in a <c>for</c> loop.
        ///     </para>
        /// </remarks>
        public static int GetNativeSourceCount()
        {
            AssertInitialized();
#if UNITY_ANDROID
            return getNativeSourceCount();
#elif UNITY_IOS
            return 15;
#else
            return 0;
#endif
        }

        /// <summary>
        ///     <para>
        ///         Ask the phone about its audio capabilities.
        ///     </para>
        ///     <para>
        ///         The returned <c>struct</c> has different properties depending on platform.
        ///         You should put preprocessor directive (<c>#if UNITY_ANDROID</c> and so on) over the returned object
        ///         if you are going to access any of its fields. Or else it would be an error if you switch your build platform.
        ///     </para>
        ///     <para>
        ///         [Editor] Does not work, returns default value of <see cref="DeviceAudioInformation"/>.
        ///     </para>
        /// </summary>
        public static DeviceAudioInformation GetDeviceAudioInformation()
        {
#if UNITY_ANDROID
            var jo = AndroidNativeAudio.CallStatic<AndroidJavaObject>(AndroidGetDeviceAudioInformation);
            return new DeviceAudioInformation(jo);
#elif UNITY_IOS
            var interopArray = new double[DeviceAudioInformation.interopArrayLength];
            var portArray = Enumerable.Repeat(-1, 20).ToArray();
            var interopArrayHandle = GCHandle.Alloc(interopArray, GCHandleType.Pinned);
            var portArrayHandle = GCHandle.Alloc(portArray, GCHandleType.Pinned);
            _GetDeviceAudioInformation(interopArrayHandle.AddrOfPinnedObject(), portArrayHandle.AddrOfPinnedObject());
            portArrayHandle.Free();
            return new DeviceAudioInformation(
                interopArray,
                portArray.Where(x => x != -1).Cast<DeviceAudioInformation.IosAudioPortType>().ToArray()
            );
#else
            return default(DeviceAudioInformation);
#endif
        }

        /// <summary>
        ///     <para>
        ///         (<b>EXPERIMENTAL</b>) Native Audio will load a small silent wav and
        ///         perform various stress test for about 1 second.
        ///         Your player won't be able to hear anything, but recommended to do it when there's no
        ///         other workload running because it will also measure FPS.
        ///     </para>
        ///     <para>
        ///         The test will be asynchronous because it has to wait for frame to play the next audio.
        ///         Yield wait for the result with the returned <see cref="NativeAudioAnalyzer"/>.
        ///         This is a component of a new game object created to run a test coroutine on your scene.
        ///     </para>
        ///     <para>
        ///         If your game is in a yieldable routine, use <c>yield return new WaitUntil( () => analyzer.Analyzed );</c>,
        ///         it will wait a frame until that is <c>true</c>.
        ///         If not, you can do a blocking wait with a <c>while</c> loop on <c>analyzer.Analyzed == false</c>.
        ///     </para>
        ///     <para>
        ///         You must have initialized Native Audio before doing the analysis or else
        ///         Native Audio will initialize with default options.
        ///         (Remember you cannot initialize twice to fix initialization options)
        ///     </para>
        ///     <para>
        ///         By the analysis result you can see if the frame rate drop while using Native Audio or not.
        ///         I have fixed most of the frame rate drop problem I found.
        ///         But if there are more obscure devices that drop frame rate, this method can check it at runtime
        ///         and by the returned result you can stop using Native Audio
        ///         and return to Unity <see cref="AudioSource"/>.
        ///     </para>
        /// </summary>
        public static NativeAudioAnalyzer SilentAnalyze()
        {
            AssertInitialized();
#if UNITY_ANDROID
            var go = new GameObject("NativeAudioAnalyzer");
            var sa = go.AddComponent<NativeAudioAnalyzer>();
            sa.Analyze();
            return sa;
#else
            throw NotSupportedThrow();
#endif
        }
    }
}