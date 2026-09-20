using System;
using System.Collections.Generic;
using UnityEditor;

namespace CDG.Quest.Editor
{
    /// <summary>
    /// QuestCatalog의 SerializedObject와 Quest Definition 편집 상태를 관리합니다.
    /// EditorWindow의 화면 표현과 실제 직렬화 편집 책임을 분리하기 위한 내부 Session입니다.
    /// </summary>
    internal sealed class QuestCatalogEditorSession
    {
        private const string QuestsPropertyName = "quests";

        private const string QuestIdPropertyName = "id";
        private const string QuestTitlePropertyName = "title";
        private const string QuestDescriptionPropertyName = "description";
        private const string ObjectivesPropertyName = "objectives";
        private const string PrerequisitesPropertyName = "prerequisiteQuestIds";
        private const string RewardsPropertyName = "rewards";
        private const string RepeatablePropertyName = "isRepeatable";

        private const string ObjectiveIdPropertyName = "id";
        private const string ObjectiveTitlePropertyName = "title";
        private const string ObjectiveDescriptionPropertyName = "description";
        private const string ObjectiveTargetProgressPropertyName = "targetProgress";

        private const string RewardTypePropertyName = "type";
        private const string RewardKeyPropertyName = "key";
        private const string RewardAmountPropertyName = "amount";

        private SerializedObject serializedObject;
        private SerializedProperty questsProperty;

        /// <summary>
        /// 현재 Session이 편집 중인 QuestCatalog입니다.
        /// </summary>
        internal QuestCatalog Catalog => serializedObject != null ? serializedObject.targetObject as QuestCatalog : null;

        /// <summary>
        /// 현재 직렬화된 Quest 항목 수입니다.
        /// </summary>
        internal int QuestCount => questsProperty?.arraySize ?? 0;

        /// <summary>
        /// 현재 Catalog Asset에 저장되지 않은 변경 사항이 있는지 나타냅니다.
        /// </summary>
        internal bool IsDirty => Catalog != null && EditorUtility.IsDirty(Catalog);

        internal QuestCatalogEditorSession(QuestCatalog catalog)
        {
            SetCatalog(catalog);
        }

        /// <summary>
        /// 편집 대상을 변경하고 SerializedObject 상태를 새 Catalog 기준으로 다시 구성합니다.
        /// </summary>
        internal void SetCatalog(QuestCatalog catalog)
        {
            if (catalog == null)
            {
                serializedObject = null;
                questsProperty = null;
                return;
            }

            serializedObject = new SerializedObject(catalog);
            questsProperty = serializedObject.FindProperty(QuestsPropertyName);

            if (questsProperty == null)
            {
                throw new InvalidOperationException($"QuestCatalog에서 '{QuestsPropertyName}' 직렬화 필드를 찾을 수 없습니다.");
            }
        }

        /// <summary>
        /// Unity 직렬화 데이터의 최신 값을 Session에 반영합니다.
        /// </summary>
        internal void Update()
        {
            serializedObject?.Update();
        }

        /// <summary>
        /// 현재 SerializedProperty 변경 사항을 Catalog에 적용합니다.
        /// 변경이 발생한 경우 Catalog를 Dirty 상태로 유지합니다.
        /// </summary>
        internal bool ApplyModifiedProperties()
        {
            if (serializedObject == null)
            {
                return false;
            }

            bool changed = serializedObject.ApplyModifiedProperties();

            if (changed && Catalog != null)
            {
                EditorUtility.SetDirty(Catalog);
            }

            return changed;
        }

        /// <summary>
        /// 지정한 인덱스의 Quest SerializedProperty를 반환합니다.
        /// </summary>
        internal SerializedProperty GetQuestProperty(int questIndex)
        {
            if (questsProperty == null || questIndex < 0 || questIndex >= questsProperty.arraySize)
            {
                return null;
            }

            return questsProperty.GetArrayElementAtIndex(questIndex);
        }

        /// <summary>
        /// 지정한 Quest의 ID를 반환합니다.
        /// </summary>
        internal string GetQuestId(int questIndex)
        {
            SerializedProperty questProperty = GetQuestProperty(questIndex);

            if (questProperty == null)
            {
                return string.Empty;
            }

            SerializedProperty idProperty = questProperty.FindPropertyRelative(QuestIdPropertyName);
            return idProperty?.stringValue ?? string.Empty;
        }

        /// <summary>
        /// 지정한 Quest의 Title을 반환합니다.
        /// </summary>
        internal string GetQuestTitle(int questIndex)
        {
            SerializedProperty questProperty = GetQuestProperty(questIndex);

            if (questProperty == null)
            {
                return string.Empty;
            }

            SerializedProperty titleProperty = questProperty.FindPropertyRelative(QuestTitlePropertyName);
            return titleProperty?.stringValue ?? string.Empty;
        }

        /// <summary>
        /// 지정한 ID를 가진 Quest가 현재 Catalog에 존재하는지 확인합니다.
        /// </summary>
        internal bool ContainsQuestId(string questId)
        {
            return FindQuestIndexById(questId) >= 0;
        }

        /// <summary>
        /// 지정한 ID와 일치하는 첫 번째 Quest의 인덱스를 반환합니다.
        /// 찾지 못하면 -1을 반환합니다.
        /// </summary>
        internal int FindQuestIndexById(string questId)
        {
            if (questsProperty == null || string.IsNullOrWhiteSpace(questId))
            {
                return -1;
            }

            for (int i = 0; i < questsProperty.arraySize; i++)
            {
                if (string.Equals(GetQuestId(i), questId, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 지정한 Quest가 직접 참조하는 선행 Quest ID 목록을 반환합니다.
        /// </summary>
        internal string[] GetPrerequisiteIds(int questIndex)
        {
            SerializedProperty prerequisitesProperty = GetChildArrayProperty(questIndex, PrerequisitesPropertyName);

            if (prerequisitesProperty == null)
            {
                return Array.Empty<string>();
            }

            string[] result = new string[prerequisitesProperty.arraySize];

            for (int i = 0; i < prerequisitesProperty.arraySize; i++)
            {
                result[i] = prerequisitesProperty.GetArrayElementAtIndex(i).stringValue ?? string.Empty;
            }

            return result;
        }

        /// <summary>
        /// 지정한 Quest를 선행 Quest로 참조하는 다른 Quest들의 인덱스를 반환합니다.
        /// </summary>
        internal int[] GetDependentQuestIndices(int questIndex)
        {
            string questId = GetQuestId(questIndex);

            if (string.IsNullOrWhiteSpace(questId))
            {
                return Array.Empty<int>();
            }

            List<int> dependentIndices = new List<int>();

            for (int i = 0; i < QuestCount; i++)
            {
                if (i == questIndex)
                {
                    continue;
                }

                SerializedProperty prerequisitesProperty = GetChildArrayProperty(i, PrerequisitesPropertyName);

                if (prerequisitesProperty == null)
                {
                    continue;
                }

                for (int prerequisiteIndex = 0; prerequisiteIndex < prerequisitesProperty.arraySize; prerequisiteIndex++)
                {
                    string prerequisiteId = prerequisitesProperty.GetArrayElementAtIndex(prerequisiteIndex).stringValue;

                    if (string.Equals(prerequisiteId, questId, StringComparison.Ordinal))
                    {
                        dependentIndices.Add(i);
                        break;
                    }
                }
            }

            return dependentIndices.ToArray();
        }

        /// <summary>
        /// 새로운 빈 Quest Definition을 추가하고 추가된 인덱스를 반환합니다.
        /// ID는 현재 Catalog에서 사용하지 않는 기본 ID를 자동 생성합니다.
        /// </summary>
        internal int AddQuest()
        {
            if (serializedObject == null || questsProperty == null)
            {
                return -1;
            }

            Undo.SetCurrentGroupName("Add Quest");

            int newIndex = questsProperty.arraySize;
            questsProperty.arraySize++;

            SerializedProperty questProperty = questsProperty.GetArrayElementAtIndex(newIndex);
            InitializeQuestProperty(questProperty, CreateUniqueQuestId());

            return newIndex;
        }

        /// <summary>
        /// 지정한 Quest를 삭제하고 삭제 후 선택하기 적절한 Quest 인덱스를 반환합니다.
        /// 남은 Quest가 없으면 -1을 반환합니다.
        /// </summary>
        internal int DeleteQuest(int questIndex)
        {
            if (questsProperty == null || questIndex < 0 || questIndex >= questsProperty.arraySize)
            {
                return -1;
            }

            Undo.SetCurrentGroupName("Delete Quest");
            questsProperty.DeleteArrayElementAtIndex(questIndex);

            if (questsProperty.arraySize == 0)
            {
                return -1;
            }

            return Math.Min(questIndex, questsProperty.arraySize - 1);
        }

        /// <summary>
        /// 지정한 Quest에 새로운 Objective를 추가하고 추가된 인덱스를 반환합니다.
        /// Objective ID는 해당 Quest 안에서 사용하지 않는 기본 ID를 자동 생성합니다.
        /// </summary>
        internal int AddObjective(int questIndex)
        {
            SerializedProperty objectivesProperty = GetChildArrayProperty(questIndex, ObjectivesPropertyName);

            if (objectivesProperty == null)
            {
                return -1;
            }

            Undo.SetCurrentGroupName("Add Objective");

            int newIndex = objectivesProperty.arraySize;
            objectivesProperty.arraySize++;

            SerializedProperty objectiveProperty = objectivesProperty.GetArrayElementAtIndex(newIndex);
            InitializeObjectiveProperty(objectiveProperty, CreateUniqueObjectiveId(objectivesProperty));

            return newIndex;
        }

        /// <summary>
        /// 지정한 Quest에서 Objective를 삭제합니다.
        /// </summary>
        internal void DeleteObjective(int questIndex, int objectiveIndex)
        {
            SerializedProperty objectivesProperty = GetChildArrayProperty(questIndex, ObjectivesPropertyName);

            if (objectivesProperty == null || objectiveIndex < 0 || objectiveIndex >= objectivesProperty.arraySize)
            {
                return;
            }

            Undo.SetCurrentGroupName("Delete Objective");
            objectivesProperty.DeleteArrayElementAtIndex(objectiveIndex);
        }

        /// <summary>
        /// 지정한 Quest에 새로운 Reward를 추가하고 추가된 인덱스를 반환합니다.
        /// </summary>
        internal int AddReward(int questIndex)
        {
            SerializedProperty rewardsProperty = GetChildArrayProperty(questIndex, RewardsPropertyName);

            if (rewardsProperty == null)
            {
                return -1;
            }

            Undo.SetCurrentGroupName("Add Reward");

            int newIndex = rewardsProperty.arraySize;
            rewardsProperty.arraySize++;

            SerializedProperty rewardProperty = rewardsProperty.GetArrayElementAtIndex(newIndex);
            InitializeRewardProperty(rewardProperty);

            return newIndex;
        }

        /// <summary>
        /// 지정한 Quest에서 Reward를 삭제합니다.
        /// </summary>
        internal void DeleteReward(int questIndex, int rewardIndex)
        {
            SerializedProperty rewardsProperty = GetChildArrayProperty(questIndex, RewardsPropertyName);

            if (rewardsProperty == null || rewardIndex < 0 || rewardIndex >= rewardsProperty.arraySize)
            {
                return;
            }

            Undo.SetCurrentGroupName("Delete Reward");
            rewardsProperty.DeleteArrayElementAtIndex(rewardIndex);
        }

        /// <summary>
        /// 현재 Quest에 추가 가능한 첫 번째 선행 Quest를 추가합니다.
        /// 자신, 빈 ID, 이미 등록된 ID는 후보에서 제외합니다.
        /// </summary>
        internal bool AddPrerequisite(int questIndex)
        {
            SerializedProperty prerequisitesProperty = GetChildArrayProperty(questIndex, PrerequisitesPropertyName);

            if (prerequisitesProperty == null)
            {
                return false;
            }

            string candidate = FindFirstAvailablePrerequisiteId(questIndex);

            if (string.IsNullOrEmpty(candidate))
            {
                return false;
            }

            Undo.SetCurrentGroupName("Add Prerequisite");

            int newIndex = prerequisitesProperty.arraySize;
            prerequisitesProperty.arraySize++;
            prerequisitesProperty.GetArrayElementAtIndex(newIndex).stringValue = candidate;

            return true;
        }

        /// <summary>
        /// 지정한 Quest에서 선행 Quest 관계를 삭제합니다.
        /// </summary>
        internal void DeletePrerequisite(int questIndex, int prerequisiteIndex)
        {
            SerializedProperty prerequisitesProperty = GetChildArrayProperty(questIndex, PrerequisitesPropertyName);

            if (prerequisitesProperty == null || prerequisiteIndex < 0 || prerequisiteIndex >= prerequisitesProperty.arraySize)
            {
                return;
            }

            Undo.SetCurrentGroupName("Delete Prerequisite");
            prerequisitesProperty.DeleteArrayElementAtIndex(prerequisiteIndex);
        }

        /// <summary>
        /// 지정한 Quest에 추가할 수 있는 선행 Quest가 하나 이상 존재하는지 확인합니다.
        /// </summary>
        internal bool CanAddPrerequisite(int questIndex)
        {
            return !string.IsNullOrEmpty(FindFirstAvailablePrerequisiteId(questIndex));
        }

        /// <summary>
        /// 하나의 Prerequisite 항목에서 선택할 수 있는 Quest ID 목록을 반환합니다.
        /// 현재 값은 유효하지 않더라도 목록에서 유지하여 기존 데이터를 잃지 않습니다.
        /// </summary>
        internal string[] GetPrerequisiteOptions(int questIndex, int prerequisiteIndex)
        {
            SerializedProperty prerequisitesProperty = GetChildArrayProperty(questIndex, PrerequisitesPropertyName);

            if (prerequisitesProperty == null || prerequisiteIndex < 0 || prerequisiteIndex >= prerequisitesProperty.arraySize)
            {
                return Array.Empty<string>();
            }

            string currentValue = prerequisitesProperty.GetArrayElementAtIndex(prerequisiteIndex).stringValue ?? string.Empty;
            string currentQuestId = GetQuestId(questIndex);

            List<string> options = new List<string>
            {
                currentValue
            };

            for (int i = 0; i < QuestCount; i++)
            {
                if (i == questIndex)
                {
                    continue;
                }

                string candidate = GetQuestId(i);

                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (string.Equals(candidate, currentQuestId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (IsPrerequisiteUsedByOtherEntry(prerequisitesProperty, prerequisiteIndex, candidate))
                {
                    continue;
                }

                if (!options.Contains(candidate))
                {
                    options.Add(candidate);
                }
            }

            return options.ToArray();
        }

        /// <summary>
        /// 현재 Catalog의 변경 내용을 디스크에 저장합니다.
        /// </summary>
        internal void Save()
        {
            if (Catalog == null)
            {
                return;
            }

            ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(Catalog);
        }

        private SerializedProperty GetChildArrayProperty(int questIndex, string propertyName)
        {
            SerializedProperty questProperty = GetQuestProperty(questIndex);
            return questProperty?.FindPropertyRelative(propertyName);
        }

        private void InitializeQuestProperty(SerializedProperty questProperty, string questId)
        {
            SerializedProperty idProperty = questProperty.FindPropertyRelative(QuestIdPropertyName);
            SerializedProperty titleProperty = questProperty.FindPropertyRelative(QuestTitlePropertyName);
            SerializedProperty descriptionProperty = questProperty.FindPropertyRelative(QuestDescriptionPropertyName);
            SerializedProperty objectivesProperty = questProperty.FindPropertyRelative(ObjectivesPropertyName);
            SerializedProperty prerequisitesProperty = questProperty.FindPropertyRelative(PrerequisitesPropertyName);
            SerializedProperty rewardsProperty = questProperty.FindPropertyRelative(RewardsPropertyName);
            SerializedProperty repeatableProperty = questProperty.FindPropertyRelative(RepeatablePropertyName);

            idProperty.stringValue = questId;
            titleProperty.stringValue = "New Quest";
            descriptionProperty.stringValue = string.Empty;
            objectivesProperty.arraySize = 0;
            prerequisitesProperty.arraySize = 0;
            rewardsProperty.arraySize = 0;
            repeatableProperty.boolValue = false;
        }

        private void InitializeObjectiveProperty(SerializedProperty objectiveProperty, string objectiveId)
        {
            SerializedProperty idProperty = objectiveProperty.FindPropertyRelative(ObjectiveIdPropertyName);
            SerializedProperty titleProperty = objectiveProperty.FindPropertyRelative(ObjectiveTitlePropertyName);
            SerializedProperty descriptionProperty = objectiveProperty.FindPropertyRelative(ObjectiveDescriptionPropertyName);
            SerializedProperty targetProgressProperty = objectiveProperty.FindPropertyRelative(ObjectiveTargetProgressPropertyName);

            idProperty.stringValue = objectiveId;
            titleProperty.stringValue = "New Objective";
            descriptionProperty.stringValue = string.Empty;
            targetProgressProperty.intValue = 1;
        }

        private void InitializeRewardProperty(SerializedProperty rewardProperty)
        {
            SerializedProperty typeProperty = rewardProperty.FindPropertyRelative(RewardTypePropertyName);
            SerializedProperty keyProperty = rewardProperty.FindPropertyRelative(RewardKeyPropertyName);
            SerializedProperty amountProperty = rewardProperty.FindPropertyRelative(RewardAmountPropertyName);

            typeProperty.stringValue = string.Empty;
            keyProperty.stringValue = string.Empty;
            amountProperty.intValue = 1;
        }

        private string CreateUniqueQuestId()
        {
            int number = 1;

            while (true)
            {
                string candidate = $"quest_{number:000}";

                if (!ContainsQuestId(candidate))
                {
                    return candidate;
                }

                number++;
            }
        }

        private static string CreateUniqueObjectiveId(SerializedProperty objectivesProperty)
        {
            int number = 1;

            while (true)
            {
                string candidate = $"objective_{number:000}";
                bool exists = false;

                for (int i = 0; i < objectivesProperty.arraySize; i++)
                {
                    SerializedProperty objectiveProperty = objectivesProperty.GetArrayElementAtIndex(i);
                    SerializedProperty idProperty = objectiveProperty.FindPropertyRelative(ObjectiveIdPropertyName);

                    if (string.Equals(idProperty.stringValue, candidate, StringComparison.Ordinal))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    return candidate;
                }

                number++;
            }
        }

        private string FindFirstAvailablePrerequisiteId(int questIndex)
        {
            SerializedProperty prerequisitesProperty = GetChildArrayProperty(questIndex, PrerequisitesPropertyName);

            if (prerequisitesProperty == null)
            {
                return string.Empty;
            }

            string currentQuestId = GetQuestId(questIndex);

            for (int i = 0; i < QuestCount; i++)
            {
                if (i == questIndex)
                {
                    continue;
                }

                string candidate = GetQuestId(i);

                if (string.IsNullOrWhiteSpace(candidate))
                {
                    continue;
                }

                if (string.Equals(candidate, currentQuestId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!ContainsPrerequisite(prerequisitesProperty, candidate))
                {
                    return candidate;
                }
            }

            return string.Empty;
        }

        private static bool ContainsPrerequisite(SerializedProperty prerequisitesProperty, string questId)
        {
            for (int i = 0; i < prerequisitesProperty.arraySize; i++)
            {
                if (string.Equals(prerequisitesProperty.GetArrayElementAtIndex(i).stringValue, questId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPrerequisiteUsedByOtherEntry(SerializedProperty prerequisitesProperty, int ignoredIndex, string questId)
        {
            for (int i = 0; i < prerequisitesProperty.arraySize; i++)
            {
                if (i == ignoredIndex)
                {
                    continue;
                }

                if (string.Equals(prerequisitesProperty.GetArrayElementAtIndex(i).stringValue, questId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}