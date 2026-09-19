namespace CDG.Quest
{
    internal static class QuestValidationCodes
    {
        internal const string QuestNull = "QUEST_NULL";
        internal const string QuestIdRequired = "QUEST_ID_REQUIRED";
        internal const string QuestIdDuplicate = "QUEST_ID_DUPLICATE";
        internal const string QuestTitleMissing = "QUEST_TITLE_MISSING";
        internal const string QuestDescriptionMissing = "QUEST_DESCRIPTION_MISSING";
        internal const string ObjectiveMissing = "OBJECTIVE_MISSING";
        internal const string ObjectiveNull = "OBJECTIVE_NULL";
        internal const string ObjectiveIdRequired = "OBJECTIVE_ID_REQUIRED";
        internal const string ObjectiveIdDuplicate = "OBJECTIVE_ID_DUPLICATE";
        internal const string ObjectiveTitleMissing = "OBJECTIVE_TITLE_MISSING";
        internal const string ObjectiveTargetProgressInvalid = "OBJECTIVE_TARGET_PROGRESS_INVALID";
        internal const string PrerequisiteIdRequired = "PREREQUISITE_ID_REQUIRED";
        internal const string PrerequisiteDuplicate = "PREREQUISITE_DUPLICATE";
        internal const string PrerequisiteSelfReference = "PREREQUISITE_SELF_REFERENCE";
        internal const string PrerequisiteNotFound = "PREREQUISITE_NOT_FOUND";
        internal const string PrerequisiteCycle = "PREREQUISITE_CYCLE";
        internal const string RewardNull = "REWARD_NULL";
        internal const string RewardTypeRequired = "REWARD_TYPE_REQUIRED";
        internal const string RewardKeyRequired = "REWARD_KEY_REQUIRED";
        internal const string RewardAmountInvalid = "REWARD_AMOUNT_INVALID";
    }
}