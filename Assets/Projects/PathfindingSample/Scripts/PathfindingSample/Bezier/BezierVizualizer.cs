using System.Linq;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(PathSmoother))]
    public class BezierVizualizer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        Transform[] _Points;

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (_Points.Length <= 2)
                return;

            var pathSmoother = GetComponent<PathSmoother>();
            var positions = pathSmoother.GetSmoothPath(_Points.Select(p => p.position).ToArray());
            Gizmos.color = Color.blue;
            for (int i = 0; i < positions.Count; i++)
            {
                Gizmos.DrawSphere(positions[i], 0.1f);
            }
        }
    }
}
