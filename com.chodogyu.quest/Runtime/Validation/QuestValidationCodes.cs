namespace CDG.Quest
{
    /// <summary>
    /// Quest Definition 검증에서 사용하는 안정적인 문제 코드 모음입니다.
    /// 외부 코드에서는 검증 메시지보다 이 코드를 기준으로 문제 종류를 구분할 수 있습니다.
    /// </summary>
    public static class QuestValidationCodes
    {
        /// <summary>
        /// Quest 항목이 null인 경우 사용합니다.
        /// </summary>
        public const string QuestNull = "QUEST_NULL";

        /// <summary>
        /// Quest ID가 비어 있는 경우 사용합니다.
        /// </summary>
        public const string QuestIdRequired = "QUEST_ID_REQUIRED";

        /// <summary>
        /// 동일한 Quest ID가 중복된 경우 사용합니다.
        /// </summary>
        public const string QuestIdDuplicate = "QUEST_ID_DUPLICATE";

        /// <summary>
        /// Quest 제목이 비어 있는 경우 사용합니다.
        /// </summary>
        public const string QuestTitleMissing = "QUEST_TITLE_MISSING";

        /// <summary>
        /// Quest 설명이 비어 있는 경우 사용합니다.
        /// </summary>
        public const string QuestDescriptionMissing = "QUEST_DESCRIPTION_MISSING";

        /// <summary>
        /// Quest에 Objective가 하나도 없는 경우 사용합니다.
        /// </summary>
        public const string ObjectiveMissing = "OBJECTIVE_MISSING";

        /// <summary>
        /// Objective 항목이 null인 경우 사용합니다.
        /// </summary>
        public const string ObjectiveNull = "OBJECTIVE_NULL";

        /// <summary>
        /// Objective ID가 비어 있는 경우 사용합니다.
        /// </summary>
        public const string ObjectiveIdRequired = "OBJECTIVE_ID_REQUIRED";

        /// <summary>
        /// 동일한 Quest 안에서 Objective ID가 중복된 경우 사용합니다.
        /// </summary>
        public const string ObjectiveIdDuplicate = "OBJECTIVE_ID_DUPLICATE";

        /// <summary>
        /// Objective 제목이 비어 있는 경우 사용합니다.
        /// </summary>
        public const string ObjectiveTitleMissing = "OBJECTIVE_TITLE_MISSING";

        /// <summary>
        /// Objective의 Target Progress가 유효하지 않은 경우 사용합니다.
        /// </summary>
        public const string ObjectiveTargetProgressInvalid = "OBJECTIVE_TARGET_PROGRESS_INVALID";

        /// <summary>
        /// 선행 Quest ID가 비어 있는 경우 사용합니다.
        /// </summary>
        public const string PrerequisiteIdRequired = "PREREQUISITE_ID_REQUIRED";

        /// <summary>
        /// 동일한 선행 Quest ID가 중복된 경우 사용합니다.
        /// </summary>
        public const string PrerequisiteDuplicate = "PREREQUISITE_DUPLICATE";

        /// <summary>
        /// Quest가 자기 자신을 선행 Quest로 참조하는 경우 사용합니다.
        /// </summary>
        public const string PrerequisiteSelfReference = "PREREQUISITE_SELF_REFERENCE";

        /// <summary>
        /// 참조한 선행 Quest가 Catalog에 존재하지 않는 경우 사용합니다.
        /// </summary>
        public const string PrerequisiteNotFound = "PREREQUISITE_NOT_FOUND";

        /// <summary>
        /// 선행 Quest 관계에 순환 참조가 존재하는 경우 사용합니다.
        /// </summary>
        public const string PrerequisiteCycle = "PREREQUISITE_CYCLE";

        /// <summary>
        /// Reward 항목이 null인 경우 사용합니다.
        /// </summary>
        public const string RewardNull = "REWARD_NULL";

        /// <summary>
        /// Reward Type이 비어 있는 경우 사용합니다.
        /// </summary>
        public const string RewardTypeRequired = "REWARD_TYPE_REQUIRED";

        /// <summary>
        /// Reward Key가 비어 있는 경우 사용합니다.
        /// </summary>
        public const string RewardKeyRequired = "REWARD_KEY_REQUIRED";

        /// <summary>
        /// Reward Amount가 유효하지 않은 경우 사용합니다.
        /// </summary>
        public const string RewardAmountInvalid = "REWARD_AMOUNT_INVALID";
    }
}