namespace CDG.Quest
{
    /// <summary>
    /// Quest Framework에서 사용하는 안정적인 오류 코드 모음입니다.
    /// 외부 코드에서는 오류 메시지보다 오류 코드를 기준으로 실패 원인을 구분할 수 있습니다.
    /// </summary>
    public static class QuestErrorCodes
    {
        /// <summary>
        /// QuestManager 생성에 필요한 QuestCatalog가 지정되지 않은 경우 사용합니다.
        /// </summary>
        public const string CatalogRequired = "QUEST_CATALOG_REQUIRED";

        /// <summary>
        /// QuestCatalog에 실행을 막는 검증 오류가 포함된 경우 사용합니다.
        /// </summary>
        public const string ValidationFailed = "QUEST_VALIDATION_FAILED";

        /// <summary>
        /// Quest ID가 null, 빈 문자열 또는 공백인 경우 사용합니다.
        /// </summary>
        public const string InvalidQuestId = "QUEST_INVALID_ID";

        /// <summary>
        /// 요청한 Quest ID가 Catalog에 존재하지 않는 경우 사용합니다.
        /// </summary>
        public const string QuestNotFound = "QUEST_NOT_FOUND";

        /// <summary>
        /// Objective ID가 null, 빈 문자열 또는 공백인 경우 사용합니다.
        /// </summary>
        public const string InvalidObjectiveId = "QUEST_INVALID_OBJECTIVE_ID";

        /// <summary>
        /// 요청한 Objective가 해당 Quest에 존재하지 않는 경우 사용합니다.
        /// </summary>
        public const string ObjectiveNotFound = "QUEST_OBJECTIVE_NOT_FOUND";

        /// <summary>
        /// Inactive 상태가 아닌 Quest에 Start를 요청한 경우 사용합니다.
        /// </summary>
        public const string QuestNotInactive = "QUEST_NOT_INACTIVE";

        /// <summary>
        /// 아직 완료되지 않은 선행 Quest가 있어 Quest를 시작할 수 없는 경우 사용합니다.
        /// </summary>
        public const string PrerequisiteNotCompleted = "QUEST_PREREQUISITE_NOT_COMPLETED";

        /// <summary>
        /// Active 상태가 아닌 Quest에 진행도 변경을 요청한 경우 사용합니다.
        /// </summary>
        public const string QuestNotActive = "QUEST_NOT_ACTIVE";

        /// <summary>
        /// Objective 진행도 또는 증가량이 허용 범위를 벗어난 경우 사용합니다.
        /// </summary>
        public const string InvalidProgress = "QUEST_INVALID_PROGRESS";

        /// <summary>
        /// 이미 완료된 Objective의 진행도를 다시 변경하려 한 경우 사용합니다.
        /// </summary>
        public const string ObjectiveAlreadyCompleted = "QUEST_OBJECTIVE_ALREADY_COMPLETED";

        /// <summary>
        /// Completed 상태가 아닌 Quest에 Reset을 요청한 경우 사용합니다.
        /// </summary>
        public const string QuestNotCompleted = "QUEST_NOT_COMPLETED";

        /// <summary>
        /// 반복 가능하지 않은 Quest에 Reset을 요청한 경우 사용합니다.
        /// </summary>
        public const string QuestNotRepeatable = "QUEST_NOT_REPEATABLE";
    }
}