using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Slot : MonoBehaviour
{
    public bool IsHotbarSlot = false;
    public bool IsOccupied { private set; get; }
    public string ItemName { private set; get; }
    public ScriptableItem Item { private set; get; }

    Image ItemIcon;
    TextMeshProUGUI AmountNumber;

    private void Start()
    {
        ItemIcon = GetComponentInChildren<Image>();
        AmountNumber = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void AddItem(ScriptableItem AddedItem)
    {
        RemoveItem();
        Item = AddedItem;
        IsOccupied = true;
        UpdateSlot();
    }

    public void RemoveItem()
    {
        Item = null;
        UpdateSlot();
    }

    public void UpdateStack(ScriptableItem AddedItem, out ScriptableItem UpdatedItem)
    {
        UpdatedItem = AddedItem;

        int difference = Item.MaxInStack - Item.Amount;

        if(AddedItem.Amount >= difference)
        {
            UpdatedItem.Amount -= difference;
            Item.Amount = Item.MaxInStack;
        }
        else
        {
            Item.Amount += AddedItem.Amount;
            UpdatedItem.Amount = 0;
        }
        UpdateSlot();
    }

    public void AddToStack(int Amount)
    {
        int newAmount = Item.Amount += Amount;
        if (newAmount <= 0)
        {
            RemoveItem();
            return;
        }
        AddItem(Item.Create(newAmount));
    }


    public bool IsStackable() => Item.Amount == Item.MaxInStack ? false : true;
    void UpdateSlot()
    {
        if(Item != null)
        {
            IsOccupied = true;
            if (Item.Amount != 1) AmountNumber.text = Item.Amount.ToString();
            ItemIcon.color = new Color32(255, 255, 255, 255);
            ItemIcon.sprite = Item.ItemIcon;
            ItemName = Item.ItemName;
        }
        else
        {
            IsOccupied = false;
            ItemIcon.color = new Color32(255, 255, 255, 0);
            ItemIcon.sprite = null;
            AmountNumber.text = null;
            ItemName = null;
        }
    }
}
