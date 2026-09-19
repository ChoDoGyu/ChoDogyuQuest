namespace CDG.Quest
{
    /// <summary>
    /// Quest 정의 검증에서 발견된 문제의 심각도를 나타냅니다.
    /// </summary>
    public enum QuestValidationSeverity
    {
        /// <summary>
        /// Quest 실행을 막지는 않지만 확인이 필요한 문제입니다.
        /// </summary>
        Warning,

        /// <summary>
        /// 정상적인 Quest 실행을 보장할 수 없어 수정이 필요한 문제입니다.
        /// </summary>
        Error
    }
}