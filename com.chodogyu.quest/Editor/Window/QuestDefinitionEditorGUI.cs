using System;
using UnityEditor;
using UnityEngine;

namespace CDG.Quest.Editor
{
    /// <summary>
    /// 하나의 Quest Definition과 Objective, Prerequisite, Reward 직렬화 데이터를 편집하는 GUI를 제공합니다.
    /// EditorWindow의 탐색 책임과 Definition 세부 편집 UI를 분리합니다.
    /// </summary>
    internal static class QuestDefinitionEditorGUI
    {
        internal static void Draw(QuestCatalogEditorSession session, int questIndex)
        {
            SerializedProperty questProperty = session.GetQuestProperty(questIndex);

            if (questProperty == null)
            {
                EditorGUILayout.HelpBox("선택한 Quest를 찾을 수 없습니다.", MessageType.Error);
                return;
            }

            DrawBasicSection(questProperty);

            EditorGUILayout.Space(12f);

            DrawObjectiveSection(session, questIndex, questProperty);

            EditorGUILayout.Space(12f);

            DrawPrerequisiteSection(session, questIndex, questProperty);

            EditorGUILayout.Space(12f);

            DrawRewardSection(session, questIndex, questProperty);
        }

        private static void DrawBasicSection(SerializedProperty questProperty)
        {
            EditorGUILayout.LabelField("Definition", EditorStyles.boldLabel);

            SerializedProperty idProperty = questProperty.FindPropertyRelative("id");
            SerializedProperty titleProperty = questProperty.FindPropertyRelative("title");
            SerializedProperty descriptionProperty = questProperty.FindPropertyRelative("description");
            SerializedProperty repeatableProperty = questProperty.FindPropertyRelative("isRepeatable");

            EditorGUILayout.PropertyField(idProperty, new GUIContent("ID"));
            EditorGUILayout.PropertyField(titleProperty, new GUIContent("Title"));

            EditorGUILayout.LabelField("Description");
            descriptionProperty.stringValue = EditorGUILayout.TextArea(descriptionProperty.stringValue, GUILayout.MinHeight(80f));

            EditorGUILayout.Space(4f);

            EditorGUILayout.PropertyField(repeatableProperty, new GUIContent("Repeatable"));
        }

        private static void DrawObjectiveSection(QuestCatalogEditorSession session, int questIndex, SerializedProperty questProperty)
        {
            SerializedProperty objectivesProperty = questProperty.FindPropertyRelative("objectives");

            EditorGUILayout.LabelField($"Objectives ({objectivesProperty.arraySize})", EditorStyles.boldLabel);

            int deleteIndex = -1;

            for (int i = 0; i < objectivesProperty.arraySize; i++)
            {
                SerializedProperty objectiveProperty = objectivesProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Objective {i + 1}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                {
                    deleteIndex = i;
                }

                EditorGUILayout.EndHorizontal();

                SerializedProperty idProperty = objectiveProperty.FindPropertyRelative("id");
                SerializedProperty titleProperty = objectiveProperty.FindPropertyRelative("title");
                SerializedProperty descriptionProperty = objectiveProperty.FindPropertyRelative("description");
                SerializedProperty targetProgressProperty = objectiveProperty.FindPropertyRelative("targetProgress");

                EditorGUILayout.PropertyField(idProperty, new GUIContent("ID"));
                EditorGUILayout.PropertyField(titleProperty, new GUIContent("Title"));

                EditorGUILayout.LabelField("Description");
                descriptionProperty.stringValue = EditorGUILayout.TextArea(descriptionProperty.stringValue, GUILayout.MinHeight(55f));

                EditorGUILayout.PropertyField(targetProgressProperty, new GUIContent("Target Progress"));

                EditorGUILayout.EndVertical();

                if (deleteIndex >= 0)
                {
                    break;
                }
            }

            if (deleteIndex >= 0)
            {
                session.DeleteObjective(questIndex, deleteIndex);
            }

            EditorGUILayout.Space(4f);

            if (GUILayout.Button("Add Objective", GUILayout.Height(26f)))
            {
                session.AddObjective(questIndex);
                GUI.FocusControl(null);
            }

            if (objectivesProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Quest를 완료하려면 하나 이상의 Objective가 필요합니다.", MessageType.Info);
            }
        }

        private static void DrawPrerequisiteSection(QuestCatalogEditorSession session, int questIndex, SerializedProperty questProperty)
        {
            SerializedProperty prerequisitesProperty = questProperty.FindPropertyRelative("prerequisiteQuestIds");

            EditorGUILayout.LabelField($"Prerequisites ({prerequisitesProperty.arraySize})", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "선행 Quest는 현재 Catalog의 Quest ID를 참조하며, 모든 선행 Quest의 완료 이력이 있어야 이 Quest를 시작할 수 있습니다.",
                EditorStyles.wordWrappedMiniLabel);

            int deleteIndex = -1;

            for (int i = 0; i < prerequisitesProperty.arraySize; i++)
            {
                SerializedProperty prerequisiteProperty = prerequisitesProperty.GetArrayElementAtIndex(i);
                string[] options = session.GetPrerequisiteOptions(questIndex, i);

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                EditorGUILayout.LabelField($"#{i + 1}", GUILayout.Width(28f));

                DrawPrerequisitePopup(session, prerequisiteProperty, options);

                if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                {
                    deleteIndex = i;
                }

                EditorGUILayout.EndHorizontal();

                if (deleteIndex >= 0)
                {
                    break;
                }
            }

            if (deleteIndex >= 0)
            {
                session.DeletePrerequisite(questIndex, deleteIndex);
            }

            EditorGUILayout.Space(4f);

            bool canAdd = session.CanAddPrerequisite(questIndex);
            EditorGUI.BeginDisabledGroup(!canAdd);

            if (GUILayout.Button("Add Prerequisite", GUILayout.Height(26f)))
            {
                session.AddPrerequisite(questIndex);
                GUI.FocusControl(null);
            }

            EditorGUI.EndDisabledGroup();

            if (!canAdd)
            {
                EditorGUILayout.HelpBox(
                    "현재 추가할 수 있는 다른 Quest가 없거나 모든 후보 Quest가 이미 선행 Quest로 등록되어 있습니다.",
                    MessageType.None);
            }
        }

        private static void DrawPrerequisitePopup(QuestCatalogEditorSession session, SerializedProperty prerequisiteProperty, string[] options)
        {
            if (options == null || options.Length == 0)
            {
                EditorGUILayout.PropertyField(prerequisiteProperty, GUIContent.none);
                return;
            }

            string currentValue = prerequisiteProperty.stringValue ?? string.Empty;
            string[] displayOptions = new string[options.Length];
            int selectedIndex = 0;

            for (int i = 0; i < options.Length; i++)
            {
                string option = options[i];

                if (string.Equals(option, currentValue, StringComparison.Ordinal))
                {
                    selectedIndex = i;
                }

                if (string.IsNullOrWhiteSpace(option))
                {
                    displayOptions[i] = "(Empty)";
                }
                else if (!session.ContainsQuestId(option))
                {
                    displayOptions[i] = $"{option} (Missing)";
                }
                else
                {
                    displayOptions[i] = option;
                }
            }

            int newIndex = EditorGUILayout.Popup(selectedIndex, displayOptions);

            if (newIndex >= 0 && newIndex < options.Length && newIndex != selectedIndex)
            {
                prerequisiteProperty.stringValue = options[newIndex];
            }
        }

        private static void DrawRewardSection(QuestCatalogEditorSession session, int questIndex, SerializedProperty questProperty)
        {
            SerializedProperty rewardsProperty = questProperty.FindPropertyRelative("rewards");

            EditorGUILayout.LabelField($"Rewards ({rewardsProperty.arraySize})", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Reward는 메타데이터만 정의합니다. 실제 지급 처리는 게임 코드가 담당합니다.",
                EditorStyles.wordWrappedMiniLabel);

            int deleteIndex = -1;

            for (int i = 0; i < rewardsProperty.arraySize; i++)
            {
                SerializedProperty rewardProperty = rewardsProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Reward {i + 1}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Remove", GUILayout.Width(70f)))
                {
                    deleteIndex = i;
                }

                EditorGUILayout.EndHorizontal();

                SerializedProperty typeProperty = rewardProperty.FindPropertyRelative("type");
                SerializedProperty keyProperty = rewardProperty.FindPropertyRelative("key");
                SerializedProperty amountProperty = rewardProperty.FindPropertyRelative("amount");

                EditorGUILayout.PropertyField(typeProperty, new GUIContent("Type"));
                EditorGUILayout.PropertyField(keyProperty, new GUIContent("Key"));
                EditorGUILayout.PropertyField(amountProperty, new GUIContent("Amount"));

                EditorGUILayout.EndVertical();

                if (deleteIndex >= 0)
                {
                    break;
                }
            }

            if (deleteIndex >= 0)
            {
                session.DeleteReward(questIndex, deleteIndex);
            }

            EditorGUILayout.Space(4f);

            if (GUILayout.Button("Add Reward", GUILayout.Height(26f)))
            {
                session.AddReward(questIndex);
                GUI.FocusControl(null);
            }

            if (rewardsProperty.arraySize == 0)
            {
                EditorGUILayout.HelpBox("보상이 없는 Quest도 사용할 수 있습니다.", MessageType.None);
            }
        }
    }
}