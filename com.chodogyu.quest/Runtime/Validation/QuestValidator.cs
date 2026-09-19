using System;
using System.Collections.Generic;

namespace CDG.Quest
{
    /// <summary>
    /// QuestCatalog와 그 안의 Quest, Objective, Reward 및 선행 관계를 검증합니다.
    /// 검증 과정에서는 원본 Definition 데이터를 수정하지 않으며 발견된 모든 문제를 함께 반환합니다.
    /// </summary>
    public static class QuestValidator
    {
        /// <summary>
        /// 지정한 QuestCatalog 전체를 검증하고 발견된 모든 문제를 반환합니다.
        /// ID 비교는 대소문자를 구분하며 Error와 Warning을 모두 수집합니다.
        /// </summary>
        /// <param name="catalog">검증할 QuestCatalog입니다.</param>
        /// <returns>발견된 검증 문제를 발견 순서대로 포함하는 읽기 전용 목록입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="catalog"/>이 null인 경우 발생합니다.</exception>
        public static IReadOnlyList<QuestValidationIssue> Validate(QuestCatalog catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            IReadOnlyList<QuestDefinition> quests = catalog.Quests;
            List<QuestValidationIssue> issues = new List<QuestValidationIssue>();
            Dictionary<string, int> questIdCounts = CountQuestIds(quests);

            for (int questIndex = 0; questIndex < quests.Count; questIndex++)
            {
                QuestDefinition quest = quests[questIndex];

                if (quest == null)
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.QuestNull, "QuestCatalog에 null Quest 항목이 포함되어 있습니다.", questIndex));
                    continue;
                }

                ValidateQuest(quest, questIndex, questIdCounts, issues);
            }

            ValidatePrerequisiteCycles(quests, questIdCounts, issues);
            return Array.AsReadOnly(issues.ToArray());
        }

        private static Dictionary<string, int> CountQuestIds(IReadOnlyList<QuestDefinition> quests)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < quests.Count; i++)
            {
                QuestDefinition quest = quests[i];

                if (quest == null || string.IsNullOrWhiteSpace(quest.Id))
                {
                    continue;
                }

                if (counts.TryGetValue(quest.Id, out int count))
                {
                    counts[quest.Id] = count + 1;
                }
                else
                {
                    counts.Add(quest.Id, 1);
                }
            }

            return counts;
        }

        private static void ValidateQuest(QuestDefinition quest, int questIndex, Dictionary<string, int> questIdCounts, List<QuestValidationIssue> issues)
        {
            if (string.IsNullOrWhiteSpace(quest.Id))
            {
                issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.QuestIdRequired, "Quest ID는 비어 있을 수 없습니다.", questIndex, quest.Id));
            }
            else if (questIdCounts.TryGetValue(quest.Id, out int count) && count > 1)
            {
                issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.QuestIdDuplicate, $"중복된 Quest ID입니다: '{quest.Id}'", questIndex, quest.Id));
            }

            if (string.IsNullOrWhiteSpace(quest.Title))
            {
                issues.Add(new QuestValidationIssue(QuestValidationSeverity.Warning, QuestValidationCodes.QuestTitleMissing, "Quest 제목이 비어 있습니다.", questIndex, quest.Id));
            }

            if (string.IsNullOrWhiteSpace(quest.Description))
            {
                issues.Add(new QuestValidationIssue(QuestValidationSeverity.Warning, QuestValidationCodes.QuestDescriptionMissing, "Quest 설명이 비어 있습니다.", questIndex, quest.Id));
            }

            ValidateObjectives(quest, questIndex, issues);
            ValidatePrerequisites(quest, questIndex, questIdCounts, issues);
            ValidateRewards(quest, questIndex, issues);
        }

        private static void ValidateObjectives(QuestDefinition quest, int questIndex, List<QuestValidationIssue> issues)
        {
            IReadOnlyList<ObjectiveDefinition> objectives = quest.Objectives;

            if (objectives.Count == 0)
            {
                issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.ObjectiveMissing, "Quest에는 최소 하나 이상의 Objective가 필요합니다.", questIndex, quest.Id));
                return;
            }

            HashSet<string> registeredObjectiveIds = new HashSet<string>(StringComparer.Ordinal);

            for (int objectiveIndex = 0; objectiveIndex < objectives.Count; objectiveIndex++)
            {
                ObjectiveDefinition objective = objectives[objectiveIndex];

                if (objective == null)
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.ObjectiveNull, "Quest에 null Objective 항목이 포함되어 있습니다.", questIndex, quest.Id, objectiveIndex));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(objective.Id))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.ObjectiveIdRequired, "Objective ID는 비어 있을 수 없습니다.", questIndex, quest.Id, objectiveIndex, objective.Id));
                }
                else if (!registeredObjectiveIds.Add(objective.Id))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.ObjectiveIdDuplicate, $"Quest 내부에 중복된 Objective ID가 있습니다: '{objective.Id}'", questIndex, quest.Id, objectiveIndex, objective.Id));
                }

                if (string.IsNullOrWhiteSpace(objective.Title))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Warning, QuestValidationCodes.ObjectiveTitleMissing, "Objective 제목이 비어 있습니다.", questIndex, quest.Id, objectiveIndex, objective.Id));
                }

                if (objective.TargetProgress <= 0)
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.ObjectiveTargetProgressInvalid, $"Objective TargetProgress는 1 이상이어야 합니다: {objective.TargetProgress}", questIndex, quest.Id, objectiveIndex, objective.Id));
                }
            }
        }

        private static void ValidatePrerequisites(QuestDefinition quest, int questIndex, Dictionary<string, int> questIdCounts, List<QuestValidationIssue> issues)
        {
            IReadOnlyList<string> prerequisiteQuestIds = quest.PrerequisiteQuestIds;
            HashSet<string> registeredPrerequisites = new HashSet<string>(StringComparer.Ordinal);

            for (int prerequisiteIndex = 0; prerequisiteIndex < prerequisiteQuestIds.Count; prerequisiteIndex++)
            {
                string prerequisiteId = prerequisiteQuestIds[prerequisiteIndex];

                if (string.IsNullOrWhiteSpace(prerequisiteId))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.PrerequisiteIdRequired, "선행 Quest ID는 비어 있을 수 없습니다.", questIndex, quest.Id, prerequisiteIndex: prerequisiteIndex));
                    continue;
                }

                if (!registeredPrerequisites.Add(prerequisiteId))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.PrerequisiteDuplicate, $"중복된 선행 Quest ID입니다: '{prerequisiteId}'", questIndex, quest.Id, prerequisiteIndex: prerequisiteIndex));
                    continue;
                }

                if (string.Equals(quest.Id, prerequisiteId, StringComparison.Ordinal))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.PrerequisiteSelfReference, $"Quest는 자기 자신을 선행 Quest로 지정할 수 없습니다: '{prerequisiteId}'", questIndex, quest.Id, prerequisiteIndex: prerequisiteIndex));
                    continue;
                }

                if (!questIdCounts.ContainsKey(prerequisiteId))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.PrerequisiteNotFound, $"존재하지 않는 선행 Quest ID입니다: '{prerequisiteId}'", questIndex, quest.Id, prerequisiteIndex: prerequisiteIndex));
                }
            }
        }

        private static void ValidateRewards(QuestDefinition quest, int questIndex, List<QuestValidationIssue> issues)
        {
            IReadOnlyList<RewardDefinition> rewards = quest.Rewards;

            for (int rewardIndex = 0; rewardIndex < rewards.Count; rewardIndex++)
            {
                RewardDefinition reward = rewards[rewardIndex];

                if (reward == null)
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.RewardNull, "Quest에 null Reward 항목이 포함되어 있습니다.", questIndex, quest.Id, rewardIndex: rewardIndex));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(reward.Type))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.RewardTypeRequired, "Reward Type은 비어 있을 수 없습니다.", questIndex, quest.Id, rewardIndex: rewardIndex));
                }

                if (string.IsNullOrWhiteSpace(reward.Key))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.RewardKeyRequired, "Reward Key는 비어 있을 수 없습니다.", questIndex, quest.Id, rewardIndex: rewardIndex));
                }

                if (reward.Amount <= 0)
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.RewardAmountInvalid, $"Reward Amount는 1 이상이어야 합니다: {reward.Amount}", questIndex, quest.Id, rewardIndex: rewardIndex));
                }
            }
        }

        private static void ValidatePrerequisiteCycles(IReadOnlyList<QuestDefinition> quests, Dictionary<string, int> questIdCounts, List<QuestValidationIssue> issues)
        {
            Dictionary<string, QuestDefinition> questById = new Dictionary<string, QuestDefinition>(StringComparer.Ordinal);
            Dictionary<string, int> visitStates = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < quests.Count; i++)
            {
                QuestDefinition quest = quests[i];

                if (quest == null || string.IsNullOrWhiteSpace(quest.Id))
                {
                    continue;
                }

                if (!questIdCounts.TryGetValue(quest.Id, out int count) || count != 1)
                {
                    continue;
                }

                questById.Add(quest.Id, quest);
                visitStates.Add(quest.Id, 0);
            }

            List<string> traversalStack = new List<string>();
            Dictionary<string, int> stackIndexes = new Dictionary<string, int>(StringComparer.Ordinal);
            HashSet<string> cycleQuestIds = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < quests.Count; i++)
            {
                QuestDefinition quest = quests[i];

                if (quest == null || string.IsNullOrWhiteSpace(quest.Id) || !questById.ContainsKey(quest.Id))
                {
                    continue;
                }

                if (visitStates[quest.Id] == 0)
                {
                    VisitPrerequisites(quest.Id, questById, visitStates, traversalStack, stackIndexes, cycleQuestIds);
                }
            }

            for (int questIndex = 0; questIndex < quests.Count; questIndex++)
            {
                QuestDefinition quest = quests[questIndex];

                if (quest != null && cycleQuestIds.Contains(quest.Id))
                {
                    issues.Add(new QuestValidationIssue(QuestValidationSeverity.Error, QuestValidationCodes.PrerequisiteCycle, $"Quest가 순환 선행 관계에 포함되어 있습니다: '{quest.Id}'", questIndex, quest.Id));
                }
            }
        }

        private static void VisitPrerequisites(string questId, Dictionary<string, QuestDefinition> questById, Dictionary<string, int> visitStates, List<string> traversalStack, Dictionary<string, int> stackIndexes, HashSet<string> cycleQuestIds)
        {
            visitStates[questId] = 1;
            stackIndexes[questId] = traversalStack.Count;
            traversalStack.Add(questId);

            QuestDefinition quest = questById[questId];

            for (int i = 0; i < quest.PrerequisiteQuestIds.Count; i++)
            {
                string prerequisiteId = quest.PrerequisiteQuestIds[i];

                if (string.IsNullOrWhiteSpace(prerequisiteId) || string.Equals(questId, prerequisiteId, StringComparison.Ordinal) || !questById.ContainsKey(prerequisiteId))
                {
                    continue;
                }

                int prerequisiteState = visitStates[prerequisiteId];

                if (prerequisiteState == 0)
                {
                    VisitPrerequisites(prerequisiteId, questById, visitStates, traversalStack, stackIndexes, cycleQuestIds);
                }
                else if (prerequisiteState == 1)
                {
                    int cycleStartIndex = stackIndexes[prerequisiteId];

                    for (int stackIndex = cycleStartIndex; stackIndex < traversalStack.Count; stackIndex++)
                    {
                        cycleQuestIds.Add(traversalStack[stackIndex]);
                    }
                }
            }

            traversalStack.RemoveAt(traversalStack.Count - 1);
            stackIndexes.Remove(questId);
            visitStates[questId] = 2;
        }
    }
}