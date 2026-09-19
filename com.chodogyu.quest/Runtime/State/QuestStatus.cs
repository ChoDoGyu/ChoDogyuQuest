namespace CDG.Quest
{
    /// <summary>
    /// Runtime에서 관리되는 Quest의 현재 상태를 나타냅니다.
    /// 시작 가능 여부는 별도의 조건으로 계산하며 상태 값으로 저장하지 않습니다.
    /// </summary>
    public enum QuestStatus
    {
        /// <summary>
        /// 아직 시작되지 않았거나 반복 Quest가 Reset된 상태입니다.
        /// </summary>
        Inactive,

        /// <summary>
        /// 현재 진행 중인 상태입니다.
        /// </summary>
        Active,

        /// <summary>
        /// 모든 필수 Objective가 완료된 상태입니다.
        /// </summary>
        Completed
    }
}