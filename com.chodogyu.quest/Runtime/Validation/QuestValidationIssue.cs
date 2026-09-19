using System;

namespace CDG.Quest
{
    /// <summary>
    /// Quest 정의 검증 과정에서 발견된 하나의 문제를 나타냅니다.
    /// 문제 코드와 메시지뿐 아니라 Editor에서 해당 위치를 찾을 수 있도록 Quest 및 하위 요소의 위치 정보를 제공합니다.
    /// </summary>
    public sealed class QuestValidationIssue
    {
        /// <summary>
        /// 발견된 문제의 심각도입니다.
        /// </summary>
        public QuestValidationSeverity Severity { get; }

        /// <summary>
        /// 문제 종류를 안정적으로 식별하기 위한 코드입니다.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// 문제의 원인을 설명하는 메시지입니다.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 문제가 발견된 Quest의 Catalog 기준 인덱스입니다.
        /// 특정 Quest와 연결되지 않은 경우 -1입니다.
        /// </summary>
        public int QuestIndex { get; }

        /// <summary>
        /// 문제가 발견된 Quest의 ID입니다.
        /// ID가 없거나 Quest를 확인할 수 없는 경우 null 또는 빈 문자열일 수 있습니다.
        /// </summary>
        public string QuestId { get; }

        /// <summary>
        /// 문제가 발견된 Objective의 Quest 내부 인덱스입니다.
        /// Objective와 관련 없는 경우 -1입니다.
        /// </summary>
        public int ObjectiveIndex { get; }

        /// <summary>
        /// 문제가 발견된 Objective의 ID입니다.
        /// Objective와 관련 없거나 ID를 확인할 수 없는 경우 null 또는 빈 문자열일 수 있습니다.
        /// </summary>
        public string ObjectiveId { get; }

        /// <summary>
        /// 문제가 발견된 선행 Quest 항목의 인덱스입니다.
        /// 선행 Quest와 관련 없는 경우 -1입니다.
        /// </summary>
        public int PrerequisiteIndex { get; }

        /// <summary>
        /// 문제가 발견된 Reward의 인덱스입니다.
        /// Reward와 관련 없는 경우 -1입니다.
        /// </summary>
        public int RewardIndex { get; }

        internal QuestValidationIssue(QuestValidationSeverity severity, string code, string message, int questIndex = -1, string questId = null, int objectiveIndex = -1, string objectiveId = null, int prerequisiteIndex = -1, int rewardIndex = -1)
        {
            Severity = severity;
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            QuestIndex = questIndex;
            QuestId = questId;
            ObjectiveIndex = objectiveIndex;
            ObjectiveId = objectiveId;
            PrerequisiteIndex = prerequisiteIndex;
            RewardIndex = rewardIndex;
        }
    }
}