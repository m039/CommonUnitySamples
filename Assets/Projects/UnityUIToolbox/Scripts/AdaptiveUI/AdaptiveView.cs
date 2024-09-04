using System;
using UnityEngine;

namespace m039.UIToolbox.Adaptive
{
    public class AdaptiveView : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        string _Id;

        #endregion

        public System.Action<AdaptiveViewContainer> onAttach;

        public System.Action onDetach;

        [NonSerialized]
        public AdaptiveViewContainer container;

        public string id => _Id;

        private void Awake()
        {
            AdaptiveManager.Instance.Register(this);
        }

        
    }
}
