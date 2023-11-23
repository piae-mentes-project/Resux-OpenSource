using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public class DependencyResolver : MonoBehaviour
{
    private static string GenerateRepoUrl(string org, string name, string version, string type) =>
        $"https://repo1.maven.org/maven2/{org.Replace('.', '/')}/{name}/{version}/{name}-{version}.{type}";
    private static HttpClient httpClient = new HttpClient();
    private static async void ResolveAndroidDependencies()
    {
        var cfg = JsonConvert.DeserializeObject<AndroidDependencyConfig>(
            File.ReadAllText("Assets/Editor/AndroidDependencies.json"));

        // mvn dependency
        foreach (var dependency in cfg.MavenDependencies)
        {
            var detail = dependency.Split(':');
            if (detail.Length != 4)
            {
                Debug.LogError("Failed to resolve: " + dependency);
                continue;
            }

            var url = GenerateRepoUrl(detail[0], detail[1], detail[2], detail[3]);
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError("Failed to resolve: " + dependency + ", http status: " + response.StatusCode);
                continue;
            }

            var fileName = Path.GetFileName(url);
            var fileStream = File.OpenWrite("Assets/Plugins/Android/" + fileName);
            await response.Content.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            fileStream.Close();

            Debug.Log("Resolved: " + dependency);
        }
    }

    [MenuItem("Tools/Resolve Dependencies")]
    public static void Resolve()
    {
#if UNITY_ANDROID
        ResolveAndroidDependencies();
#endif
    }

    private class AndroidDependencyConfig
    {
        public string[] MavenDependencies = Array.Empty<string>();
    }
}
