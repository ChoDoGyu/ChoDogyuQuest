using System;
using UnityEditor;
using UnityEngine;

namespace CDG.Quest.Editor
{
    /// <summary>
    /// 선택한 Quest의 선행 관계와 해당 Quest를 참조하는 Dependent 관계를 읽기 전용으로 표시합니다.
    /// 관계 항목을 통해 연결된 Quest로 바로 이동할 수 있습니다.
    /// </summary>
    internal static class QuestRelationshipEditorGUI
    {
        internal static void Draw(QuestCatalogEditorSession session, int questIndex, Action<int> navigateToQuest)
        {
            EditorGUILayout.LabelField("Relationships", EditorStyles.boldLabel);

            DrawPrerequisites(session, questIndex, navigateToQuest);

            EditorGUILayout.Space(6f);

            DrawDependents(session, questIndex, navigateToQuest);
        }

        private static void DrawPrerequisites(QuestCatalogEditorSession session, int questIndex, Action<int> navigateToQuest)
        {
            string[] prerequisiteIds = session.GetPrerequisiteIds(questIndex);

            EditorGUILayout.LabelField($"Prerequisites ({prerequisiteIds.Length})", EditorStyles.miniBoldLabel);

            if (prerequisiteIds.Length == 0)
            {
                EditorGUILayout.HelpBox("이 Quest가 요구하는 선행 Quest가 없습니다.", MessageType.None);
                return;
            }

            for (int i = 0; i < prerequisiteIds.Length; i++)
            {
                string prerequisiteId = prerequisiteIds[i] ?? string.Empty;
                int targetIndex = session.FindQuestIndexById(prerequisiteId);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                if (string.IsNullOrWhiteSpace(prerequisiteId))
                {
                    EditorGUILayout.LabelField("(Empty)");
                }
                else if (targetIndex < 0)
                {
                    EditorGUILayout.LabelField($"{prerequisiteId} (Missing)");
                }
                else
                {
                    string title = session.GetQuestTitle(targetIndex);

                    EditorGUILayout.LabelField(
                        string.IsNullOrWhiteSpace(title)
                            ? prerequisiteId
                            : $"{prerequisiteId} — {title}");
                }

                EditorGUI.BeginDisabledGroup(targetIndex < 0);

                if (GUILayout.Button("Go", GUILayout.Width(45f)))
                {
                    navigateToQuest?.Invoke(targetIndex);
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawDependents(QuestCatalogEditorSession session, int questIndex, Action<int> navigateToQuest)
        {
            string questId = session.GetQuestId(questIndex);

            EditorGUILayout.LabelField("Dependents", EditorStyles.miniBoldLabel);

            if (string.IsNullOrWhiteSpace(questId))
            {
                EditorGUILayout.HelpBox(
                    "현재 Quest ID가 비어 있어 Dependent 관계를 계산할 수 없습니다.",
                    MessageType.Warning);

                return;
            }

            int[] dependentIndices = session.GetDependentQuestIndices(questIndex);

            if (dependentIndices.Length == 0)
            {
                EditorGUILayout.HelpBox("현재 Quest를 선행 조건으로 참조하는 Quest가 없습니다.", MessageType.None);
                return;
            }

            for (int i = 0; i < dependentIndices.Length; i++)
            {
                int dependentIndex = dependentIndices[i];
                string dependentId = session.GetQuestId(dependentIndex);
                string dependentTitle = session.GetQuestTitle(dependentIndex);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.LabelField(
                    string.IsNullOrWhiteSpace(dependentTitle)
                        ? dependentId
                        : $"{dependentId} — {dependentTitle}");

                if (GUILayout.Button("Go", GUILayout.Width(45f)))
                {
                    navigateToQuest?.Invoke(dependentIndex);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }
}