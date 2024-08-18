using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public enum ObstalceType
    {
        BigRock = 0, OneStone = 1, TwoStones = 2
    }

    public class Obstacle : GameEntity
    {
        #region Inspector

        [SerializeField]
        float _SpawnRadius = 0;

        [SerializeField]
        ObstalceType _TypeClass = ObstalceType.BigRock;

        #endregion

        public override GameEntityType type => GameEntityType.Obstacle;

        public override float spawnRadius => _SpawnRadius;

        public override int typeClass => (int)_TypeClass;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }
    }
}
