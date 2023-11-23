using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Resux.UI;

[CustomEditor(typeof(UIFrameAnimation))]
public class UIFrameAnimationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("该组件 优先Image ，没有的话就用SpriteRender，都没有就报错了", MessageType.Info);
        // GUILayout.Label("该组件优先Image，没有的话就用SpriteRender，都没有就报错了");
        base.OnInspectorGUI();
        UIFrameAnimation frameAnimation = target as UIFrameAnimation;
        Sprite sprite;
        if (!frameAnimation.Image)
        {
            var image = frameAnimation.GetComponent<Image>();
            frameAnimation.Image = image;
        }
        if (!frameAnimation.spriteRenderer)
        {
            var spriteRender = frameAnimation.GetComponent<SpriteRenderer>();
            frameAnimation.spriteRenderer = spriteRender;
        }

        if (frameAnimation.Image)
        {
            sprite = frameAnimation.Image.sprite;
        }
        else if (frameAnimation.spriteRenderer)
        {
            sprite = frameAnimation.spriteRenderer.sprite;
        }
        else
        {
            return;
        }

        if (sprite)
        {
            var path = AssetDatabase.GetAssetPath(sprite.GetInstanceID());
            path = System.IO.Path.GetDirectoryName(path);
            path = path.Substring(path.IndexOf("Resources") + 10);
            if (!path.Equals(frameAnimation.FrameImagePath))
            {
                frameAnimation.FrameImagePath = path;
            }
        }
        else
        {
            Debug.LogError("帧动画没有赋值<color=red>初始值</color>，无法获取对应路径");
        }
    }
}