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
        static List<GithubItem> githubItems = new List<GithubItem>();
        static GithubItem selectedItem;
        static EditPanelUI editPanel;
        static List<string> versions = new List<string>(){
            "custom"
        };
        static MainWindow instance;
        static string manifestText;
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
            TextField githubToken = new TextField("Github Token")
            {
                value = GithubAPI.Token
            };
            githubToken.AddToClassList("GithubToken");
            githubToken.RegisterCallback<ChangeEvent<string>>((evt) =>
            {
                GithubAPI.Token = evt.newValue;
            });
            root.Add(githubToken);
            Button getToken = new Button(() =>
            {
                Application.OpenURL("https://github.com/settings/tokens");
            })
            {
                text = "Get Token"
            };
            getToken.AddToClassList("GetToken");
            root.Add(getToken);
            // 从 Packages/manifest.json 中读取所有的包
            string manifestPath = Application.dataPath + "/../Packages/manifest.json";
            manifestText = System.IO.File.ReadAllText(manifestPath);
            manifest manifest = JsonConvert.DeserializeObject<manifest>(manifestText);
            foreach (KeyValuePair<string, string> item in manifest.dependencies)
            {
                if (item.Value.StartsWith("https://github.com"))
                {
                    // Debug.Log(item.Key + " " + item.Value);
                    uGMui.githubItems.Add(new GithubItem()
                    {
                        name = item.Key,
                        url = item.Value
                    });
                }
            }
            uGMui.githubList.Refresh();
            uGMui.githubList.onItemChosen += (item) =>
            {
                selectedItem = item as GithubItem;
                editPanel.SetItem(selectedItem).ContinueWith((task) => { });
            };
            editPanel.saveButton.clicked += () =>
            {
                if (selectedItem != null)
                {
                    string newUrl = editPanel.urlField.value;
                    newUrl += editPanel.pathField.value == "" ? "" : ("?path=" + editPanel.pathField.value);
                    newUrl += editPanel.versionField.value == "" ? "" : ("#" + editPanel.versionField.value);
                    if (newUrl != selectedItem.url)
                    {
                        manifest.dependencies[selectedItem.name] = newUrl;
                        foreach (var item in githubItems)
                        {
                            if (item.name == selectedItem.name)
                            {
                                item.url = newUrl;
                                break;
                            }
                        }
                        selectedItem.url = newUrl;
                        string newManifestText = JsonConvert.SerializeObject(manifest, Formatting.Indented);
                        System.IO.File.WriteAllText(manifestPath, newManifestText);
                        Debug.Log("[UPM Github Manager]: Save success!");
                        uGMui.githubList.Refresh();
                        AssetDatabase.Refresh();
                    }
                }
            };
            // Debug.Log(uGMui.githubList.itemsSource.Count);
        }
        UGMUI CreateUI()
        {
            VisualElement root = new VisualElement();
            root.AddToClassList("UGMMain");
            ListView githubList = new ListView(){
                itemHeight = 35,
                makeItem = makeGithubItem,
                bindItem = bindGithubItem,
                itemsSource = githubItems,
                selectionType = SelectionType.Single,
            };
            githubList.AddToClassList("GithubList");
            githubItems.Clear();
            root.Add(githubList);
            root.Add(DragLine.CreateDragLine(githubList));
            editPanel = CreateEditPanelUI();
            root.Add(editPanel.root);
            return new UGMUI()
            {
                root = root,
                githubList = githubList,
                githubItems = githubItems
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
        class UGMUI
        {
            /// <summary>
            /// 根节点
            /// </summary>
            public VisualElement root;
            /// <summary>
            /// Github 分组
            /// </summary>
            public ListView githubList;
            public List<GithubItem> githubItems;
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
            public async Task SetItem(GithubItem item)
            {
                nameField.value = item.name;
                urlField.value = item.sourceUrl;
                pathField.value = item.path;
                versionField.value = item.version == "#latest#" ? "" : item.version;
                versionsField.SetEnabled(false);
                saveButton.SetEnabled(false);
                var tags = await GithubAPI.GetTags(item.sourceUrl);
                var branches = await GithubAPI.GetBranches(item.sourceUrl);
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
        static Func<VisualElement> makeGithubItem = () =>
        {
            Label item = new Label();
            item.AddToClassList("githubItem");
            Label nameLabel = new Label("Name: ");
            nameLabel.name = "name";
            item.Add(nameLabel);
            Label versionLabel = new Label("Version: ");
            versionLabel.name = "version";
            item.Add(versionLabel);
            return item;
        };
        static Action<VisualElement, int> bindGithubItem = (VisualElement item, int index) =>
        {
            GithubItem githubItem = githubItems[index];
            Label nameLabel = item.Q<Label>("name");
            Label versionLabel = item.Q<Label>("version");
            nameLabel.text = "Name: " + githubItems[index].name;
            versionLabel.text = "Version: " + (githubItems[index].version == "#latest#" ? "latest" : githubItems[index].version);
        };
    }
}
