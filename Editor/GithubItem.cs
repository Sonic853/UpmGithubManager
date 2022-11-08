using System.Collections.Generic;

namespace Sonic853.UpmGithubManager
{
        class GithubItem
        {
            /// <summary>
            /// 包名
            /// </summary>
            public string name;
            string _url;
            /// <summary>
            /// URL
            /// </summary>
            // url = https://github.com/Username/Repo.git?path=/Packages/com.your.package#6.6.6
            public string url
            {
                get
                {
                    return _url + "?path=" + path + (version == "#latest#" ? "" : ("#" + version));
                }
                set
                {
                    oldUrl = value;
                    string[] urlSplit = value.Split('#');
                    if (urlSplit.Length > 1)
                    {
                        version = urlSplit[1];
                    }
                    else
                    {
                        version = "#latest#";
                    }
                    // args
                    string[] argsSplit = urlSplit[0].Split('?');
                    args.Clear();
                    path = "";
                    _url = argsSplit[0];
                    if (argsSplit.Length > 1)
                    {
                        string[] argList = argsSplit[1].Split('&');
                        foreach (string arg in argList)
                        {
                            string[] argSplit = arg.Split('=');
                            if (argSplit.Length > 1)
                            {
                                args.Add(argSplit[0], argSplit[1]);
                                if (argSplit[0] == "path")
                                {
                                    path = argSplit[1];
                                }
                            }
                            else
                            {
                                args.Add(argSplit[0], "");
                            }
                        }
                    }
                }
            }
            public string sourceUrl
            {
                get
                {
                    return _url;
                }
            }
            public string oldUrl;
            public bool customVersion;
            public string version;
            public string[] tags;
            public string[] branches;
            /// <summary>
            /// 参数
            /// </summary>
            Dictionary<string, string> args = new Dictionary<string, string>();
            /// <summary>
            /// 路径
            /// </summary>
            public string path;
        }
}
