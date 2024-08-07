using m039.Common.Blackboard;

namespace Game
{
#if true

    public class GameBlackboard : Blackboard
    {
    }

#else

    using System;
    using System.Collections.Generic;

    // Cached version of blackboard.
    public class GameBlackboard : BlackboardBase
    {
        static readonly Dictionary<Type, Queue<Entry>> s_EntryCache = new();

        readonly Dictionary<BlackboardKey, int> _keyIndex = new();

        readonly Entry[] _entries = new Entry[20]; 

        int _index = 0;

        public override int Count => throw new NotImplementedException();

        public override void Clear()
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i] == null)
                    continue;

                ReleaseEntry(_entries[i]);

                _entries[i] = null;
            }
        }

        public override bool ContainsKey<T>(BlackboardKey<T> key)
        {
            if (_keyIndex.TryGetValue(key, out int index))
            {
                return _entries[index] != null;
            }

            return false;
        }

        public override void Remove<T>(BlackboardKey<T> key)
        {
            if (_keyIndex.TryGetValue(key, out int index))
            {
                if (_entries[index] != null)
                {
                    ReleaseEntry(_entries[index]);
                }
                _entries[index] = null;
            }
        }

        public override void SetValue<T>(BlackboardKey<T> key, T value)
        {
            if (_keyIndex.TryGetValue(key, out int index))
            {
                if (_entries[index] == null)
                {
                    _entries[index] = GetEntry(value);
                } else
                {
                    ((Entry<T>)_entries[index]).value = value;
                }
            } else
            {
                var index2 = _index++;
                _keyIndex[key] = index2;
                _entries[index2] = GetEntry(value);
            }
        }

        public override bool TryGetValue<T>(BlackboardKey<T> key, out T value)
        {
            if (_keyIndex.TryGetValue(key, out int index) && _entries[index] != null)
            {
                value = ((Entry<T>)_entries[index]).value;
                return true;
            }
            else
            {
                value = default;
                return false;
            };
        }

        static Entry<T> GetEntry<T>(T value)
        {
            Entry<T> entry;
            var type = typeof(T);

            if (s_EntryCache.ContainsKey(type) && s_EntryCache[type].Count > 0)
            {
                entry = (Entry<T>) s_EntryCache[type].Dequeue();
            }
            else
            {
                entry = new();
            }
            entry.value = value;
            return entry;
        }

        static void ReleaseEntry(Entry entry)
        {
            var type = entry.GetValueType();
            entry.Clear();

            if (!s_EntryCache.ContainsKey(type))
            {
                s_EntryCache[type] = new();
            }

            s_EntryCache[type].Enqueue(entry);
        }

        abstract class Entry
        {
            public abstract Type GetValueType();

            public abstract void Clear();
        }

        class Entry<T> : Entry
        {
            public T value;

            public override void Clear()
            {
                if (value is IReleasable releasable)
                {
                    releasable.Release();
                }
                value = default;
            }

            public override Type GetValueType() => typeof(T);
        }
    }
#endif
}
