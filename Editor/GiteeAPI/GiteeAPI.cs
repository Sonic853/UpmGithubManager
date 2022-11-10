using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Sonic853.UpmGithubManager
{
    public class GiteeAPI
    {
        /// <summary>
        /// 查询用的 Token，如果不填写，每小时只能查询 60 次
        /// </summary>
        public static string Token
        {
            get => EditorPrefs.GetString("Sonic853.UpmGithubManager.GiteeAPI.Token", "");
            set => EditorPrefs.SetString("Sonic853.UpmGithubManager.GiteeAPI.Token", value);
        }
        /// <summary>
        /// 获取所有 Tag
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string[]> GetTags(string url)
        {
            // url = https://gitee.com/Username/Repo.git
            // url = ssh://git@gitee.com/Username/Repo.git
            // 从 url 获取 Username/Repo，去掉 .git
            string[] urlSplit = url.Split('/');
            string repo = urlSplit[urlSplit.Length - 1];
            if (repo.EndsWith(".git"))
            {
                repo = repo.Substring(0, repo.Length - 4);
            }
            string username = urlSplit[urlSplit.Length - 2];
            // https://gitee.com/api/v5/repos/Username/Repo/tags
            string apiUrl = string.Format("https://gitee.com/api/v5/repos/{0}/{1}/tags", username, repo);
            if (Token != null)
            {
                apiUrl += "?access_token=" + Token;
            }
            UnityWebRequest www = UnityWebRequest.Get(apiUrl);
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
                var _result = JsonConvert.DeserializeObject<GiteeAPITag[]>(www.downloadHandler.text);
                string[] tags = new string[_result.Length];
                for (int i = 0; i < _result.Length; i++)
                {
                    tags[i] = _result[i].name;
                }
                return tags;
            }
        }
        /// <summary>
        /// 获取所有 Branch
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string[]> GetBranches(string url)
        {
            string[] urlSplit = url.Split('/');
            string repo = urlSplit[urlSplit.Length - 1];
            if (repo.EndsWith(".git"))
            {
                repo = repo.Substring(0, repo.Length - 4);
            }
            string username = urlSplit[urlSplit.Length - 2];
            string apiUrl = string.Format("https://gitee.com/api/v5/repos/{0}/{1}/branches", username, repo);
            if (Token != null)
            {
                apiUrl += "?access_token=" + Token;
            }
            UnityWebRequest www = UnityWebRequest.Get(apiUrl);
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
                var _result = JsonConvert.DeserializeObject<GiteeAPIBranche[]>(www.downloadHandler.text);
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
