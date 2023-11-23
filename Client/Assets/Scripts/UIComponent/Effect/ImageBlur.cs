using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI.Component.Effect
{
    [RequireComponent(typeof(Image))]
    public class ImageBlur : MonoBehaviour
    {
        #region inner class

        public class texture2DData
        {
            public Color32[] colors;
            public int width;
            public int height;
        }

        #endregion

        #region properties

        private const float PI = 3.141592654f;
        private const float PI2 = 2 * PI;
        private float SqrtPI2 = Mathf.Sqrt(PI2);

        [SerializeField] [Range(1, 64)] private int blurRadius = 10;
        private float sigma;
        private float sigmaQuare;
        private float sigmaQuare2;

        private float sigmaQuarePI2;
        private float sigmaSqrtPI2;

        private Image image;
        private Texture2D texture;

        #endregion

        void Start()
        {
            image = GetComponent<Image>();
            sigma = blurRadius / 3;
            sigmaQuare = sigma * sigma;
            sigmaQuare2 = 2 * sigmaQuare;
            sigmaQuarePI2 = PI2 * sigmaQuare;
            sigmaSqrtPI2 = sigma * SqrtPI2;
        }

        #region Public Method

        public async Task StartBlur()
        {
            texture = image.sprite.texture;
            var data = new texture2DData()
            {
                width = texture.width,
                height = texture.height,
                colors = texture.GetPixels32(0)
            };
            await Task.Run(() => GaussianBlur(data));
            // 结果转换
            var resultTexture = new Texture2D(data.width, data.height);
            resultTexture.SetPixels32(data.colors);
            resultTexture.Apply();
            var result = Sprite.Create(resultTexture, new Rect(0, 0, data.width, data.height), new Vector2(0.5f, 0.5f));
            result.name = "blur";
            image.sprite = result;
        }

        #endregion

        #region Private Method

        private void GaussianBlur(texture2DData rawData)
        {
            var (width, height) = (rawData.width, rawData.height);
            // 总的像素色彩数组，从第一行到最后一行
            var colors = rawData.colors;
            var tempColors = colors;
            var targetColors = new Color32[colors.Length];
            // 因为N(0,1)的分布是关于y轴对称的正态分布，但是为了快点就全存了
            // 对应的概率分布应该为x=μ-3σ范围内
            var distributions = new float[2 * blurRadius + 1];
            // 一维
            float totalDistribution = 0;
            for (int i = -blurRadius, index = 0; i <= blurRadius; i++, index++)
            {
                distributions[index] = GetGaussianDistribution1Axis(i);
                totalDistribution += distributions[index];
            }
            // 为了使权重总和为1而进行归一化处理
            for (int i = 0; i < distributions.Length; i++)
            {
                distributions[i] /= totalDistribution;
            }

            // 水平采样
            for (int y = 0, index = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++, index++)
                {
                    // 每个像素的模糊计算
                    var color = new Color32()
                    {
                        a = colors[index].a
                    };
                    float r, g, b;
                    r = g = b = 0;
                    // 采样坐标
                    for (int i = -blurRadius, distIndex = 0; i <= blurRadius; i++, distIndex++)
                    {
                        int t_i = Edge(i, y, height);
                        var idx = index + t_i;
                        r += colors[idx].r * distributions[distIndex];
                        g += colors[idx].g * distributions[distIndex];
                        b += colors[idx].b * distributions[distIndex];
                    }

                    color.r = (byte)Mathf.Clamp((int)r, 0, 255);
                    color.g = (byte)Mathf.Clamp((int)g, 0, 255);
                    color.b = (byte)Mathf.Clamp((int)b, 0, 255);

                    tempColors[index] = color;
                }
            }
            // 更新采样数据
            colors = tempColors;
            // 竖直采样
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 每个像素的模糊计算
                    var index = y * width + x;
                    var color = new Color32()
                    {
                        a = colors[index].a
                    };
                    float r, g, b;
                    r = g = b = 0;
                    // 采样坐标
                    for (int i = -blurRadius, distIndex = 0; i <= blurRadius; i++, distIndex++)
                    {
                        int t_i = Edge(i, y, height);
                        var idx = index + t_i * width;
                        r += tempColors[idx].r * distributions[distIndex];
                        g += tempColors[idx].g * distributions[distIndex];
                        b += tempColors[idx].b * distributions[distIndex];
                    }

                    color.r = (byte)Mathf.Clamp((int)r, 0, 255);
                    color.g = (byte)Mathf.Clamp((int)g, 0, 255);
                    color.b = (byte)Mathf.Clamp((int)b, 0, 255);

                    targetColors[index] = color;
                }
            }

            rawData.colors = targetColors;
        }

        /// <summary>
        /// 二维的高斯函数
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns>概率/权重</returns>
        private float GetGaussianDistribution2Axis(float x, float y)
        {
            return Mathf.Exp(-(x * x + y * y) / sigmaQuare2) / sigmaQuarePI2;
        }

        /// <summary>
        /// 一维的高斯函数
        /// </summary>
        /// <param name="x">坐标</param>
        /// <returns>概率/权重</returns>
        private float GetGaussianDistribution1Axis(float x)
        {
            return Mathf.Exp(-x * x / sigmaQuare2) / sigmaSqrtPI2;
        }

        /// <summary>
        /// 把偏移后的点位限制在单行或列的范围内
        /// </summary>
        /// <param name="offset">偏移</param>
        /// <param name="pos">基准点位</param>
        /// <param name="width">行或列的长度</param>
        /// <returns></returns>
        private int Edge(int offset, int pos, int width)
        {
            var res = pos + offset;
            if (res < 0)
            {
                res = -pos;
            }
            else if (res >= width)
            {
                res = width - 1 - pos;
            }
            else
            {
                res = offset;
            }

            return res;
        }

        #endregion

        #region Coroutine



        #endregion
    }
}
