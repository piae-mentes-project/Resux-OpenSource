using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// mono通用单例
    /// </summary>
    /// <typeparam name="T">需要以单例实现的mono类</typeparam>
    [DisallowMultipleComponent]
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static object obj = new object();

        public static T Instance
        {
            get
            {
                Type type = typeof(T);
                lock (obj)
                {
                    if (instance == null)
                    {
                        var instances = FindObjectsOfType(type);
                        if (instances.Length > 1)
                        {
                            Logger.LogWarning($"<color=yellow> 单例mono类型\"{type.FullName}\"的实例有多个！ </color>");
                            instance = (T)instances[0];
                        }
                        else if (instances.Length == 1)
                        {
                            instance = (T)instances[0];
                        }
                        else
                        {
                            var attrs = type.GetCustomAttributes(typeof(AutoSingletonAttribute), true);
                            if (attrs.Length <= 0)
                            {
                                Logger.LogError($"<color=red> 单例mono类型\"{type.FullName}\"不存在实例且不存在自动创建(AutoSingletonAttribute)属性！ </color>");
                                return null;
                            }

                            var attr = (AutoSingletonAttribute)attrs[0];
                            if (string.IsNullOrEmpty(attr.PrefabPath))
                            {
                                instance = Instantiate(new GameObject(attr.Name)).GetComponent<T>();
                            }
                            else
                            {
                                var go = Resources.Load<GameObject>(attr.PrefabPath);
                                instance = Instantiate(go).GetComponent<T>();
                            }
                        }
                    }

                    return instance;
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoSingletonAttribute : Attribute
    {
        public string Name;
        public string PrefabPath;

        public AutoSingletonAttribute(string name, string prefabPath = null)
        {
            Name = name;
            PrefabPath = prefabPath;
        }
    }
}
