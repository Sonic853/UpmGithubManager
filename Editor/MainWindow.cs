using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sonic853.UpmGithubManager
{
    class MainWindow : EditorWindow
    {
        private const string ResourcesPath = "Packages/com.sonic853.upm-github-manager/Editor/StyleSheets/";
        private const string s_StyleSheetPath = ResourcesPath + "MainWindow.uss";
        static List<GitItem> gitItems = new List<GitItem>();
        static GitItem selectedItem;
        static EditPanelUI editPanel;
        static List<string> versions = new List<string>(){
            "custom"
        };
        static string manifestText;
        static MainWindow instance;
        public static MainWindow getInstance
        {
            get
            {
                return instance;
            }
        }
        public static MainWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    OpenMainWindow();
                }
                return instance;
            }
        }
        public static readonly string OpenWindowCommand = nameof(OpenMainWindowCommand);
        [MenuItem("853Lab/UPM Github Manager", false, 3010)]
        public static void OpenMainWindowCommand()
        {
            if (CommandService.Exists(OpenWindowCommand))
                CommandService.Execute(OpenWindowCommand, CommandHint.Menu);
            else
            {
                OpenMainWindow();
            }
        }
        public static void OpenMainWindow()
        {
            instance = GetWindow<MainWindow>();
            instance.minSize = new Vector2(450, 240);
            instance.titleContent = new GUIContent("UPM Github Manager");
        }
        public void OnEnable()
        {
            VisualElement root = rootVisualElement;
            root.styleSheets.Add(EditorGUIUtility.Load(s_StyleSheetPath) as StyleSheet);
            UGMUI uGMui = CreateUI();
            root.Add(uGMui.root);
            var tokenPanel = CreateTokenUI();
            root.Add(tokenPanel);
            // 从 Packages/manifest.json 中读取所有的包
            string manifestPath = Application.dataPath + "/../Packages/manifest.json";
            manifestText = System.IO.File.ReadAllText(manifestPath);
            manifest manifest = JsonConvert.DeserializeObject<manifest>(manifestText);
            foreach (KeyValuePair<string, string> item in manifest.dependencies)
            {
                if (item.Value.StartsWith("https://github.com")
                || item.Value.StartsWith("ssh://git@github.com")
                || item.Value.StartsWith("https://gitee.com")
                || item.Value.StartsWith("ssh://git@gitee.com"))
                {
                    // Debug.Log(item.Key + " " + item.Value);
                    uGMui.gitItems.Add(new GitItem()
                    {
                        name = item.Key,
                        url = item.Value
                    });
                }
            }
            uGMui.gitList.Refresh();
            uGMui.gitList.onItemChosen += (item) =>
            {
                selectedItem = item as GitItem;
                editPanel.SetItem(selectedItem).ContinueWith((task) => { });
            };
            editPanel.saveButton.clicked += () =>
            {
                if (selectedItem != null)
                {
                    string newUrl = editPanel.urlField.value.Trim();
                    newUrl += string.IsNullOrEmpty(editPanel.pathField.value.Trim()) ? "" : ("?path=" + editPanel.pathField.value.Trim());
                    newUrl += string.IsNullOrEmpty(editPanel.versionField.value.Trim()) ? "" : ("#" + editPanel.versionField.value.Trim());
                    if (newUrl != selectedItem.url
                    && !string.IsNullOrEmpty(newUrl))
                    {
                        // manifest.dependencies[selectedItem.name] = newUrl;
                        manifestText = manifestText.Replace(selectedItem.oldUrl, newUrl);
                        foreach (var item in gitItems)
                        {
                            if (item.name == selectedItem.name)
                            {
                                item.url = newUrl;
                                break;
                            }
                        }
                        selectedItem.url = newUrl;
                        System.IO.File.WriteAllText(manifestPath, manifestText);
                        // string newManifestText = JsonConvert.SerializeObject(manifest, Formatting.Indented);
                        // System.IO.File.WriteAllText(manifestPath, newManifestText);
                        Debug.Log("[UPM Github Manager]: Save success!");
                        uGMui.gitList.Refresh();
                        AssetDatabase.Refresh();
                    }
                }
            };
            // Debug.Log(uGMui.gitList.itemsSource.Count);
        }
        UGMUI CreateUI()
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("UGMMain");
            ListView gitList = new ListView(){
                itemHeight = 35,
                makeItem = makeGitItem,
                bindItem = bindGitItem,
                itemsSource = gitItems,
                selectionType = SelectionType.Single,
            };
            gitList.AddToClassList("GitList");
            gitItems.Clear();
            root.Add(gitList);
            root.Add(DragLine.CreateDragLine(gitList));
            editPanel = CreateEditPanelUI();
            root.Add(editPanel.root);
            return new UGMUI()
            {
                root = root,
                gitList = gitList,
                gitItems = gitItems
            };
        }
        EditPanelUI CreateEditPanelUI()
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("EditPanel");
            TextField nameField = new TextField("Name"){
                isReadOnly = true
            };
            nameField.AddToClassList("name");
            root.Add(nameField);
            TextField urlField = new TextField("Url");
            urlField.AddToClassList("url");
            root.Add(urlField);
            TextField pathField = new TextField("Path");
            pathField.AddToClassList("path");
            root.Add(pathField);
            TextField versionField = new TextField("Version");
            versionField.AddToClassList("version");
            root.Add(versionField);
            VisualElement versionsVE = new VisualElement();
            versionsVE.AddToClassList("versions");
            PopupField<string> versionsField = new PopupField<string>("Select Version");
            versionsField.SetEnabled(false);
            versionsVE.Add(versionsField);
            root.Add(versionsVE);
            Button saveButton = new Button(){
                text = "Save"
            };
            saveButton.AddToClassList("save");
            saveButton.SetEnabled(false);
            urlField.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                saveButton.SetEnabled(true);
            });
            pathField.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                saveButton.SetEnabled(true);
            });
            versionField.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                // 判断焦点是否在版本输入框
                if (versionField.panel.focusController.focusedElement == versionField)
                {
                    editPanel.versionsField.value = "custom";
                }
                saveButton.SetEnabled(true);
            });
            root.Add(saveButton);
            return new EditPanelUI()
            {
                root = root,
                nameField = nameField,
                urlField = urlField,
                pathField = pathField,
                versionField = versionField,
                versionsVE = versionsVE,
                versionsField = versionsField,
                saveButton = saveButton
            };
        }
        VisualElement CreateTokenUI()
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("TokenPanel");
            VisualElement ghTPanel = new VisualElement();
            ghTPanel.AddToClassList("GitHubTokenPanel");
            TextField githubToken = new TextField("Github Token")
            {
                value = GithubAPI.Token,
                isPasswordField = true
            };
            githubToken.AddToClassList("GithubToken");
            githubToken.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                GithubAPI.Token = evt.newValue;
            });
            ghTPanel.Add(githubToken);
            Button ghGetToken = new Button(() =>
            {
                Application.OpenURL("https://github.com/settings/tokens");
            })
            {
                text = "Get Token"
            };
            ghGetToken.AddToClassList("GetToken");
            ghTPanel.Add(ghGetToken);
            root.Add(ghTPanel);
            VisualElement geTPanel = new VisualElement();
            geTPanel.AddToClassList("GiteeTokenPanel");
            TextField giteeToken = new TextField("Gitee Token")
            {
                value = GiteeAPI.Token,
                isPasswordField = true
            };
            giteeToken.AddToClassList("GiteeToken");
            giteeToken.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                GiteeAPI.Token = evt.newValue;
            });
            geTPanel.Add(giteeToken);
            Button geGetToken = new Button(() =>
            {
                Application.OpenURL("https://gitee.com/profile/personal_access_tokens");
            })
            {
                text = "Get Token"
            };
            geGetToken.AddToClassList("GetToken");
            geTPanel.Add(geGetToken);
            root.Add(geTPanel);
            return root;
        }
        class UGMUI
        {
            /// <summary>
            /// 根节点
            /// </summary>
            public VisualElement root;
            /// <summary>
            /// Github 分组
            /// </summary>
            public ListView gitList;
            public List<GitItem> gitItems;
        }
        class EditPanelUI
        {
            /// <summary>
            /// 根节点
            /// </summary>
            public VisualElement root;
            public TextField nameField;
            public TextField urlField;
            public TextField pathField;
            public TextField versionField;
            public VisualElement versionsVE;
            public PopupField<string> versionsField;
            public Button saveButton;
            public async Task SetItem(GitItem item)
            {
                nameField.value = item.name;
                urlField.value = item.sourceUrl;
                pathField.value = item.path;
                versionField.value = item.version == "#latest#" ? "" : item.version;
                versionsField.SetEnabled(false);
                saveButton.SetEnabled(false);
                var tags = new string[0];
                var branches = new string[0];
                if (item.sourceUrl.StartsWith("https://github.com/")
                || item.sourceUrl.StartsWith("ssh://git@github.com/"))
                {
                    tags = await GithubAPI.GetTags(item.sourceUrl);
                    branches = await GithubAPI.GetBranches(item.sourceUrl);
                }
                else if (item.sourceUrl.StartsWith("https://gitee.com/")
                || item.sourceUrl.StartsWith("ssh://git@gitee.com/"))
                {
                    tags = await GiteeAPI.GetTags(item.sourceUrl);
                    branches = await GiteeAPI.GetBranches(item.sourceUrl);
                }
                versions.Clear();
                versions.Add("custom");
                foreach (var tag in tags)
                {
                    versions.Add("[tag]" + tag);
                }
                foreach (var branch in branches)
                {
                    versions.Add("[branch]" + branch);
                }
                if (tags.Length != 0 || branches.Length != 0)
                {
                    // 移除 versionsField
                    versionsVE.Remove(versionsField);
                    int index = versions.IndexOf("[tag]" + versionField.value);
                    index = index == -1 ? versions.IndexOf("[branch]" + versionField.value) : index;
                    index = index == -1 ? 0 : index;
                    versionsField = new PopupField<string>("Select Version", versions, (item.version == "#latest#" ? 0 : index));
                    versionsField.SetEnabled(true);
                    versionsField.formatSelectedValueCallback = (value) =>
                    {
                        saveButton.SetEnabled(true);
                        if (value == "custom")
                        {
                            return value;
                        }
                        else if (value.StartsWith("[tag]"))
                        {
                            versionField.value = value.Substring(5);
                            // return "tag: " + value.Substring(5);
                        }
                        else if (value.StartsWith("[branch]"))
                        {
                            versionField.value = value.Substring(8);
                            // return "branch: " + value.Substring(8);
                        }
                        return value;
                    };
                    versionsVE.Add(versionsField);
                }
                saveButton.SetEnabled(false);
                // return item;
            }
        }
        static Func<VisualElement> makeGitItem = () =>
        {
            Label item = new Label();
            item.AddToClassList("gitItem");
            Label nameLabel = new Label("Name: ");
            nameLabel.name = "name";
            item.Add(nameLabel);
            Label versionLabel = new Label("Version: ");
            versionLabel.name = "version";
            item.Add(versionLabel);
            return item;
        };
        static Action<VisualElement, int> bindGitItem = (VisualElement item, int index) =>
        {
            GitItem gitItem = gitItems[index];
            Label nameLabel = item.Q<Label>("name");
            Label versionLabel = item.Q<Label>("version");
            nameLabel.text = "Name: " + gitItems[index].name;
            versionLabel.text = "Version: " + (gitItems[index].version == "#latest#" ? "latest" : gitItems[index].version);
        };
    }
}
