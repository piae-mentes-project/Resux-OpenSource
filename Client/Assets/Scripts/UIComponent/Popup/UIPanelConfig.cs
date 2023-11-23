using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Resux.UI
{
    /// <summary>
    /// UI配置
    /// </summary>
    [Serializable]
    public struct UIConfig
    {
        /// <summary>
        /// UI物体
        /// </summary>
        public GameObject gameObject;

        /// <summary>
        /// UI名
        /// </summary>
        public string name;
    }

    public class UIPanelConfig : MonoBehaviour
    {
        public List<UIConfig> uiCfgs = new List<UIConfig>();

        /// <summary>
        /// 获取名字对应UI
        /// </summary>
        /// <param name="name">UI名</param>
        /// <returns>UI物体</returns>
        public GameObject GetUIByName(string name)
        {
            for (int i = 0; i < uiCfgs.Count; i++)
            {
                if (!uiCfgs[i].name.Equals(string.Empty))
                {
                    if (uiCfgs[i].name.Equals(name))
                    {
                        return uiCfgs[i].gameObject;
                    }
                }
                else
                {
                    if (uiCfgs[i].gameObject.name.Equals(name))
                    {
                        return uiCfgs[i].gameObject;
                    }
                }
            }

            return null;
        }
    }
}