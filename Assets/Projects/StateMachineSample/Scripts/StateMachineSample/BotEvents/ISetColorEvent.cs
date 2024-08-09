using m039.Common.Events;
using UnityEngine;

namespace Game
{
    public interface ISetColorEvent : IEventSubscriber
    {
        void SetColor(Color color);
    }
}
