#if UNITY_IOS
using System;
using System.Runtime.InteropServices;
#endif

namespace E7.Native
{
    public static partial class NativeAudio
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        internal static extern int _Initialize();

        [DllImport("__Internal")]
        internal static extern int _SendByteArray(IntPtr byteArrayInput, int byteSize, int channels, int samplingRate,
            LoadOptions.ResamplingQuality resamplingQuality);

        [DllImport("__Internal")]
        internal static extern int _LoadAudio(string soundUrl, int resamplingQuality);

        [DllImport("__Internal")]
        internal static extern void _PrepareAudio(int audioBufferIndex, int nativeSourceIndex);

        [DllImport("__Internal")]
        internal static extern void _PlayAudioWithNativeSourceIndex(int nativeSourceIndex,
            NativeSource.PlayOptions playOptions);

        [DllImport("__Internal")]
        internal static extern void _UnloadAudio(int audioBufferIndex);

        [DllImport("__Internal")]
        internal static extern float _LengthByAudioBuffer(int audioBufferIndex);

        [DllImport("__Internal")]
        internal static extern void _GetDeviceAudioInformation(IntPtr interopArray, IntPtr outputDeviceEnumArray);

        [DllImport("__Internal")]
        internal static extern int _GetNativeSource(int nativeSourceIndex);

        // -- Operates on sound "source" chosen for a particular audio --
        // ("source" terms of OpenAL is like a speaker, not the "source of data" which is a loaded byte array.)

        [DllImport("__Internal")]
        internal static extern void _StopAudio(int nativeSourceIndex);

        [DllImport("__Internal")]
        internal static extern void _SetVolume(int nativeSourceIndex, float volume);

        [DllImport("__Internal")]
        internal static extern void _SetPan(int nativeSourceIndex, float pan);

        [DllImport("__Internal")]
        internal static extern float _GetPlaybackTime(int nativeSourceIndex);

        [DllImport("__Internal")]
        internal static extern void _SetPlaybackTime(int nativeSourceIndex, float offsetSeconds);

        [DllImport("__Internal")]
        internal static extern void _Pause(int nativeSourceIndex);

        [DllImport("__Internal")]
        internal static extern void _Resume(int nativeSourceIndex);
#endif
    }
}