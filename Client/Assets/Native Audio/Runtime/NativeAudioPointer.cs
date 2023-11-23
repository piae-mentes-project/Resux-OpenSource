using System;
using UnityEngine;

namespace E7.Native
{
    /// <summary>
    ///     <para>
    ///         A representation of loaded audio memory at the native side.
    ///     </para>
    ///     <para>
    ///         When you <see cref="NativeAudio.Load(AudioClip)"/> it is copying audio memory to native side. Each memory area
    ///         of loaded audio is given an ID. This "pointer" is not really a "memory address pointer" like in C++, but just
    ///         the mentioned ID. Just a simple integer.
    ///     </para>
    ///     <para>
    ///         Please do not create an instance of this class on your own. You can only get and keep from calling
    ///         <see cref="NativeAudio.Load(AudioClip)"/>
    ///     </para>
    /// </summary>
    public class NativeAudioPointer
    {
        /// <summary>
        ///     Some implementation in the future may need you to specify concurrent amount for each audio upfront
        ///     so I prepared this field. But it is always 1 for now. It has no effect because both iOS and Android
        ///     implementation automatically rotate players on play.
        ///     And so you get the concurrent amount equal to amount of players shared for all sounds,
        ///     not just this one sound.
        /// </summary>
        private readonly int amount;

        private readonly string soundPath;
        private readonly int startingIndex;

        private int currentIndex;

        private bool isUnloaded;

        /// <param name="amount">Right now amount is not used anywhere yet.</param>
        internal NativeAudioPointer(string soundPath, int index, float length, int amount = 1)
        {
            this.soundPath = soundPath;
            startingIndex = index;
            this.amount = amount;
            Length = length;

            currentIndex = index;
        }

        /// <summary>
        ///     <b>Cached</b> length in <b>seconds</b> of a loaded audio calculated from PCM byte size and specifications.
        /// </summary>
        public float Length { get; }

        /// <summary>
        ///     This will automatically cycles for you if the amount is not 1.
        /// </summary>
        internal int NextIndex
        {
            get
            {
                var toReturn = currentIndex;
                currentIndex = currentIndex + 1;
                if (currentIndex > startingIndex + amount - 1)
                {
                    currentIndex = startingIndex;
                }

                return toReturn;
            }
        }

        internal void AssertLoadedAndInitialized()
        {
            if (isUnloaded)
            {
                throw new InvalidOperationException("You cannot use an unloaded NativeAudioPointer.");
            }

            if (NativeAudio.Initialized == false)
            {
                throw new InvalidOperationException(
                    "You cannot use NativeAudioPointer while Native Audio itself is not in initialized state.");
            }
        }

        public override string ToString()
        {
            return soundPath;
        }

        /// <summary>
        ///     <para>
        ///         Free up loaded audio memory.
        ///         You cannot call <see cref="NativeSource.Play(NativeAudioPointer)"/> using this pointer anymore after unloading.
        ///         It will throw an exception.
        ///     </para>
        ///     <para>
        ///         <b>THIS METHOD IS UNSAFE ON ANDROID.</b> Read remarks and use with care!
        ///     </para>
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         [iOS] Unloads OpenAL audio buffer. If some native sources are currently playing
        ///         audio memory that you just unload, those tracks will be stopped automatically.
        ///     </para>
        ///     <para>
        ///         [Android] <c>free</c> the unmanaged audio data array at native side instantly.
        ///     </para>
        ///     <para>
        ///         This memory freeing could cause segmentation fault (SIGSEGV) if there are audio tracks
        ///         currently playing the memory.
        ///         You have to make sure by yourself there is no native source playing this audio before unloading.
        ///     </para>
        ///     <para>
        ///         On some higher-end phones, it will not crash but instead you will hear loud glitched audio.
        ///         This is the sound of play head running over freed memory and it interprets those as
        ///         sound instead of crashing.
        ///     </para>
        ///     <para>
        ///         Below are details why this method was not made less dangerous.
        ///     </para>
        ///     <para>
        ///         So the correct approach should be like this : we have to find out who is using the audio.
        ///         There could be multiple users playing a single audio memory.
        ///         And then stop them all before freeing memory, ideally.
        ///     </para>
        ///     <para>
        ///         However my native implementation for the best latency is to never stop any source, because
        ///         starting one again cause problems on some phones. The callbacks are always running.
        ///         We can't stop, won't stop.
        ///     </para>
        ///     <para>
        ///         The next idea is to let the callback know that it is not good to continue, by setting some kind
        ///         of "unloaded" flag for each audio on unloading. Then we free the memory immediately as before.
        ///     </para>
        ///     <para>
        ///         However, those callbacks are on a separated thread. It might be in the middle of copying
        ///         audio, and already pass the check we want to do to prevent the copy. Communication by flagging is possible
        ///         but it may be too late. This is what results in <c>memcpy</c> crash in your SIGSEGV crash report.
        ///     </para>
        ///     <para>
        ///         How about unloading NOT unload the audio instantly when you call unload,
        ///         but allowing all the playing sources that are using that audio to play this audio memory til the end.
        ///         at the same time prevents any new user. Then when all current tracks finished, unload at that moment.
        ///         (By keeping a play count of sorts similar to garbage collection, when reduced to zero while unload flag
        ///         is true, release the resource.)
        ///     </para>
        ///     <para>
        ///         Unfortunately again, this means we have to add `if` conditional to the callback function. So it could
        ///         decide should it free memory or not.
        ///     </para>
        ///     <para>
        ///         This callback function runs every little audio buffer that will be sent out your speaker, so it is a very
        ///         hot code path. Performance is very important especially considering the point of Native Audio.
        ///     </para>
        ///     <para>
        ///         For better assembly code, I have optimized very hard to eliminate all <c>if</c>,
        ///         to reduce the need of branch prediction for CPU.
        ///         And I couldn't bring myself to add back an <c>if</c> that the whole point is just to protect
        ///         from SIGSEGV potential from unloading, an operation that
        ///         is 1% rare when compared to how many times we play the audio, which triggers the callback over and over.
        ///     </para>
        ///     <para>
        ///         And what's more, you could prevent the crash manually 100% by just stopping the source you know are
        ///         playing that audio before unloading. Waiting a moment for all audio to finish before unloading
        ///         is an option too. I choose this manual work over automatic protection for rare case by
        ///         adding something to a hot code path.
        ///     </para>
        ///     <para>
        ///         Finally, what I settled with is that unloading <b>could cause SIGSEGV by design</b>,
        ///         and it is an unsafe method. I won't fix it.
        ///         Sorry that it doesn't look polished but it's all for better latency, the whole point of Native Audio.
        ///         I will do whatever it takes to get to the enqueue buffer call faster in that callback method.
        ///     </para>
        ///     <para>
        ///         What you have to do is just to be careful not to unload while someone is playing that audio by yourself.
        ///         The code can't help you since the check would be expensive.
        ///     </para>
        ///     <para>
        ///         One last warning, if you <see cref="NativeSource.Stop"/>
        ///         then immediately <see cref="Unload"/> on the next line of code,
        ///         it is actually <b>not</b> 100% safe.
        ///     </para>
        ///     <para>
        ///         The thing that keeps pumping audio to the speaker runs on thread,
        ///         by callbacks that runs on themselves over and over.
        ///         But stopping is issued on the main thread. It is basically just setting some flags so that
        ///         the next time that audio thread came, it stops putting out any more audio.
        ///         However by the nature of thread it will be concurrent with your main thread.
        ///     </para>
        ///     <para>
        ///         So for example this situation : if the stop runs, the thread had already pass the check
        ///         for stop and is putting out audio, then you call unload, then you get SIGSEGV because
        ///         it is putting out freed memory.
        ///     </para>
        ///     <para>
        ///         So stop and give it a few frames before unloading to be safe.
        ///     </para>
        /// </remarks>
        public void Unload()
        {
            if (!isUnloaded)
            {
#if UNITY_IOS
                NativeAudio._UnloadAudio(startingIndex);
                isUnloaded = true;
#elif UNITY_ANDROID
                for (var i = startingIndex; i < startingIndex + amount; i++)
                {
                    NativeAudio.unloadAudio(i);
                }
#endif
                isUnloaded = true;
            }
        }
    }
}