using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Resux
{
    public class TextureBlur : IDisposable
    {
        private static Material _blurMaterial;
        public static Shader blurShader;
        public static Material BlurMaterial
		{
            get
			{
                if (_blurMaterial == null) { _blurMaterial = new Material(blurShader ?? Shader.Find("Yeeeeeeee/Blur")); }
                return _blurMaterial;
            }
		}

        private Texture _textureCache;
        private bool _half;
        private RenderTexture rt1;
        private RenderTexture rt2;
        private bool cacheValidity;
        private bool _disposed;

        public Texture Texture
		{
			get
			{
                if (_disposed) { throw new ObjectDisposedException("TextureBlur"); }
                if (!cacheValidity) { RenderTextureImmediate(); }
                return _textureCache;
			}
		}
        public bool IsHalf => _half;
        public RenderTexture DestRenderTexture
		{
			get
            {
                if (_disposed) { throw new ObjectDisposedException("TextureBlur"); }
                return rt1;
            }
		}
        public bool Disposed => _disposed;

        public TextureBlur(Texture texture, int downSample, GraphicsFormat rtFormat = GraphicsFormat.R32G32B32A32_SFloat)
        {
            int width = texture.width / downSample;
            int height = texture.height / downSample;

            rt1 = RenderTexture.GetTemporary(width, height, 0, rtFormat);
            rt1.filterMode = FilterMode.Bilinear;
            rt2 = RenderTexture.GetTemporary(width, height, 0, rtFormat);
            rt2.filterMode = FilterMode.Bilinear;
            rt1.name = $"Blur_{texture.name}_RT1";
            rt2.name = $"Blur_{texture.name}_RT2";

            Graphics.Blit(texture, rt1);
            _half = false;
            cacheValidity = false;
            _disposed = false;
        }

        /// <summary>
        /// 模糊一次，如果上一次是模糊半次则这次为模糊剩下半次
        /// </summary>
        /// <param name="blurSize">模糊半径</param>
        public void Step(int blurSize)
		{
            if (_disposed) { throw new ObjectDisposedException("TextureBlur"); }

            if (IsHalf) { StepHalf(blurSize); }
			else
            {
                BlurMaterial.SetFloat("_BlurSize", blurSize);
                Graphics.Blit(rt1, rt2, BlurMaterial, 0);
                Graphics.Blit(rt2, rt1, BlurMaterial, 1);
            }
            cacheValidity = false;
		}

        /// <summary>
        /// 模糊半次(若是前半部分则DestRenderTexture内容不变),需要重新获取DestRenderTexture
        /// </summary>
        /// <param name="blurSize">模糊半径</param>
        public void StepHalf(int blurSize)
        {
            if (_disposed) { throw new ObjectDisposedException("TextureBlur"); }
            BlurMaterial.SetFloat("_BlurSize", blurSize);
            Graphics.Blit(rt1, rt2, BlurMaterial, IsHalf ? 1 : 0);
            (rt1, rt2) = (rt2, rt1);
            _half = !_half;
            cacheValidity = false;
		}

        /// <summary>
        /// 立刻把模糊结果转为Texture2D，并存入Texture缓存
        /// </summary>
        /// <returns>模糊了的贴图</returns>
        public Texture RenderTextureImmediate()
        {
            if (_disposed) { throw new ObjectDisposedException("TextureBlur"); }
            var oldActive = RenderTexture.active;
            RenderTexture.active = rt1;
            Texture2D result = new Texture2D(rt1.width, rt1.height, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, rt1.width, rt1.height), 0, 0);
            result.Apply();
            RenderTexture.active = oldActive;
            _textureCache = result;
            cacheValidity = true;
            return _textureCache;
        }

        /// <summary>
        /// Dispose之后就不能再调用步进，DestRenderTexture和Texture
        /// </summary>
        public void Dispose()
		{
            if (_disposed) { return; }
            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
            _textureCache = null;
            cacheValidity = false;
        }
	}
}
