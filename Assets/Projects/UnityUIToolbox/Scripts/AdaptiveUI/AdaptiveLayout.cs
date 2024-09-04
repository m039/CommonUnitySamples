using Game;
using System.Collections.Generic;
using UnityEngine;

namespace m039.UIToolbox.Adaptive
{
    public class AdaptiveLayout : MonoBehaviour
    {
        readonly Dictionary<string, AdaptiveViewContainer> _containers = new();

        void Awake()
        {
            FindAllContainers();
        }

        void FindAllContainers()
        {
            foreach (var container in GetComponentsInChildren<AdaptiveViewContainer>(true))
            {
                if (_containers.ContainsKey(container.id))
                {
                    Debug.LogError($"Container with '{container.id}' id is already exist.");
                    continue;
                }

                _containers.Add(container.id, container);
            }
        }

        public void Attach(AdaptiveView view)
        {
            if (!_containers.ContainsKey(view.id))
            {
                Debug.LogError($"Can't attach the view, there is no container with the same id '{view.id}'");
                return;
            }

            view.transform.SetParent(_containers[view.id].transform, false);
        }

        public void Detach(AdaptiveView view)
        {
            view.transform.SetParent(null, false);
        }
    }
}
