
using m039.Common;
using m039.Common.DependencyInjection;
using UnityEngine;

namespace Game.PathfindingSample
{
    public class WorldGenerator : MonoBehaviour, IDependencyProvider
    {
        #region Inspector

        [SerializeField]
        MinMaxInt _ObstaclesCount = new(10, 30);

        #endregion

        [Provide]
        WorldGenerator GetWorldGenerator()
        {
            return this;
        }

        GameObject _parent;

        void Awake()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        public void Generate()
        {
            if (_parent != null)
            {
                Destroy(_parent);
            }

            _parent = new GameObject("< Obstacles >");

            var count = _ObstaclesCount.Random();
            for (int i = 0; i < count; i++)
            {
                var prefab = transform.GetChild(Random.Range(0, transform.childCount));
                var instance = Instantiate(prefab);
                instance.SetParent(_parent.transform, true);

                var p = CameraUtils.RandomPositionOnScreen();
                instance.position = new Vector3(p.x, p.y, instance.position.z);
                instance.gameObject.SetActive(true);
            }
        }
    }
}
