using System;
using UnityEngine;

namespace CDG.Quest
{
    /// <summary>
    /// Quest 완료 후 게임 코드가 해석할 수 있는 보상 정의 정보입니다.
    /// 실제 아이템 지급, 재화 추가 또는 기능 해금은 Quest Framework가 직접 수행하지 않습니다.
    /// </summary>
    [Serializable]
    public sealed class RewardDefinition
    {
        [SerializeField]
        private string type = string.Empty;

        [SerializeField]
        private string key = string.Empty;

        [SerializeField]
        private int amount = 1;

        /// <summary>
        /// 보상의 종류를 식별하는 문자열입니다.
        /// 예를 들어 Currency, Item, Unlock처럼 게임 코드가 해석할 값을 사용할 수 있습니다.
        /// </summary>
        public string Type => type;

        /// <summary>
        /// 실제 보상 대상을 식별하기 위한 Key입니다.
        /// 예를 들어 Gold, Potion, Stage_02와 같은 값을 사용할 수 있습니다.
        /// </summary>
        public string Key => key;

        /// <summary>
        /// 보상의 수량 또는 게임 코드가 해석할 정수 값입니다.
        /// 유효한 정의에서는 1 이상의 값을 사용합니다.
        /// </summary>
        public int Amount => amount;

        internal RewardDefinition()
        {
        }

        internal RewardDefinition(string type, string key, int amount)
        {
            this.type = type;
            this.key = key;
            this.amount = amount;
        }
    }
}