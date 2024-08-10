using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class Bonfire : GameEntity
    {
        #region Inspector

        [SerializeField]
        float _SpawnRadius = 1f;

        #endregion

        public override float spawnRadius => _SpawnRadius;

        public override GameEntityType type => GameEntityType.Bonfire;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }
    }
}
