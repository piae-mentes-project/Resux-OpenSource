using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System;
using Resux.Data;

namespace Resux
{
    public static class Utils
    {
        /// <summary>
        /// 检查从数组头部开始的<paramref name="length"/>长度内，两个数组是否相同
        /// </summary>
        /// <typeparam name="T">数组元素类型</typeparam>
        /// <param name="array1">数组1</param>
        /// <param name="array2">数组2</param>
        /// <param name="length">检查长度</param>
        /// <returns>两个数组在范围内是否相等</returns>
        public static bool IsSameArray<T>(T[] array1, T[] array2, int length)
        {
            for (int i = 0; i < length; i++)
            {
                if (array1[i].Equals(array2[i]))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 将类似"1,2,3"的list字符串转换为<see cref="List{T}"/>
        /// </summary>
        /// <param name="content">字符串内容</param>
        /// <param name="split">分隔符</param>
        /// <returns><see cref="int"/>的<see cref="List{T}"/></returns>
        public static List<int> TransToList(string content, char split = ',')
        {
            var res = new List<int>();
            var items = content.Split(split);
            foreach (var item in items)
            {
                res.Add(int.Parse(item));
            }

            return res;
        }

        /// <summary>
        /// 将字符串以两种分隔符转换为<see cref="Dictionary{TKey, TValue}"/>实例
        /// key为<see cref="int"/>，value为<see cref="int"/>
        /// </summary>
        /// <param name="content">字符串内容</param>
        /// <param name="split1">分隔符1（两个元素间）</param>
        /// <param name="split2">分隔符2（key和value间）</param>
        /// <returns>Dic实例</returns>
        public static Dictionary<int, int> TransToDicII(string content, char split1 = ',', char split2 = '-')
        {
            var res = new Dictionary<int, int>();
            var pairs = content.Split(split1);
            foreach (var pair in pairs)
            {
                var kv = pair.Split(split2);
                res.Add(int.Parse(kv[0]), int.Parse(kv[1]));
            }

            return res;
        }

        /// <summary>
        /// 将字符串以两种分隔符转换为<see cref="Dictionary{TKey, TValue}"/>实例
        /// key为<see cref="string"/>，value为<see cref="int"/>
        /// </summary>
        /// <param name="content">字符串内容</param>
        /// <param name="split1">分隔符1（两个元素间）</param>
        /// <param name="split2">分隔符2（key和value间）</param>
        /// <returns>Dic实例</returns>
        public static Dictionary<string, int> TransToDicSI(string content, char split1 = ',', char split2 = '-')
        {
            var res = new Dictionary<string, int>();
            var pairs = content.Split(split1);
            foreach (var pair in pairs)
            {
                var kv = pair.Split(split2);
                res.Add(kv[0], int.Parse(kv[1]));
            }

            return res;
        }

        /// <summary>
        /// 将字符串以两种分隔符转换为<see cref="Dictionary{TKey, TValue}"/>实例
        /// key为<see cref="int"/>，value为<see cref="string"/>
        /// </summary>
        /// <param name="content">字符串内容</param>
        /// <param name="split1">分隔符1（两个元素间）</param>
        /// <param name="split2">分隔符2（key和value间）</param>
        /// <returns>Dic实例</returns>
        public static Dictionary<int, string> TransToDicIS(string content, char split1 = ',', char split2 = '-')
        {
            var res = new Dictionary<int, string>();
            var pairs = content.Split(split1);
            foreach (var pair in pairs)
            {
                var kv = pair.Split(split2);
                res.Add(int.Parse(kv[0]), kv[1]);
            }

            return res;
        }

        /// <summary>
        /// 将字符串以两种分隔符转换为<see cref="Dictionary{TKey, TValue}"/>实例
        /// key为<see cref="string"/>，value为<see cref="string"/>
        /// </summary>
        /// <param name="content">字符串内容</param>
        /// <param name="split1">分隔符1（两个元素间）</param>
        /// <param name="split2">分隔符2（key和value间）</param>
        /// <returns>Dic实例</returns>
        public static Dictionary<string, string> TransToDicSS(string content, char split1 = ',', char split2 = '-')
        {
            var res = new Dictionary<string, string>();
            var pairs = content.Split(split1);
            foreach (var pair in pairs)
            {
                var kv = pair.Split(split2);
                res.Add(kv[0], kv[1]);
            }

            return res;
        }

        /// <summary>
        /// 将字符串转化为Vector2（x，y）
        /// </summary>
        /// <param name="vecStr">字符串内容</param>
        /// <param name="hasEdge">是否存在边界（即括号）</param>
        /// <returns>转化后的Vector2</returns>
        public static Vector2 ConvertStrToVector2(string vecStr, bool hasEdge = true)
        {
            var vec = hasEdge ? vecStr.Substring(1, vecStr.Length - 1) : vecStr;
            var values = vec.Split(',');
            return new Vector2(float.Parse(values[0]), float.Parse(values[1]));
        }

        /// <summary>
        /// 将字符串转化为Vector2Int（x，y）
        /// </summary>
        /// <param name="vecStr">字符串内容</param>
        /// <param name="hasEdge">是否存在边界（即括号）</param>
        /// <returns>转化后的Vector2</returns>
        public static Vector2Int ConvertStrToVector2Int(string vecStr, bool hasEdge = true)
        {
            var vec = hasEdge ? vecStr.Substring(1, vecStr.Length - 1) : vecStr;
            var values = vec.Split(',');
            return new Vector2Int(int.Parse(values[0]), int.Parse(values[1]));
        }

        /// <summary>
        /// 链接成字符串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string ConvertToStrBySplit<T>(IEnumerable<T> array, string split = ",")
        {
            string res = "";

            foreach (var item in array)
            {
                res += $"{item}{split}";
            }
            res = res.Substring(0, res.Length - split.Length);

            return res;
        }

        /// <summary>
        /// 通过取概率表随机获取一个结果
        /// </summary>
        /// <typeparam name="T">结果的类型</typeparam>
        /// <param name="probabilities">概率表</param>
        /// <param name="MaxValue">随机值的最大值</param>
        /// <param name="MinValue">随机值的最小值</param>
        /// <returns>结果类型</returns>
        public static T GetResultByWeight<T>(Dictionary<T, int> probabilities, int MaxValue, int MinValue = 0)
        {
            var value = UnityEngine.Random.Range(MinValue, MaxValue * 10) % MaxValue;
            int currentValue = 0;
            return probabilities.FirstOrDefault(pair => value <= (currentValue + pair.Value)).Key;
        }

        /// <summary>
        /// 通过给定的概率随机一个结果
        /// </summary>
        /// <param name="probability">概率</param>
        /// <returns>是否成功</returns>
        public static bool GetResultByWeight(int probability)
        {
            var value = UnityEngine.Random.Range(0, ConstConfigs.MaxProbability);
            return value <= probability;
        }

        /// <summary>
        /// 获取得分（富文本）
        /// </summary>
        /// <param name="score">分数</param>
        /// <param name="frontColor">前边补齐的0颜色</param>
        /// <param name="backColor">后边的分数颜色</param>
        /// <returns>得分的富文本</returns>
        public static string GetScoreText(int score, Color frontColor, Color backColor)
        {
            StringBuilder sb = new StringBuilder();

            // 1000000是1+6位，不足6位的前方补浅色的0
            var scoreLength = score.ToString().Length;
            var zeroCount = score == 0 ? 7 : 7 - scoreLength;
            if (zeroCount > 0)
            {
                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(frontColor)}>");
                for (int i = 0; i < zeroCount; i++)
                {
                    sb.Append('0');
                }
                sb.Append("</color>");
            }

            if (zeroCount < 7)
            {
                sb.Append($"<color=#{ColorUtility.ToHtmlStringRGBA(backColor)}>{score}</color>");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取图像的宽高（只有png）
        /// </summary>
        /// <param name="bytes">图像的字节流</param>
        /// <returns>(宽,高)</returns>
        public static Vector2Int GetWidthAndHeight(byte[] bytes)
        {
            //byte[] png = new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a };
            byte[] png = new byte[] { 0x89, 0x50, 0x4e };
            // byte[] jpg = new byte[] { 0xff, 0xd8, 0xff};
            if (IsSameArray(png, bytes, png.Length))
            {
                // 是png，获取宽高
                int width = 0, height = 0;
                for (int i = 16; i < 20; i++)
                {
                    width += bytes[i];
                }
                for (int i = 20; i < 24; i++)
                {
                    height += bytes[i];
                }
                Logger.Log($"图片宽：{width}，高：{height}");
                return new Vector2Int(width, height);
            }
            Logger.Log("该图不是png格式");
            return new Vector2Int(1920, 1080);
        }

        /// <summary>
        /// 加载路径下的图片为texture2D
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns>路径下的texture2D资源</returns>
        public static Texture2D LoadTexture2DByPath(string path)
        {
            // 通过比特流读取图片数据
            byte[] bytes;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                bytes = new byte[fileStream.Length];
                fileStream.Read(bytes, 0, bytes.Length);
            }
            // 通过比特流数据创建Texture2D（图片）
            var size = Utils.GetWidthAndHeight(bytes);
            Texture2D texture = new Texture2D(size.x, size.y);
            texture.LoadImage(bytes);
            return texture;
        }

        /// <summary>
        /// 加载路径下的图片为sprite
        /// </summary>
        /// <param name="path">图片路径</param>
        /// <returns>路径下的sprite资源</returns>
        public static Sprite LoadSpriteByPath(string path)
        {
            var texture = LoadTexture2DByPath(path);
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public static Dictionary<string, string> ToDictionary<T>(T obj) where T : class
        {
            if (obj == null)
            {
                return null;
            }

            var type = obj.GetType();
            var properties = type.GetProperties();
            var fields = type.GetFields();

            return properties.Select(p => (p.Name, p.GetValue(obj)))
                .Concat(fields.Select(f => (f.Name, f.GetValue(obj))))
                .ToDictionary(pair => pair.Name, pair => pair.Item2.ToString());
        }

        public static T ConvertJsonToObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string ConvertObjectToJson<T>(T @object)
        {
            return JsonConvert.SerializeObject(@object);
        }

        public static bool IsValueType(object obj)
        {
            var type = obj.GetType();
            if (type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong)
                || type == typeof(short) || type == typeof(int) || type == typeof(long)
                || type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                return true;
            }

            return false;
        }

        public static bool IsBooleanType(object obj)
        {
            var type = obj.GetType();

            return type == typeof(bool);
        }

        public static bool IsStringType(object obj, out string value)
        {
            var type = obj.GetType();
            if (type == typeof(string) || type == typeof(StringBuilder) || type == typeof(Uri))
            {
                value = obj.ToString();
                return true;
            }
            else if (type == typeof(UriBuilder))
            {
                value = (obj as UriBuilder).Uri.ToString();
                return true;
            }

            value = null;
            return false;
        }

        public static bool IsEnumType(object obj)
        {
            var type = obj.GetType();

            return type.IsEnum;
        }
    }
}