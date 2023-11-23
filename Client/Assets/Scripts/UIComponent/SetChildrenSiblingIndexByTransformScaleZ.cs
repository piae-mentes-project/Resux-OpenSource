using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Resux.UI
{
    public class SetChildrenSiblingIndexByTransformScaleZ : MonoBehaviour
    {
        private void Update()
        {
            var children = new Transform[transform.childCount];
            for (int i = 0; i < children.Length; i++) { children[i] = transform.GetChild(i); }
            children = children.OrderBy(t => t.localScale.z).ToArray();
            var index = 0;
            foreach (var child in children)
            {
                child.SetSiblingIndex(index);
                index++;
            }
        }
    }
}
