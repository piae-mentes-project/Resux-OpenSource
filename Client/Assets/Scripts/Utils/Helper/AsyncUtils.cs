using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Resux
{
    [Obsolete]
    public static class AsyncUtils
    {
        [Obsolete("ʹ��`Task.Run`����")]
        public static Task<T> RunAsync<T>(Func<Task<T>> func)
        {
            return func();
        }


        [Obsolete("ʹ��`Task.Run`����")]
        public static Task RunAsync(Func<Task> func)
        {
            return func();
        }

        [Obsolete("ʹ��`await ...`����")]
        public static T RunAsyncWithResult<T>(Func<Task<T>> func)
        {
            return WaitAsyncWithResult(RunAsync(func));
        }

        [Obsolete("ʹ��`await ...`����")]
        public static T WaitAsyncWithResult<T>(Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}