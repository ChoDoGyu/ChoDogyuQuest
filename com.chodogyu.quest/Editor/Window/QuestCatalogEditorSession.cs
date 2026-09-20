using System;
using UnityEditor;

namespace CDG.Quest.Editor
{
    /// <summary>
    /// QuestCatalog의 SerializedObject와 Quest 배열 편집 상태를 관리합니다.
    /// EditorWindow의 화면 표현과 실제 직렬화 편집 책임을 분리하기 위한 내부 Session입니다.
    /// </summary>
    internal sealed class QuestCatalogEditorSession
    {
        private const string QuestsPropertyName = "quests";
        private const string IdPropertyName = "id";
        private const string TitlePropertyName = "title";
        private const string DescriptionPropertyName = "description";
        private const string ObjectivesPropertyName = "objectives";
        private const string PrerequisitesPropertyName = "prerequisiteQuestIds";
        private const string RewardsPropertyName = "rewards";
        private const string RepeatablePropertyName = "isRepeatable";

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
        /// </summary>
        internal bool ApplyModifiedProperties()
        {
            return serializedObject != null && serializedObject.ApplyModifiedProperties();
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

            serializedObject.Update();

            int newIndex = questsProperty.arraySize;
            questsProperty.arraySize++;

            SerializedProperty questProperty = questsProperty.GetArrayElementAtIndex(newIndex);
            InitializeQuestProperty(questProperty, CreateUniqueQuestId());

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(Catalog);

            return newIndex;
        }

        /// <summary>
        /// 지정한 Quest를 삭제하고 삭제 후 선택하기 적절한 Quest 인덱스를 반환합니다.
        /// 남은 Quest가 없으면 -1을 반환합니다.
        /// </summary>
        internal int DeleteQuest(int questIndex)
        {
            if (serializedObject == null || questsProperty == null || questIndex < 0 || questIndex >= questsProperty.arraySize)
            {
                return -1;
            }

            Undo.SetCurrentGroupName("Delete Quest");

            serializedObject.Update();
            questsProperty.DeleteArrayElementAtIndex(questIndex);
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(Catalog);

            if (questsProperty.arraySize == 0)
            {
                return -1;
            }

            return Math.Min(questIndex, questsProperty.arraySize - 1);
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

            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(Catalog);
        }

        private void InitializeQuestProperty(SerializedProperty questProperty, string questId)
        {
            SerializedProperty idProperty = questProperty.FindPropertyRelative(IdPropertyName);
            SerializedProperty titleProperty = questProperty.FindPropertyRelative(TitlePropertyName);
            SerializedProperty descriptionProperty = questProperty.FindPropertyRelative(DescriptionPropertyName);
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

        private bool ContainsQuestId(string questId)
        {
            for (int i = 0; i < questsProperty.arraySize; i++)
            {
                SerializedProperty questProperty = questsProperty.GetArrayElementAtIndex(i);
                SerializedProperty idProperty = questProperty.FindPropertyRelative(IdPropertyName);

                if (string.Equals(idProperty.stringValue, questId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}