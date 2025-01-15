using System;
using UnityEngine;

public class TableSO<TKey, TValue> : ScriptableObject 
    where TKey : Enum {
    [SerializeField] private PropertySet<TKey, TValue> _table;     
    public TValue this[TKey key] => _table[key];
}
