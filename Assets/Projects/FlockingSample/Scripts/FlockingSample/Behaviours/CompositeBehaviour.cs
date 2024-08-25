using UnityEngine;

namespace Game.FlockingSample
{
    [CreateAssetMenu(fileName = "CompositeBehaviour", menuName = Consts.RootMenu + "/Flocking/Composite", order = 1)]
    public class CompositeBehaviour : FlockingBehaviour
    {
        #region Inspector

        [SerializeField] BehaviourData[] _Data;

        #endregion

        public override Vector2 CalculateMove(FlockingManager manager, FlockingAgent agent)
        {
            if (_Data == null || _Data.Length == 0)
            {
                return Vector2.zero;
            }

            var move = Vector2.zero;
            foreach (var data in _Data)
            {
                var moveTmp = data.behvaiour.CalculateMove(manager, agent) * data.weight;
                if (moveTmp != Vector2.zero)
                {
                    if (moveTmp.sqrMagnitude > data.weight * data.weight)
                    {
                        moveTmp.Normalize();
                        moveTmp *= data.weight;
                    }

                    move += moveTmp;
                }
            }

            return move;
        }

        [System.Serializable]
        class BehaviourData
        {
            public FlockingBehaviour behvaiour;
            public float weight = 1f;
        }
    }
}
