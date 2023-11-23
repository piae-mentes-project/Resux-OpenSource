using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    public class LocalizationLoader : MonoBehaviour
    {
        [Serializable]
        public class KeyValuePair
        {
            public string key;
            public Text val;
        }

        public List<KeyValuePair> LocalizationMap = new List<KeyValuePair>();

        void Start()
        {
            foreach (var kvPair in LocalizationMap)
            {
                if (kvPair.key == null || kvPair.val == null) continue;
                kvPair.val.text = kvPair.key.Localize();
            }
        }
    }
}
