namespace E7.Native
{
    /// <summary>
    ///     An <c>interface</c> to use with <see cref="NativeAudio.GetNativeSourceAuto(INativeSourceSelector)"/>
    ///     You can implement your own logic that derives an index depending on some internal state.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         You can for example create <c>class MyKickDrumSelector : INativeSourceSelector</c>
    ///         and <c>class MySnareSelector : INativeSourceSelector</c>.
    ///     </para>
    ///     <para>
    ///         The target is that the kick is short, but often used. You want it to use native source index 0 exclusively.
    ///         The snares keep using index 1 and 2 to not have to trouble the kick drum.
    ///     </para>
    ///     <para>
    ///         Code the logic such that :
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     The kick drum one keeps returning <c>0</c> in its
    ///                     <see cref="NextNativeSourceIndex"/> implementation.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     The snare one return <c>1</c> and <c>2</c> alternately on each
    ///                     <see cref="NextNativeSourceIndex"/> call.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public interface INativeSourceSelector
    {
        /// <summary>
        ///     <para>
        ///         Each call could return a different native source index by your own logic.
        ///         Native Audio will call this once on each <see cref="NativeAudio.GetNativeSourceAuto(INativeSourceSelector)"/>
        ///     </para>
        ///     <para>
        ///         If the returned <c>int</c> turns out to be an invalid index at native side,
        ///         it has a fallback to round-robin native source selection.
        ///     </para>
        /// </summary>
        int NextNativeSourceIndex();
    }
}