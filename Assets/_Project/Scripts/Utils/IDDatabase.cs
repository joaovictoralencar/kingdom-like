using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingdomLike.Utils
{
    public static class IDDatabase
    {
        private static readonly Dictionary<Guid, ScriptableObjectWithID> _entries = new();

        public static bool IsInitialized { get; private set; }

        public static int Count => _entries.Count;

        public static void Initialize(IEnumerable<ScriptableObjectWithID> entries)
        {
            Clear();

            if (entries == null)
            {
                IsInitialized = true;
                return;
            }

            foreach (ScriptableObjectWithID entry in entries)
            {
                if (entry == null)
                    continue;

                if (entry.Id == null)
                {
                    Debug.LogError($"Cannot register {entry.name} because it does not have an ID.", entry);
                    continue;
                }

                Guid id = entry.Id.Id;

                if (_entries.ContainsKey(id))
                {
                    Debug.LogError($"Duplicate ID detected while registering {entry.name}: {id}.", entry);
                    continue;
                }

                _entries.Add(id, entry);
            }

            IsInitialized = true;
        }

        public static bool TryGet<T>(
            Guid id,
            out T entry)
            where T : ScriptableObjectWithID
        {
            if (_entries.TryGetValue(id, out ScriptableObjectWithID value) &&
                value is T typedValue)
            {
                entry = typedValue;
                return true;
            }

            entry = null;
            return false;
        }

        public static bool TryGet(
            Guid id,
            out ScriptableObjectWithID entry)
        {
            return _entries.TryGetValue(id, out entry);
        }

        public static bool Contains(Guid id)
        {
            return _entries.ContainsKey(id);
        }

        public static void Clear()
        {
            _entries.Clear();
            IsInitialized = false;
        }
    }
}