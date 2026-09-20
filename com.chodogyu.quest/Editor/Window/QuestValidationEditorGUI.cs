using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CDG.Quest.Editor
{
    /// <summary>
    /// Runtime QuestValidator의 결과를 Quest Editor 안에서 표시합니다.
    /// Editor 전용 검증 규칙을 별도로 만들지 않고 Runtime과 동일한 검증 결과를 사용합니다.
    /// </summary>
    internal static class QuestValidationEditorGUI
    {
        internal static void Draw(QuestCatalog catalog, Action<int> navigateToQuest)
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            IReadOnlyList<QuestValidationIssue> issues = QuestValidator.Validate(catalog);

            int errorCount = 0;
            int warningCount = 0;

            for (int i = 0; i < issues.Count; i++)
            {
                if (issues[i].Severity == QuestValidationSeverity.Error)
                {
                    errorCount++;
                }
                else
                {
                    warningCount++;
                }
            }

            DrawSummary(issues.Count, errorCount, warningCount);

            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "QuestCatalog 검증 문제가 없습니다.",
                    MessageType.Info);

                return;
            }

            EditorGUILayout.Space(4f);

            for (int i = 0; i < issues.Count; i++)
            {
                DrawIssue(i, issues[i], navigateToQuest);
            }
        }

        private static void DrawSummary(int totalCount, int errorCount, int warningCount)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(
                $"Total: {totalCount}",
                GUILayout.Width(80f));

            EditorGUILayout.LabelField(
                $"Errors: {errorCount}",
                GUILayout.Width(90f));

            EditorGUILayout.LabelField(
                $"Warnings: {warningCount}",
                GUILayout.Width(100f));

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawIssue(int displayIndex, QuestValidationIssue issue, Action<int> navigateToQuest)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            string severityText = issue.Severity == QuestValidationSeverity.Error
                ? "Error"
                : "Warning";

            EditorGUILayout.LabelField(
                $"{displayIndex + 1}. {severityText} — {issue.Code}",
                EditorStyles.boldLabel);

            GUILayout.FlexibleSpace();

            bool canNavigate = issue.QuestIndex >= 0;

            EditorGUI.BeginDisabledGroup(!canNavigate);

            if (GUILayout.Button("Go", GUILayout.Width(45f)))
            {
                navigateToQuest?.Invoke(issue.QuestIndex);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(
                CreateLocationLabel(issue),
                EditorStyles.miniLabel);

            EditorGUILayout.HelpBox(
                issue.Message,
                issue.Severity == QuestValidationSeverity.Error
                    ? MessageType.Error
                    : MessageType.Warning);

            EditorGUILayout.EndVertical();
        }

        private static string CreateLocationLabel(QuestValidationIssue issue)
        {
            string questLabel = issue.QuestIndex >= 0
                ? $"Quest #{issue.QuestIndex + 1}"
                : "Catalog";

            if (!string.IsNullOrWhiteSpace(issue.QuestId))
            {
                questLabel += $" [{issue.QuestId}]";
            }

            if (issue.ObjectiveIndex >= 0)
            {
                string objectiveLabel = $" → Objective #{issue.ObjectiveIndex + 1}";

                if (!string.IsNullOrWhiteSpace(issue.ObjectiveId))
                {
                    objectiveLabel += $" [{issue.ObjectiveId}]";
                }

                return questLabel + objectiveLabel;
            }

            if (issue.PrerequisiteIndex >= 0)
            {
                return questLabel + $" → Prerequisite #{issue.PrerequisiteIndex + 1}";
            }

            if (issue.RewardIndex >= 0)
            {
                return questLabel + $" → Reward #{issue.RewardIndex + 1}";
            }

            return questLabel;
        }
    }
}