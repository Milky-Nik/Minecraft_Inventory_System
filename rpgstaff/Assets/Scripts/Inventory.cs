using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour
{
    [SerializeField] List<Slot> ItemInventorySlots = new List<Slot>();
    [SerializeField] Slot CursorSlot;
    
    [Space(20)]

    PlayerInput playerInput;
    InputAction leftClick;
    InputAction leftClick_DoubleTap;
    InputAction rightClick;
    InputAction shiftKey;
    InputAction dropKey;

    [SerializeField] GraphicRaycaster ui_raycaster;
    PointerEventData click_data;
    List<RaycastResult> click_results;

    bool IsHoldingShift = false;
    bool RightMouseButtonIsHolding = false;
    List<Slot> HoveredSlots = new List<Slot>();


    private void Start()
    {

        click_data = new PointerEventData(EventSystem.current);
        click_results = new List<RaycastResult>();

        playerInput = GetComponent<PlayerInput>();
        leftClick = playerInput.actions["LMB"];
        leftClick_DoubleTap = playerInput.actions["LMB_DoubleTap"];
        rightClick = playerInput.actions["RMB"];
        shiftKey = playerInput.actions["Shift"];
        dropKey = playerInput.actions["Drop"];

        dropKey.performed += ctx =>
        {
            var HoveredSlot = GetSlotMouseHovered();
            if (HoveredSlot == null || HoveredSlot.Item == null) return;
            HoveredSlot.RemoveItem();

        };

        shiftKey.performed += ctx =>
        {
            IsHoldingShift = true;
        };
        shiftKey.canceled += ctx =>
        {
            IsHoldingShift = false;
        };

        leftClick_DoubleTap.performed += ctx =>
        {
            var clickedslot = GetSlotMouseHovered();
            if (clickedslot == null) return;
            if (!clickedslot.IsOccupied || CursorSlot.IsOccupied || !clickedslot.IsStackable()) return;

            foreach(var FoundedSlot in ItemInventorySlots.FindAll(x => x.IsOccupied && clickedslot.ItemName == x.ItemName))
            {
                if (!clickedslot.IsStackable()) break;

                ScriptableItem UpdatedItem = FoundedSlot.Item.Create(FoundedSlot.Item.Amount);
                clickedslot.UpdateStack(UpdatedItem, out UpdatedItem);

                if (UpdatedItem.Amount == 0) FoundedSlot.RemoveItem();
                else FoundedSlot.AddToStack(FoundedSlot.Item.Amount - UpdatedItem.Amount);
            }
        };
        leftClick.performed += ctx =>
        {
            var clickedslot = GetSlotMouseHovered();
            if (clickedslot == null) return;

            if (CursorSlot.IsOccupied)
            {
                if (clickedslot.IsOccupied)
                {
                    if (CursorSlot.ItemName == clickedslot.ItemName && clickedslot.IsStackable() && CursorSlot.IsStackable())
                    {
                        ScriptableItem UpdatedItem = CursorSlot.Item.Create(CursorSlot.Item.Amount);
                        clickedslot.UpdateStack(UpdatedItem, out UpdatedItem);
                        if (UpdatedItem.Amount == 0) CursorSlot.RemoveItem();
                        else CursorSlot.AddItem(UpdatedItem);
                        return;
                    }

                    var temp = clickedslot.Item;
                    clickedslot.AddItem(CursorSlot.Item);
                    CursorSlot.AddItem(temp);
                    return;
                }

                clickedslot.AddItem(CursorSlot.Item);
                CursorSlot.RemoveItem();
                return;
            }

            if (clickedslot.IsOccupied)
            {
                if (IsHoldingShift)
                {
                    Slot tempslot = null;
                    if(clickedslot.IsHotbarSlot)
                    {
                        tempslot = FindEmptySlotInInventory();
                        if (tempslot != null) tempslot.AddItem(clickedslot.Item);
                        else return;
                    }
                    else
                    {
                        tempslot = FindEmptySlotInHotbar();
                        if (tempslot != null) tempslot.AddItem(clickedslot.Item);
                        else return;
                    }
                    clickedslot.RemoveItem();
                    return;
                }

                CursorSlot.AddItem(clickedslot.Item);
                clickedslot.RemoveItem();
            }
        };

        rightClick.performed += ctx =>
        {
            RightMouseButtonIsHolding = true;
            var clickedslot = GetSlotMouseHovered();
            if (clickedslot == null) return;

            if (CursorSlot.IsOccupied)
            {
                if (clickedslot.IsOccupied)
                {
                    if (CursorSlot.ItemName == clickedslot.ItemName)
                    {
                        if (!clickedslot.IsStackable()) return;

                        clickedslot.AddToStack(1);
                        CursorSlot.AddToStack(-1);
                        return;
                    }

                    var temp = clickedslot.Item;
                    clickedslot.AddItem(CursorSlot.Item);
                    CursorSlot.AddItem(temp);
                    return;
                }
                return;
            }

            if (clickedslot.IsOccupied)
            {
                int biggestAmountHalf = (int)Math.Ceiling((double)clickedslot.Item.Amount / 2);
                CursorSlot.AddItem(clickedslot.Item.Create(biggestAmountHalf));
                clickedslot.AddToStack(-biggestAmountHalf);
            }
        };
        rightClick.canceled += ctx =>
        {
            RightMouseButtonIsHolding = false;
            HoveredSlots.Clear();
        };
    }

    private void Update()
    {
        CursorSlot.gameObject.transform.position = Mouse.current.position.ReadValue();
    }

    private void LateUpdate()
    {
        if (!RightMouseButtonIsHolding || CursorSlot.Item == null) return; // When holding right mouse button with item, add 1 item to hovered slot from holding stack
        var HoveredSlot = GetSlotMouseHovered();
        if (HoveredSlot == null || HoveredSlots.Contains(HoveredSlot)) return;

        HoveredSlots.Add(HoveredSlot);
        
        if (HoveredSlot.Item == null)
        {
            HoveredSlot.AddItem(CursorSlot.Item.Create(1));
            CursorSlot.AddToStack(-1);
            return;
        }
        if (CursorSlot.ItemName == HoveredSlot.ItemName && HoveredSlot.IsStackable())
        {
            HoveredSlot.AddToStack(1);
            CursorSlot.AddToStack(-1);
        }
    }


    public void PickUpItem(ScriptableItem Item) // FIND CORRECT SLOT FOR PICKED UP ITEM AND ADD IT
    {
        ScriptableItem UpdatedItem = Item.Create(Item.Amount);

        foreach (var slot in ItemInventorySlots.FindAll(slot => slot.IsOccupied && slot.IsStackable() && slot.ItemName == Item.ItemName))
        {
            slot.UpdateStack(UpdatedItem, out UpdatedItem);
            if (UpdatedItem.Amount == 0) return;
        }
        Slot FindEmptySlot = ItemInventorySlots.Find(x => !x.IsOccupied);
        if(FindEmptySlot != null) FindEmptySlot.AddItem(UpdatedItem);
    }

    Slot GetSlotMouseHovered()
    {
        /** Get all the UI elements clicked, using the current mouse position and raycasting. **/
        click_data.position = Mouse.current.position.ReadValue();
        click_results.Clear();
        ui_raycaster.Raycast(click_data, click_results);

        Slot clickedslot;

        foreach (RaycastResult result in click_results)
        {
            clickedslot = result.gameObject.GetComponentInParent<Slot>();
            if (clickedslot != null) return clickedslot;
        }
        return null;
    }

    Slot FindEmptySlotInInventory()
    {
        Slot tempslot = null;
        for (int i = 9;i <= ItemInventorySlots.Count; i++)
        {
            if (!ItemInventorySlots[i].IsOccupied)
            {
                print("found: " + ItemInventorySlots[i].name);
                tempslot = ItemInventorySlots[i];
                break;
            }
        }
        return tempslot;
    }

    Slot FindEmptySlotInHotbar()
    {
        Slot tempslot = null;
        for (int i = 0; i <= 9; i++)
        {
            if (!ItemInventorySlots[i].IsOccupied)
            {
                print("found: " + ItemInventorySlots[i].name);
                tempslot = ItemInventorySlots[i];
                break;
            }
        }
        return tempslot;
    }
}