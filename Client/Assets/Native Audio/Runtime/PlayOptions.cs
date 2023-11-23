using System.Runtime.InteropServices;

namespace E7.Native
{
    public partial struct NativeSource
    {
        /// <summary>
        ///     Used with <see cref="Play(NativeAudioPointer, PlayOptions)"/> to customize your play.
        ///     Start creating it from <see cref="PlayOptions.defaultOptions"/>.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         On some platforms like iOS, adjusting them after the play with <see cref="NativeSource"/>
        ///         is already too late because you will already hear the audio. (Even in consecutive lines of code)
        ///     </para>
        ///     <para>
        ///         It has to be a <c>struct</c> since this will be sent to the native side,
        ///         interop to a matching code in other language.
        ///     </para>
        /// </remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct PlayOptions
        {
            /// <summary>
            ///     <para>
            ///         Structs couldn't have custom default values and something like volume
            ///         is better defaulted to 1 instead of 0.
            ///     </para>
            ///     <para>
            ///         This pre-allocated <c>static</c> variable contains sensible default values
            ///         that you can copy from as a starting point.
            ///     </para>
            /// </summary>
            /// <remarks>
            ///     Consists of :
            ///     <list type="bullet">
            ///         <item>
            ///             <description>Volume 1 (no attenuation).</description>
            ///         </item>
            ///         <item>
            ///             <description>Pan 0 (center).</description>
            ///         </item>
            ///         <item>
            ///             <description>Offset seconds 0 (starts from the beginning).</description>
            ///         </item>
            ///         <item>
            ///             <description>Source loop <c>false</c>.</description>
            ///         </item>
            ///     </list>
            /// </remarks>
            public static readonly PlayOptions defaultOptions = new PlayOptions
            {
                volume = 1,
                pan = 0,
                offsetSeconds = 0,
                sourceLoop = false,
            };

            /// <summary>
            ///     Set the volume of target native source before play.
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         [iOS] Maps to <c>AL_GAIN</c>. It is a scalar amplitude multiplier, so the value can
            ///         go over 1.0 for increasing volume but can be clipped.
            ///         If you put 0.5f, it is attenuated by 6 dB.
            ///     </para>
            ///     <para>
            ///         [Android] Maps to <c>SLVolumeItf</c> interface -> <c>SetVolumeLevel</c>.
            ///         The floating volume parameter will be converted to millibel (20xlog10x100)
            ///         so that putting 0.5f here results in 6dB attenuation.
            ///     </para>
            /// </remarks>
            public float volume;

            /// <summary>
            ///     <para>
            ///         Set the pan of target native source before play.
            ///         -1 for full left, 0 for center, 1 for full right.
            ///     </para>
            ///     <para>
            ///         This pan is based on "balance effect" and not a "constant energy pan"
            ///         that is at the center you hear each side fully.
            ///         (Constant energy pan has 3dB attenuation to both on center.)
            ///     </para>
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         [iOS] 2D panning in iOS will be emulated in OpenAL's 3D audio engine by splitting your stereo sound
            ///         into a separated mono sounds, then position each one on left and right ear of the listener.
            ///         When panning, instead of adjusting gain we will just move the source
            ///         further from the listener and the distance attenuation will do the work.
            ///         (Gain is reserved to the setting volume command, so we have 2 stage of gain adjustment this way.)
            ///     </para>
            ///     <para>
            ///         [Android] Maps to <c>SLVolumeItf</c> interface -> <c>SetStereoPosition</c>
            ///     </para>
            /// </remarks>
            public float pan;

            /// <summary>
            ///     <para>
            ///         Start playing from other point in the audio by offsetting
            ///         the target native source's play head time SECONDS unit.
            ///     </para>
            ///     <para>
            ///         Will do nothing if the offset is over the length of audio.
            ///     </para>
            /// </summary>
            public float offsetSeconds;

            /// <summary>
            ///     Apply a looping state on the native source.
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         The reason why it is "sourceLoop" instead of "loop" is to emphasize that if some newer sound
            ///         decided to use that native source to play, that looping sound is immediately stopped since we do not mix
            ///         and one native source can only handle one audio.
            ///     </para>
            ///     <para>
            ///         To "protect" the looping sound, you likely have to plan your native source index carefully when
            ///         choosing which source to play via <see cref="NativeAudio.GetNativeSource(int)"/>
            ///     </para>
            ///     <para>
            ///         Using the default round-robin <see cref="NativeAudio.GetNativeSourceAuto()"/> sooner or later
            ///         will stop your looping sound when it wraps back.
            ///     </para>
            /// </remarks>
            public bool sourceLoop;
        }
    }
}