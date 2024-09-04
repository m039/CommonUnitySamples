using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AdaptiveViewContainer : MonoBehaviour
    {
        #region Inspector

        [SerializeField]
        string _Id;

        #endregion

        public string id => _Id;
    }
}
