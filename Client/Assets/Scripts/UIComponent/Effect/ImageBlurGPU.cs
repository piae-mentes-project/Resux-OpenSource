using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI.Component.Effect
{
	[RequireComponent(typeof(RawImage))]
	public class ImageBlurGPU : MonoBehaviour
	{
		private RawImage image;
		private TextureBlur blur;
		[SerializeField] private Shader blurShader;
		[SerializeField] private float duration;
		public AnimationCurve sizeWithIterationCurve;
		private float _startTime;
		private int _iter = 0;
		private bool _half = false;
		private bool _started = false;
		private bool _done = false;

		public int Iteration => (int)sizeWithIterationCurve.keys[sizeWithIterationCurve.keys.Length - 1].time * (_half ? 2 : 1);

		private void Awake()
		{
			image = GetComponent<RawImage>();
            TextureBlur.blurShader = blurShader;
        }

		public void SetTexture(RenderTexture rt)
        {
			image.texture = rt;
        }

		public void SetTexture(Texture2D tex)
		{
			image.texture = tex;
		}

		public void SetTexture(Sprite sprite)
        {
			image.texture = sprite.texture;
        }

		public void ResetBlur()
        {
			_started = false;
			_done = false;
        }

		public void StartBlur(bool halfStep)
		{
			if (_started) { return; }
			_started = true;
			_half = halfStep;
			_iter = 0;
			blur = new TextureBlur(image.texture, 1);
			image.texture = blur.DestRenderTexture;
			_startTime = Time.time;
			if (sizeWithIterationCurve.keys.Length < 2) { _done = true; }
			Logger.Log($"[ImageBlurGPU] Start blur cover, iteration: {Iteration}");
		}

		private void Update()
		{
			while (_started && !_done && _iter < Iteration && (_iter + 1) * (duration / Iteration) <= (Time.time - _startTime))
			{
				_iter++;
				var blurSize = (int)sizeWithIterationCurve.Evaluate(_half ? _iter / 2 : _iter);
				// Debugger.Log("[ImageBlurGPU] Step " + (_half ? "half " : "") + $"blur, size:{blurSize} ({_iter}/{Iteration})");
				if (_half) { blur.StepHalf(blurSize); }
				else { blur.Step(blurSize); }
				if (_iter == Iteration)
				{
					Logger.Log("[ImageBlurGPU] Rendering blur texture");
					image.texture = blur.Texture;
					blur.Dispose();
					_done = true;
				}
			}
		}

		public IEnumerator WaitBlurDone()
		{
			var wait = new WaitForEndOfFrame();
			while (!_done && _started)
			{
				yield return wait;
			}
		}

		private void OnDestroy()
		{
			blur?.Dispose();
		}
	}
}
