﻿using UnityEngine;

public class ClickableItem : ClickableThing {
    [SerializeField] private ItemDataSO itemData;
    [SerializeField] private AudioClip itemTaked;

    public override void HandleClick() {
        base.HandleClick();
        Player.inst.controller.MoveTo(transform.position, () => {
            PlayerInventory.inst.PickupItem(new Item(itemData));
            AudioManager.inst.Play(new AudioQuery(itemTaked));
            FindObjectOfType<MagicOrchestrator>().items[key] = true;
            Destroy(gameObject);
        });
    }
}