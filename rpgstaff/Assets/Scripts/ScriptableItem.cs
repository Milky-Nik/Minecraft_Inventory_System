using UnityEngine;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item")]
public class ScriptableItem : ScriptableObject
{
    public string id { private set; get; }
    public string ItemName;
    public int MaxInStack;
    [HideInInspector] public int Amount = 0;
    public Sprite ItemIcon;

    public ScriptableItem Create(int NewAmount)
    {
        var data = ScriptableObject.CreateInstance<ScriptableItem>();
        data.Amount = NewAmount;
        data.ItemName = ItemName;
        data.MaxInStack = MaxInStack;
        data.ItemIcon = ItemIcon;

        data.id = Guid.NewGuid().ToString();

        return data;
    }
}
