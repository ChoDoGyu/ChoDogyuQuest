using UnityEditor;
using UnityEngine;

namespace CDG.Quest.Editor
{
    /// <summary>
    /// QuestCatalog에 정의된 Quest를 탐색하고 관리하기 위한 Quest Framework 전용 EditorWindow입니다.
    /// Quest 목록과 선택한 Quest를 분리된 패널로 제공하며 세부 편집은 전용 Definition GUI에 위임합니다.
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

        private QuestCatalogEditorSession editorSession;
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
            RestoreEditorSession();

            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;

            EnsureSelection();
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnProjectChange()
        {
            RestoreEditorSession();
            EnsureSelection();
            Repaint();
        }

        private void OnGUI()
        {
            EnsureEditorSession();
            editorSession?.Update();
            EnsureSelection();

            DrawHeader();

            EditorGUILayout.Space(8f);

            DrawCatalogSection();

            EditorGUILayout.Space(8f);

            if (catalog == null || editorSession == null)
            {
                EditorGUILayout.HelpBox("편집할 QuestCatalog을 선택하세요.", MessageType.Info);
                return;
            }

            DrawMainContent();

            if (editorSession.ApplyModifiedProperties())
            {
                Repaint();
            }
        }

        private void ConfigureWindow()
        {
            titleContent = new GUIContent(WindowTitle);
            minSize = new Vector2(820f, 520f);
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("ChoDogyu Quest Framework & Editor", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Quest Definition, Objective, 선행 관계 및 Reward 메타데이터를 하나의 Catalog에서 관리합니다.",
                EditorStyles.wordWrappedLabel);
        }

        private void DrawCatalogSection()
        {
            EditorGUILayout.LabelField("Catalog", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            QuestCatalog newCatalog = (QuestCatalog)EditorGUILayout.ObjectField(
                "Quest Catalog",
                catalog,
                typeof(QuestCatalog),
                false);

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

            if (catalog == null || editorSession == null)
            {
                return;
            }

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Quest Count", editorSession.QuestCount);
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            string stateText = editorSession.IsDirty ? "Unsaved Changes" : "Saved";
            EditorGUILayout.LabelField(stateText, GUILayout.Width(110f));

            EditorGUI.BeginDisabledGroup(!editorSession.IsDirty);

            if (GUILayout.Button("Save Catalog", GUILayout.Width(100f)))
            {
                editorSession.Save();
                GUI.FocusControl(null);
                Repaint();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
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
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox,
                GUILayout.Width(LeftPanelWidth),
                GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Quests", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            leftScrollPosition = EditorGUILayout.BeginScrollView(leftScrollPosition);

            if (editorSession.QuestCount == 0)
            {
                EditorGUILayout.HelpBox("Catalog에 등록된 Quest가 없습니다.", MessageType.Info);
            }
            else
            {
                DrawQuestList();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4f);

            if (GUILayout.Button("Add Quest", GUILayout.Height(26f)))
            {
                AddQuest();
            }

            EditorGUI.BeginDisabledGroup(
                selectedQuestIndex < 0 ||
                selectedQuestIndex >= editorSession.QuestCount);

            if (GUILayout.Button("Delete Quest", GUILayout.Height(24f)))
            {
                DeleteSelectedQuest();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuestList()
        {
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };

            for (int questIndex = 0; questIndex < editorSession.QuestCount; questIndex++)
            {
                SerializedProperty questProperty = editorSession.GetQuestProperty(questIndex);
                bool isSelected = questIndex == selectedQuestIndex;
                string label = CreateQuestListLabel(questProperty, questIndex, isSelected);

                if (GUILayout.Button(label, buttonStyle, GUILayout.MinHeight(32f)))
                {
                    selectedQuestIndex = questIndex;
                    rightScrollPosition = Vector2.zero;
                    GUI.FocusControl(null);
                }
            }
        }

        private void DrawQuestDetailsPanel()
        {
            EditorGUILayout.BeginVertical(
                EditorStyles.helpBox,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            EditorGUILayout.LabelField("Quest Details", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            rightScrollPosition = EditorGUILayout.BeginScrollView(rightScrollPosition);

            if (editorSession.GetQuestProperty(selectedQuestIndex) == null)
            {
                EditorGUILayout.HelpBox("왼쪽 목록에서 Quest를 선택하세요.", MessageType.Info);
            }
            else
            {
                QuestDefinitionEditorGUI.Draw(editorSession, selectedQuestIndex);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void AddQuest()
        {
            int newIndex = editorSession.AddQuest();

            if (newIndex < 0)
            {
                return;
            }

            selectedQuestIndex = newIndex;
            leftScrollPosition = Vector2.zero;
            rightScrollPosition = Vector2.zero;

            GUI.FocusControl(null);
            Repaint();
        }

        private void DeleteSelectedQuest()
        {
            SerializedProperty selectedQuest = editorSession.GetQuestProperty(selectedQuestIndex);

            if (selectedQuest == null)
            {
                return;
            }

            SerializedProperty idProperty = selectedQuest.FindPropertyRelative("id");
            string questId = string.IsNullOrWhiteSpace(idProperty.stringValue)
                ? "(No ID)"
                : idProperty.stringValue;

            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Quest",
                $"Quest '{questId}'를 삭제하시겠습니까?\n\n이 작업은 Unity Undo로 되돌릴 수 있습니다.",
                "Delete",
                "Cancel");

            if (!confirmed)
            {
                return;
            }

            selectedQuestIndex = editorSession.DeleteQuest(selectedQuestIndex);
            rightScrollPosition = Vector2.zero;

            GUI.FocusControl(null);
            Repaint();
        }

        private void UseCurrentSelection()
        {
            if (Selection.activeObject is not QuestCatalog selectedCatalog)
            {
                EditorUtility.DisplayDialog(
                    "Quest Editor",
                    "현재 선택된 Asset이 QuestCatalog가 아닙니다.",
                    "확인");

                return;
            }

            ChangeCatalog(selectedCatalog);
            GUI.FocusControl(null);
        }

        private void ChangeCatalog(QuestCatalog newCatalog)
        {
            catalog = newCatalog;
            editorSession = catalog != null
                ? new QuestCatalogEditorSession(catalog)
                : null;

            leftScrollPosition = Vector2.zero;
            rightScrollPosition = Vector2.zero;

            if (editorSession == null || editorSession.QuestCount == 0)
            {
                selectedQuestIndex = -1;
                return;
            }

            selectedQuestIndex = 0;
        }

        private void RestoreEditorSession()
        {
            editorSession = catalog != null
                ? new QuestCatalogEditorSession(catalog)
                : null;
        }

        private void EnsureEditorSession()
        {
            if (catalog == null)
            {
                editorSession = null;
                return;
            }

            if (editorSession == null || editorSession.Catalog != catalog)
            {
                editorSession = new QuestCatalogEditorSession(catalog);
            }
        }

        private void EnsureSelection()
        {
            if (editorSession == null || editorSession.QuestCount == 0)
            {
                selectedQuestIndex = -1;
                return;
            }

            if (selectedQuestIndex < 0 || selectedQuestIndex >= editorSession.QuestCount)
            {
                selectedQuestIndex = 0;
            }
        }

        private void OnUndoRedo()
        {
            editorSession?.Update();
            EnsureSelection();
            Repaint();
        }

        private static string CreateQuestListLabel(
            SerializedProperty questProperty,
            int questIndex,
            bool isSelected)
        {
            string prefix = isSelected ? "▶ " : string.Empty;

            if (questProperty == null)
            {
                return $"{prefix}{questIndex + 1}. (Invalid Quest)";
            }

            SerializedProperty idProperty = questProperty.FindPropertyRelative("id");
            SerializedProperty titleProperty = questProperty.FindPropertyRelative("title");

            string id = string.IsNullOrWhiteSpace(idProperty.stringValue)
                ? "(No ID)"
                : idProperty.stringValue;

            string title = string.IsNullOrWhiteSpace(titleProperty.stringValue)
                ? "(No Title)"
                : titleProperty.stringValue;

            return $"{prefix}{id}\n{title}";
        }
    }
}