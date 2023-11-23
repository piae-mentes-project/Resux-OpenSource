using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// һ�㹤����
/// </summary>
public static class Utils
{
    // source from: https://blog.csdn.net/u014732824/article/details/102724502
    public static string CalculateMD5(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        using (var md5 = MD5.Create())
        {
            {
                byte[] buffer = md5.ComputeHash(Encoding.UTF8.GetBytes(content));
                StringBuilder stringBuilder = new StringBuilder();
                // ѭ��������ϣ���ݵ�ÿһ���ֽڲ���ʽ��Ϊʮ�������ַ���
                for (int i = 0; i < buffer.Length; i++)
                {
                    stringBuilder.Append(buffer[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }
    }

    /// <summary>
    /// ������ֶκ�����һ��ת��Ϊ�ֵ�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
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
}
