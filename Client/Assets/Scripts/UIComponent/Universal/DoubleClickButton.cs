using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Resux.UI
{
	[RequireComponent(typeof(Button))]
	public class DoubleClickButton : MonoBehaviour
	{
		[SerializeField] private float effectiveTime = 3f;
		private Button button;
		private float previousClickTime;
		private UnityEvent m_onFirstClick = new UnityEvent();
		private UnityEvent m_onSecondClick = new UnityEvent();

		private ColorBlock previousNormalColorBlock;

		public float EffectiveTime
		{
			get => effectiveTime;
			set => effectiveTime = value;
		}
		public UnityEvent OnFirstClick => m_onFirstClick;
		public UnityEvent OnSecondClick => m_onSecondClick;

		private void Awake()
		{
			button = GetComponent<Button>();
			button.onClick.AddListener(() =>
			{
				if (Time.time - previousClickTime <= EffectiveTime)
				{
					m_onSecondClick.Invoke();
					previousClickTime = -114514f;
					button.colors = previousNormalColorBlock;
				}
				else
				{
					m_onFirstClick.Invoke();
					previousClickTime = Time.time;
					previousNormalColorBlock = button.colors;
					button.colors = new ColorBlock() { normalColor = button.colors.selectedColor, selectedColor = button.colors.selectedColor, colorMultiplier = button.colors.colorMultiplier, 
						disabledColor = button.colors.disabledColor, fadeDuration = button.colors.fadeDuration, highlightedColor = button.colors.highlightedColor, pressedColor = button.colors.pressedColor};
					StartCoroutine(ResetColor());
				}
			});
		}

		private IEnumerator ResetColor()
		{
			yield return new WaitForSeconds(effectiveTime - (Time.time - previousClickTime));
			button.colors = previousNormalColorBlock;
		}
	}
}
