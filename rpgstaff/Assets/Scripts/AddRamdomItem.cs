using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class AddRamdomItem : MonoBehaviour
{
    Inventory inv;
    [SerializeField] List<ScriptableItem> ItemList = new List<ScriptableItem>();

    private void Start()
    {
        inv = GetComponent<Inventory>();
    }

    public void AddRandomItemRandomAmountToInventory()
    {
        ScriptableItem tempItem = ItemList[Random.Range(0, ItemList.Count)];
        tempItem = tempItem.Create(1);

        print("Created: " + tempItem.ItemName + ", Amount: " + tempItem.Amount + ", ID: " + tempItem.id);

        inv.PickUpItem(tempItem);
    }
}
