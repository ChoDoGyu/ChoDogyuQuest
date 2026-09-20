using UnityEditor;
using UnityEngine;

namespace CDG.Quest.Editor
{
    /// <summary>
    /// QuestCatalog에 정의된 Quest를 탐색하고 관리하기 위한 Quest Framework 전용 EditorWindow입니다.
    /// Quest 목록과 선택한 Quest 정보를 분리된 패널로 표시하며 이후 편집, 검증 및 관계 탐색 기능의 진입점으로 사용됩니다.
    /// </summary>
    internal sealed class QuestEditorWindow : EditorWindow
    {
        private const string MenuPath = "Tools/ChoDogyu/Quest Editor";
        private const string WindowTitle = "CDG Quest";
        private const float LeftPanelWidth = 260f;

        [SerializeField]
        private QuestCatalog catalog;

        [SerializeField]
        private int selectedQuestIndex = -1;

        private Vector2 leftScrollPosition;
        private Vector2 rightScrollPosition;

        [MenuItem(MenuPath)]
        private static void Open()
        {
            QuestEditorWindow window = GetWindow<QuestEditorWindow>();
            window.ConfigureWindow();

            if (window.catalog == null && Selection.activeObject is QuestCatalog selectedCatalog)
            {
                window.ChangeCatalog(selectedCatalog);
            }

            window.Show();
        }

        private void OnEnable()
        {
            ConfigureWindow();
            EnsureSelection();
        }

        private void OnProjectChange()
        {
            EnsureSelection();
            Repaint();
        }

        private void OnGUI()
        {
            EnsureSelection();

            DrawHeader();

            EditorGUILayout.Space(8f);

            DrawCatalogSection();

            EditorGUILayout.Space(8f);

            if (catalog == null)
            {
                EditorGUILayout.HelpBox("편집할 QuestCatalog을 선택하세요.", MessageType.Info);
                return;
            }

            DrawMainContent();
        }

        private void ConfigureWindow()
        {
            titleContent = new GUIContent(WindowTitle);
            minSize = new Vector2(760f, 460f);
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("ChoDogyu Quest Framework & Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("QuestCatalog에 정의된 Quest를 탐색하고 관리합니다.", EditorStyles.wordWrappedLabel);
        }

        private void DrawCatalogSection()
        {
            EditorGUILayout.LabelField("Catalog", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            QuestCatalog newCatalog = (QuestCatalog)EditorGUILayout.ObjectField("Quest Catalog", catalog, typeof(QuestCatalog), false);

            if (GUILayout.Button("Use Selection", GUILayout.Width(100f)))
            {
                UseCurrentSelection();
                newCatalog = catalog;
            }

            EditorGUILayout.EndHorizontal();

            if (newCatalog != catalog)
            {
                ChangeCatalog(newCatalog);
                GUI.FocusControl(null);
            }

            if (catalog == null)
            {
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Quest Count", catalog.Count);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawMainContent()
        {
            EditorGUILayout.BeginHorizontal();

            DrawQuestListPanel();

            GUILayout.Space(8f);

            DrawQuestDetailsPanel();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawQuestListPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(LeftPanelWidth), GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Quests", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            leftScrollPosition = EditorGUILayout.BeginScrollView(leftScrollPosition);

            if (catalog.Count == 0)
            {
                EditorGUILayout.HelpBox("Catalog에 등록된 Quest가 없습니다.", MessageType.Info);
            }
            else
            {
                DrawQuestList();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuestList()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };

            for (int questIndex = 0; questIndex < catalog.Quests.Count; questIndex++)
            {
                QuestDefinition quest = catalog.Quests[questIndex];
                bool isSelected = questIndex == selectedQuestIndex;
                string label = CreateQuestListLabel(quest, questIndex, isSelected);

                if (GUILayout.Button(label, buttonStyle, GUILayout.MinHeight(32f)))
                {
                    selectedQuestIndex = questIndex;
                    GUI.FocusControl(null);
                }
            }
        }

        private void DrawQuestDetailsPanel()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Quest Details", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            rightScrollPosition = EditorGUILayout.BeginScrollView(rightScrollPosition);

            if (!TryGetSelectedQuest(out QuestDefinition quest))
            {
                EditorGUILayout.HelpBox("왼쪽 목록에서 Quest를 선택하세요.", MessageType.Info);
            }
            else if (quest == null)
            {
                EditorGUILayout.HelpBox("선택한 Quest 항목이 null입니다.", MessageType.Error);
            }
            else
            {
                DrawSelectedQuest(quest);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private static void DrawSelectedQuest(QuestDefinition quest)
        {
            EditorGUI.BeginDisabledGroup(true);

            EditorGUILayout.TextField("ID", quest.Id);
            EditorGUILayout.TextField("Title", quest.Title);

            EditorGUILayout.LabelField("Description");
            EditorGUILayout.TextArea(quest.Description ?? string.Empty, GUILayout.MinHeight(70f));

            EditorGUILayout.Space(6f);

            EditorGUILayout.IntField("Objectives", quest.Objectives.Count);
            EditorGUILayout.IntField("Prerequisites", quest.PrerequisiteQuestIds.Count);
            EditorGUILayout.IntField("Rewards", quest.Rewards.Count);
            EditorGUILayout.Toggle("Repeatable", quest.IsRepeatable);

            EditorGUI.EndDisabledGroup();
        }

        private void UseCurrentSelection()
        {
            if (Selection.activeObject is not QuestCatalog selectedCatalog)
            {
                EditorUtility.DisplayDialog("Quest Editor", "현재 선택된 Asset이 QuestCatalog가 아닙니다.", "확인");
                return;
            }

            ChangeCatalog(selectedCatalog);
            GUI.FocusControl(null);
        }

        private void ChangeCatalog(QuestCatalog newCatalog)
        {
            catalog = newCatalog;
            leftScrollPosition = Vector2.zero;
            rightScrollPosition = Vector2.zero;

            if (catalog == null || catalog.Count == 0)
            {
                selectedQuestIndex = -1;
                return;
            }

            selectedQuestIndex = 0;
        }

        private void EnsureSelection()
        {
            if (catalog == null || catalog.Count == 0)
            {
                selectedQuestIndex = -1;
                return;
            }

            if (selectedQuestIndex < 0 || selectedQuestIndex >= catalog.Count)
            {
                selectedQuestIndex = 0;
            }
        }

        private bool TryGetSelectedQuest(out QuestDefinition quest)
        {
            quest = null;

            if (catalog == null || selectedQuestIndex < 0 || selectedQuestIndex >= catalog.Count)
            {
                return false;
            }

            quest = catalog.Quests[selectedQuestIndex];
            return true;
        }

        private static string CreateQuestListLabel(QuestDefinition quest, int questIndex, bool isSelected)
        {
            string prefix = isSelected ? "▶ " : string.Empty;

            if (quest == null)
            {
                return $"{prefix}{questIndex + 1}. (Null Quest)";
            }

            string id = string.IsNullOrWhiteSpace(quest.Id) ? "(No ID)" : quest.Id;
            string title = string.IsNullOrWhiteSpace(quest.Title) ? "(No Title)" : quest.Title;

            return $"{prefix}{id}\n{title}";
        }
    }
}