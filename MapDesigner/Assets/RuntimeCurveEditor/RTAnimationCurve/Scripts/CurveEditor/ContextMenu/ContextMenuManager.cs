using UnityEngine;
using System.Collections.Generic;

namespace RuntimeCurveEditor
{
    /// <summary>
    /// Manages the context menus for all the curves' lists of key.
    /// </summary>
    public class ContextMenuManager
    {
        /// <summary>
        /// Keeps the context menu struct for all curves' keys 
        /// </summary>	
        public Dictionary<AnimationCurve, List<ContextMenu>> dictCurvesContextMenus = new Dictionary<AnimationCurve, List<ContextMenu>>();

        public static bool IsKeyframeFlat(Keyframe keyframe) {
            return (Mathf.Abs(keyframe.inTangent) < Mathf.Epsilon && Mathf.Abs(keyframe.outTangent) < Mathf.Epsilon);
        }

        public void AddContextMenuObjects(AnimationCurve curve) {
            if (curve != null) {
                List<ContextMenu> listContextMenus = new List<ContextMenu>();
                foreach (Keyframe keyframe in curve.keys) {
                    ContextMenu contextMenu = new ContextMenu();
                    contextMenu.freeSmooth = true;
                    if (IsKeyframeFlat(keyframe)) {
                        contextMenu.flat = true;
                    }
                    listContextMenus.Add(contextMenu);
                }
                dictCurvesContextMenus[curve] = listContextMenus;
            }
        }

        public void Remove(AnimationCurve curve) {
            dictCurvesContextMenus.Remove(curve);
        }

        public void UpdateContextMenuList(AnimationCurve curve) {
            List<ContextMenu> listContextMenus = new List<ContextMenu>();
            foreach (Keyframe keyframe in curve.keys) {
                ContextMenu contextMenu = new ContextMenu();
                contextMenu.freeSmooth = true;
                if (IsKeyframeFlat(keyframe)) {
                    contextMenu.flat = true;
                }
                listContextMenus.Add(contextMenu);
            }
            dictCurvesContextMenus[curve] = listContextMenus;
        }

        /// <summary>
        /// Updates the dictionary of context menu for the given curve.
        /// </summary>
        /// <param name='keysAddedCount'>
        /// Number of keys added(removed, if is negative) outside of this curve editor
        /// </param>
        public void UpdateDictContextMenu(AnimationCurve curve, int keysAddedCount) {
            //TODO
            //a much more correct way of updating the list of context menu objects, for example inserting or deleting the exact item in/from the list
            //for now, it's just adding/removing at the end of the list, so when just adding a key in the middle of the curve, outside of this curve editor,
            //the curve in this curve editor, might have some context menu changed(shifted) for the existing keys after the newly added key
            //for now, this easier solution, shouldn't harm, as there is not likely to change a curve outside of this curve editor		
            List<ContextMenu> listContextMenus = dictCurvesContextMenus[curve];
            if (keysAddedCount > 0) {
                for (int i = 0; i < keysAddedCount; ++i) {
                    ContextMenu contextMenu = new ContextMenu();
                    contextMenu.freeSmooth = true;
                    listContextMenus.Add(contextMenu);
                }
            } else {
                for (int i = 0; i < -keysAddedCount; ++i) {
                    listContextMenus.RemoveAt(curve.length);
                }
            }
        }

    }
}

