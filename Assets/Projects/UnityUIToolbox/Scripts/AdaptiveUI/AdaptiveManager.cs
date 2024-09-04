using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace m039.UIToolbox.Adaptive
{
    public class AdaptiveManager : MonoBehaviour
    {
        enum Orientation
        {
            Portrait, Landscape
        }

        protected static AdaptiveManager s_Instance;

        public static AdaptiveManager Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    var go = new GameObject(typeof(AdaptiveManager).Name);
                    s_Instance = go.AddComponent<AdaptiveManager>();
                }

                return s_Instance;
            }
        }

        readonly Dictionary<string, AdaptiveView> _views = new();

        readonly Dictionary<string, AdaptiveLayout> _layouts = new();

        Orientation _currentOrientation = Orientation.Portrait;

        void Awake()
        {
            _currentOrientation = Screen.width > Screen.height ? Orientation.Landscape : Orientation.Portrait;
        }

        public void Register(AdaptiveView view)
        {
            if (_views.ContainsKey(view.id))
            {
                Debug.LogError($"The view with {view.id} is already registered.");
                return;
            }

            GetOrCreateLayout(_currentOrientation).Attach(view);

            _views.Add(view.id, view);
        }

        AdaptiveLayout GetOrCreateLayout(Orientation orientation)
        {
            var path = $"AdaptiveUI/Layout_{(Application.isMobilePlatform ? "Mobile" : "Desktop")}_{(orientation == Orientation.Portrait ? "Portrait" : "Landscape")}";
            if (_layouts.ContainsKey(path))
            {
                return _layouts[path];
            }

            var go = Resources.Load<GameObject>(path);
            if (go == null)
            {
                throw new Exception($"Can't find layout in '{path}'");
            }

            var instance = Instantiate(go);
            instance.transform.SetParent(transform);

            var layout = instance.GetOrAddComponent<AdaptiveLayout>();
            _layouts[path] = layout;
            return layout;
        }

        void Update()
        {
            ProcessOrientation();
        }

        void ProcessOrientation()
        {
            var newOrientation = Screen.width > Screen.height ? Orientation.Landscape : Orientation.Portrait;
            if (newOrientation != _currentOrientation)
            {
                var oldLayout = GetOrCreateLayout(_currentOrientation);
                var newLayout = GetOrCreateLayout(newOrientation);

                foreach (var view in _views.Values)
                {
                    oldLayout.Detach(view);
                    newLayout.Attach(view);
                }

                foreach (var layout in _layouts.Values)
                {
                    layout.gameObject.SetActive(layout == newLayout);
                }

                _currentOrientation = newOrientation;
            }
        }
    }
}
