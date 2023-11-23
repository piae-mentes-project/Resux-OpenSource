using System;
using System.Collections;
using System.Collections.Generic;

namespace Resux
{
    /// <summary>
    /// 可绑定对象
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public class BindableObject<T>
    {
        private T val;
        public T Value
        {
            get => val;
            set
            {
                if (val.Equals(value))
                {
                    return;
                }

                val = value;
                onValueChanged?.Invoke(value);
            }
        }

        private Action<T> onValueChanged;

        public BindableObject()
        {
            val = default(T);
        }

        public BindableObject(T val)
        {
            this.val = val;
        }

        public void Bind(Action<T> onValueChanged)
        {
            this.onValueChanged = onValueChanged;
        }

        public override string ToString()
        {
            return val.ToString();
        }

        #region static implicit method

        public static implicit operator BindableObject<T>(T val)
        {
            return new BindableObject<T>(val);
        }

        public static implicit operator T(BindableObject<T> obj)
        {
            return obj.val;
        }

        #endregion
    }
}
