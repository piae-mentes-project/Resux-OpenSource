namespace E7.Native
{
    public static partial class NativeAudio
    {
        /// <summary>
        ///     An option for <see cref="NativeAudio.Load(UnityEngine.AudioClip, LoadOptions)"/>.
        ///     Because it is a <c>struct</c>, start making it from <see cref="defaultOptions"/>
        ///     to get a good default values.
        /// </summary>
        public struct LoadOptions
        {
            /// <summary>
            ///     A good starting values to create custom options. A <c>struct</c> cannot have default value on <c>new</c>.
            /// </summary>
            public static readonly LoadOptions defaultOptions = new LoadOptions
            {
                resamplingQuality = ResamplingQuality.SINC_FASTEST,
            };

            /// <summary>
            ///     Determines what resampling quality for <a href="http://www.mega-nerd.com/SRC/">Secret Rabbit Code</a> to use.
            /// </summary>
            public enum ResamplingQuality
            {
                //SINC_BEST_QUALITY = 0,
                //SINC_MEDIUM_QUALITY = 1,

                /// <summary>
                ///     Use a coefficients from sinc wave for reconstruction. Takes a bit of time to complete.
                /// </summary>
                /// <remarks>
                ///     Some benchmark : Resampling a WAV file of about 2MB (this is quite big) from 44.1kHz
                ///     to 48kHz freezes the screen for 0.4 seconds on an Xperia Z5.
                ///     (See https://ccrma.stanford.edu/~jos/resample/)
                /// </remarks>
                SINC_FASTEST = 2,

                /// <summary>
                ///     Just use the previous value for any missing data.
                ///     It is the fastest resampling method but might sounds poor.
                /// </summary>
                ZERO_ORDER_HOLD = 3,

                /// <summary>
                ///     The missing value will be linearly interpolated.
                ///     Faster than sinc resampling.
                /// </summary>
                LINEAR = 4,
            }

            /// <summary>
            ///     The quality which <c>libsamplerate</c> will use to resample your audio to match the device's native rate.
            ///     Default to <see cref="ResamplingQuality.SINC_FASTEST"/> on <see cref="defaultOptions"/>
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         Two top quality setting has been removed from the source code since the sinc wave constant coefficients
            ///         are needed and could potentially make Native Audio sized at 0.79MB (For medium quality)
            ///         or 9.2MB (For best quality)
            ///     </para>
            ///     <para>
            ///         If you really need it, you can uncomment and then go modify back the source and
            ///         recompile with the missing coefficients.
            ///     </para>
            /// </remarks>
            public ResamplingQuality resamplingQuality;
        }
    }
}