using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "AnimationData", menuName = "Game/AnimationData", order = 1)]
    public class AnimationData : ScriptableObject
    {
        #region Inspector

        [SerializeField]
        Data[] _Data;

        #endregion

        public Data[] GetData() => _Data;

        [Serializable]
        public struct Data
        {
            public string stateName;
            public AnimationClip animationClip;
        }
    }
}
