using Game.BehaviourTreeSample;
using UnityEngine;

namespace Game.GOAPSample
{
    public class House : GameEntity
    {
        #region Inspector

        [SerializeField]
        float _SpawnRadius = 1f;

        [SerializeField]
        Transform _Pivot;

        #endregion

        public override Vector2 position
        {
            set
            {
                var p = transform.position;
                p.x = value.x;
                p.y = value.y;
                transform.position = p;

                var y = _Pivot.position.y;
                p.z = Mathf.InverseLerp(-100, 100, y) * 100;
                transform.position = p;
            }
        }

        public override float spawnRadius => _SpawnRadius;

        public override GameEntityType type => GameEntityType.House;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _SpawnRadius);
        }
    }
}
