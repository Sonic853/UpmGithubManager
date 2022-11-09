using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Sonic853.UpmGithubManager
{
    class GithubAPI
    {
        /// <summary>
        /// 查询用的 Token，如果不填写，每小时只能查询 60 次
        /// </summary>
        public static string Token
        {
            get => EditorPrefs.GetString("Sonic853.UpmGithubManager.GithubAPI.Token", "");
            set => EditorPrefs.SetString("Sonic853.UpmGithubManager.GithubAPI.Token", value);
        }
        /// <summary>
        /// 获取所有 Tag
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string[]> GetTags(string url)
        {
            // url = https://github.com/Username/Repo.git
            // 从 url 获取 Username/Repo，去掉 .git
            string[] urlSplit = url.Split('/');
            string repo = urlSplit[urlSplit.Length - 1];
            if (repo.EndsWith(".git"))
            {
                repo = repo.Substring(0, repo.Length - 4);
            }
            string username = urlSplit[urlSplit.Length - 2];
            // https://api.github.com/repos/Username/Repo/tags
            string apiUrl = "https://api.github.com/repos/" + username + "/" + repo + "/tags";
            UnityWebRequest www = UnityWebRequest.Get(apiUrl);
            if (Token != null)
            {
                www.SetRequestHeader("Authorization", "Bearer " + Token);
            }
            // 不使用 await www.SendWebRequest();
            www.SendWebRequest();
            while (!www.isDone)
            {
                await Task.Yield();
            }
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                return new string[0];
            }
            else
            {
                var _result = JsonConvert.DeserializeObject<GithubAPITag[]>(www.downloadHandler.text);
                string[] tags = new string[_result.Length];
                for (int i = 0; i < _result.Length; i++)
                {
                    tags[i] = _result[i].name;
                }
                return tags;
            }
        }
        public static async Task<string[]> GetBranches(string url)
        {
            // url = https://github.com/Username/Repo.git
            // 从 url 获取 Username/Repo，去掉 .git
            string[] urlSplit = url.Split('/');
            string repo = urlSplit[urlSplit.Length - 1];
            if (repo.EndsWith(".git"))
            {
                repo = repo.Substring(0, repo.Length - 4);
            }
            string username = urlSplit[urlSplit.Length - 2];
            // https://api.github.com/repos/Username/Repo/branches
            string apiUrl = "https://api.github.com/repos/" + username + "/" + repo + "/branches";
            UnityWebRequest www = UnityWebRequest.Get(apiUrl);
            if (Token != null)
            {
                www.SetRequestHeader("Authorization", "Bearer " + Token);
            }
            www.SendWebRequest();
            while (!www.isDone)
            {
                await Task.Yield();
            }
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                return new string[0];
            }
            else
            {
                var _result = JsonConvert.DeserializeObject<GithubAPIBranche[]>(www.downloadHandler.text);
                string[] branches = new string[_result.Length];
                for (int i = 0; i < _result.Length; i++)
                {
                    branches[i] = _result[i].name;
                }
                return branches;
            }
        }
    }
}
