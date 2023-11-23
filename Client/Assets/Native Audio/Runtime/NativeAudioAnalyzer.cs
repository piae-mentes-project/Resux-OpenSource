using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace E7.Native
{
    /// <summary>
    ///     Result from running <see cref="NativeAudioAnalyzer.Analyze"/>.
    /// </summary>
    public class NativeAudioAnalyzerResult
    {
        public float averageFps;
    }

    /// <summary>
    ///     The game object with this component is able to test native audio over several frames.
    /// </summary>
    public class NativeAudioAnalyzer : MonoBehaviour
    {
        /// <summary>
        ///     If the analysis was too long for your liking you can reduce it here,
        ///     but the average value return might not be so accurate.
        /// </summary>
        private const float secondsOfPlay = 1f;

        /// <summary>
        ///     Assuming your game runs at 60 FPS, it will test <c>60 * seconds</c> times.
        /// </summary>
        private const int framesOfPlay = (int) (60 * secondsOfPlay);

        private static NativeAudioPointer silence;

        public List<long> allTicks = new List<long>();

        private IEnumerator analyzeRoutine;
        private Stopwatch sw;

        /// <summary>
        ///     <para>
        ///         You can wait for the result on this. Then after it is done, <c>AnalysisResult</c>
        ///         contains the result. If not, that variable is <c>null</c>.
        ///     </para>
        ///     <para>
        ///         If your game is in a yieldable routine, use <c>yield return new WaitUntil( () => analyzer.Analyzed );</c>
        ///     </para>
        ///     <para>
        ///         If not, you can do a blocking wait with a <c>while</c> loop on <c>analyzer.Analyzed == false</c>.
        ///     </para>
        /// </summary>
        public bool Analyzed => analyzeRoutine == null;

        /// <summary>
        ///     Access this property after <see cref="Analyzed"/> property became true.
        /// </summary>
        public NativeAudioAnalyzerResult AnalysisResult { get; private set; }

        private float TicksToMs(long ticks)
        {
            return ticks / 10000f;
        }

        private float TicksToMs(double ticks)
        {
            return (float) (ticks / 10000);
        }

        private static float StdDev(IEnumerable<long> values)
        {
            float ret = 0;
            var count = values.Count();
            if (count > 1)
            {
                var avg = (float) values.Average();
                var sum = values.Sum(d => (d - avg) * (d - avg));
                ret = Mathf.Sqrt(sum / count);
            }

            return ret;
        }

        /// <summary>
        ///     <para>
        ///         This is already called from <see cref="NativeAudio.SilentAnalyze"/>
        ///         But you can do it again if you want, it might return a new result who knows...
        ///     </para>
        ///     <para>
        ///         You can wait on the public property <see cref="Analyzed"/>.
        ///     </para>
        ///     <para>
        ///         If your game is in a yieldable routine, use <c>yield return new WaitUntil( () => analyzer.Analyzed );</c>
        ///     </para>
        ///     <para>
        ///         If not, you can do a blocking wait with a <c>while</c> loop on <c>analyzer.Analyzed == false</c>.
        ///     </para>
        /// </summary>
        public void Analyze()
        {
            if (analyzeRoutine != null)
            {
                StopCoroutine(analyzeRoutine);
            }

            analyzeRoutine = AnalyzeRoutine();
            StartCoroutine(analyzeRoutine);
        }

        /// <summary>
        ///     There is a test game object for running the coroutine on your scene.
        ///     It does not take anything significant but you can call this to destroy it.
        /// </summary>
        public void Finish()
        {
            Destroy(this);
        }

        private IEnumerator AnalyzeRoutine()
        {
            Debug.Log("Built in analyze start");
            sw = new Stopwatch();
            allTicks = new List<long>();

            if (silence != null)
            {
                silence.Unload();
            }

            //This "" is a special path to load a silence.
            silence = NativeAudio.Load("");

            //To warm up the audio circuit we will discard half of the test.
            for (var i = 0; i < framesOfPlay / 2; i++)
            {
                NativeAudio.GetNativeSourceAuto().Play(silence);
                yield return null;
            }

            //Ok this is the real thing.
            for (var i = 0; i < framesOfPlay / 2; i++)
            {
                sw.Start();
                NativeAudio.GetNativeSourceAuto().Play(silence);
                yield return null;
                sw.Stop();
                allTicks.Add(sw.ElapsedTicks);
                sw.Reset();
            }

            AnalysisResult = new NativeAudioAnalyzerResult
            {
                averageFps = 1000 / TicksToMs(allTicks.Average()),
            };
            analyzeRoutine = null;
            Debug.Log("Built in analyze end");
        }
    }
}