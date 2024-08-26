using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class DebugLinePool
    {
        readonly LineRenderer _template;

        readonly Transform _parent;

        readonly List<LineRenderer> _lines = new();

        Queue<LineRenderer> _cachedLines;

        public DebugLinePool(LineRenderer template)
        {
            _template = template;
            _parent = new GameObject("<DebugLinePool>").transform;
        }

        public void PreUpdate()
        {
            _cachedLines = new Queue<LineRenderer>(_lines);
            foreach (var line in _lines)
            {
                line.positionCount = 0;
            }
        }

        public void DestroyLines()
        {
            if (_lines.Count <= 0)
                return;

            foreach (var l in _lines)
            {
                Object.Destroy(l.gameObject);
            }
            _lines.Clear();
        }

        public LineRenderer GetLine()
        {
            if (_cachedLines.Count > 0)
            {
                return _cachedLines.Dequeue();
            }
            else
            {
                var line = Object.Instantiate(_template);
                line.transform.SetParent(_parent);
                _lines.Add(line);
                return line;
            }
        }
    }
}
