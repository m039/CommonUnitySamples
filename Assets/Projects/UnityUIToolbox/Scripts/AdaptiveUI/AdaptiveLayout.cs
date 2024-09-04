using Game;
using System.Collections.Generic;
using UnityEngine;

namespace m039.UIToolbox.Adaptive
{
    public class AdaptiveLayout : MonoBehaviour
    {
        readonly Dictionary<string, AdaptiveViewContainer> _containers = new();

        readonly Dictionary<string, AdaptiveView> _views = new();

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

            if (_views.ContainsKey(view.id))
            {
                Debug.LogError($"The view with '{view.id}' id is already attached.");
                return;
            }

            view.transform.SetParent(_containers[view.id].transform, false);

            view.container = _containers[view.id];
            view.onAttach?.Invoke(view.container);

            _views.Add(view.id, view);
        }

        public void Detach(AdaptiveView view)
        {
            view.transform.SetParent(null, false);

            if (!_views.ContainsKey(view.id))
            {
                Debug.LogError($"Can't detach the view with '{view.id}' id.");
                return;
            }

            _views.Remove(view.id);

            view.container = null;
            view.onDetach?.Invoke();
        }

        public void HideUnusedContainers()
        {
            foreach (var container in _containers.Values)
            {
                if (container.hideIfNoView)
                {
                    container.gameObject.SetActive(_views.ContainsKey(container.id));
                }
            }
        }
    }
}
