using Resux.UI.Component.Effect;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.Editor
{
	[CustomEditor(typeof(ImageBlurGPU))]
	public class ImageBlurGPUEditor : UnityEditor.Editor
    {
        private SerializedProperty shaderValue;
        private SerializedProperty durationValue;
        private SerializedProperty sizeWithIterationCurveValue;
        private bool half = false;
        private ImageBlurGPU component;
        private RawImage image;
        private Texture blurResult;

        private void OnEnable()
        {
            shaderValue = serializedObject.FindProperty("blurShader");
            durationValue = serializedObject.FindProperty("duration");
            sizeWithIterationCurveValue = serializedObject.FindProperty("sizeWithIterationCurve");

            shaderValue.objectReferenceValue = shaderValue.objectReferenceValue ?? Shader.Find("Yeeeeeeee/Blur");
            component = serializedObject.targetObject as ImageBlurGPU;
            image = component.GetComponent<RawImage>();
            blurResult = image.texture;
        }

        public override void OnInspectorGUI()
		{
            serializedObject.Update();
            EditorGUILayout.PropertyField(shaderValue);
            EditorGUILayout.PropertyField(durationValue);
            EditorGUILayout.PropertyField(sizeWithIterationCurveValue);
            if (sizeWithIterationCurveValue.animationCurveValue.keys.Length < 2)
			{
                EditorGUILayout.HelpBox("SizeWithIterationCurve的键必须至少2个", MessageType.Error);
			}
            EditorGUILayout.LabelField("预览");
            half = EditorGUILayout.Toggle("半次模式", half);
            GUILayout.Box(new GUIContent(blurResult), GUILayout.Width(EditorGUIUtility.currentViewWidth), GUILayout.Height(EditorGUIUtility.currentViewWidth / blurResult.width * blurResult.height));
            if (GUILayout.Button("看看"))
            {
                TextureBlur blur = new TextureBlur(image.texture, 1);
                var iteration = (int)sizeWithIterationCurveValue.animationCurveValue.keys[sizeWithIterationCurveValue.animationCurveValue.keys.Length - 1].time * (half ? 2 : 1);
                for (int i = 0; i < iteration; i++)
                {
                    var blurSize = (int)sizeWithIterationCurveValue.animationCurveValue.Evaluate(half ? i : i / 2);
                    if (half) { blur.StepHalf(blurSize); }
                    else { blur.Step(blurSize); }
                }
                blurResult = blur.Texture;
                blur.Dispose();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
