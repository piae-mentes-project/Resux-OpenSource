using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Resux
{
    /// <summary>
    /// monoͨ�õ���
    /// </summary>
    /// <typeparam name="T">��Ҫ�Ե���ʵ�ֵ�mono��</typeparam>
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
                            Logger.LogWarning($"<color=yellow> ����mono����\"{type.FullName}\"��ʵ���ж���� </color>");
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
                                Logger.LogError($"<color=red> ����mono����\"{type.FullName}\"������ʵ���Ҳ������Զ�����(AutoSingletonAttribute)���ԣ� </color>");
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
