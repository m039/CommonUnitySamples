using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Game
{
    [RequireComponent(typeof(Animator))]
    public class AnimationPlayer : MonoBehaviour, IAnimationClipSource
    {
        #region Inspector

        [SerializeField]
        AnimationData _Data;

        #endregion

        PlayableGraph _graph;

        AnimationPlayableOutput _output;

        AnimationClipPlayable _playable;

        Dictionary<string, AnimationData.Data> _dataByStateName;

        Dictionary<int, AnimationData.Data> _dataByHashedStateName;

        void Init()
        {
            if (_dataByStateName == null)
            {
                if (_Data == null)
                {
                    _dataByStateName = new();
                }
                else
                {
                    _dataByStateName = _Data.GetData().ToDictionary(d => d.stateName, d => d);
                }
            }

            if (_dataByHashedStateName == null)
            {
                _dataByHashedStateName = _dataByStateName.ToDictionary(a => Animator.StringToHash(a.Key), a => a.Value);
            }

            if (_graph.IsValid())
                return;

            _graph = PlayableGraph.Create();
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            _output = AnimationPlayableOutput.Create(_graph, "Animation", GetComponent<Animator>());
        }

        public void Play(int stateName, float playSpeed = 1f)
        {
            Init();

            PlayInternal(_dataByHashedStateName[stateName], playSpeed);
        }

        void PlayInternal(AnimationData.Data data, float playSpeed = 1f)
        {
            if (_playable.IsValid())
            {
                _playable.SetTime(0);
                _graph.Evaluate();
                _playable.Destroy();
            }

            _playable = AnimationClipPlayable.Create(_graph, data.animationClip);
            _playable.SetSpeed(playSpeed);
            _output.SetSourcePlayable(_playable);
            _graph.Play();
        }

        public void GetAnimationClips(List<AnimationClip> results)
        {
            if (_Data == null)
                return;

            results.AddRange(_Data.GetData().Select(d => d.animationClip));
        }

        private void OnDisable()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

    }
}
