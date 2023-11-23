#if UNITY_ANDROID
using System;
using System.Runtime.InteropServices;
using UnityEngine;
#endif

namespace E7.Native
{
    public static partial class NativeAudio
    {
#if UNITY_ANDROID
        private static AndroidJavaClass androidNativeAudio;

        internal static AndroidJavaClass AndroidNativeAudio
        {
            get
            {
                if (androidNativeAudio == null)
                {
                    androidNativeAudio = new AndroidJavaClass("com.Exceed7.NativeAudio.NativeAudio");
                }

                return androidNativeAudio;
            }
        }

        /// <summary>
        ///     [Android] Initialize needs to contact Java as it need the device's native sampling rate
        ///     and native buffer size to get the "fast path" audio.
        /// </summary>
        internal const string AndroidInitialize = "Initialize";

        /// <summary>
        ///     [Android] A method to transform any buffer size number into one potentially more suitable to the device.
        /// </summary>
        internal const string AndroidOptimizeBufferSize = "OptimizeBufferSize";

        /// <summary>
        ///     [Android] Load needs to contact Java as it needs to read the audio file sent from <c>StreamingAssets</c>,
        ///     which could end up in either app persistent space or an another OBB package
        ///     which we will unpack it and get the content.
        /// </summary>
        internal const string AndroidLoadAudio = "LoadAudio";

        internal const string AndroidGetDeviceAudioInformation = "GetDeviceAudioInformation";
        internal const string AndroidDispose = "Dispose";

        // -- Operates on an audio file ("source" of data) --

        //The lib name is libnativeaudioe7

        // -- the play chain --

        [DllImport("nativeaudioe7")]
        internal static extern int getNativeSource(int nativeSourceIndex);

        [DllImport("nativeaudioe7")]
        internal static extern void prepareAudio(int audioBufferIndex, int nativeSourceIndex);

        [DllImport("nativeaudioe7")]
        internal static extern void playAudioWithNativeSourceIndex(int nativeSourceIndex,
            NativeSource.PlayOptions playOptions);

        // -- operation on native sources --

        [DllImport("nativeaudioe7")]
        internal static extern int stopAudio(int nativeSourceIndex);

        [DllImport("nativeaudioe7")]
        internal static extern void setVolume(int nativeSourceIndex, float volume);

        [DllImport("nativeaudioe7")]
        internal static extern void setPan(int nativeSourceIndex, float pan);

        [DllImport("nativeaudioe7")]
        internal static extern float getPlaybackTime(int nativeSourceIndex);

        [DllImport("nativeaudioe7")]
        internal static extern void setPlaybackTime(int nativeSourceIndex, float offsetSeconds);

        [DllImport("nativeaudioe7")]
        internal static extern void pause(int nativeSourceIndex);

        [DllImport("nativeaudioe7")]
        internal static extern void resume(int nativeSourceIndex);

        // -- others --

        [DllImport("nativeaudioe7")]
        internal static extern int getNativeSourceCount();

        [DllImport("nativeaudioe7")]
        internal static extern int sendByteArray(IntPtr byteArrayInput, int byteSize, int channels, int samplingRate,
            LoadOptions.ResamplingQuality resamplingQuality);

        [DllImport("nativeaudioe7")]
        internal static extern void unloadAudio(int audioBufferIndex);

        [DllImport("nativeaudioe7")]
        internal static extern float lengthByAudioBuffer(int audioBufferIndex);

#endif
    }
}