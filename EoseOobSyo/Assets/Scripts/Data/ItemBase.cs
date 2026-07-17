using UnityEngine;

public abstract class ItemBase : MonoBehaviour, IInteractable
{
    [Header("Item Info")]
    [SerializeField]
    private string itemName;

    public string ItemName => itemName;

    public virtual void PickUp(Transform holdPoint)
    {
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if(rb != null)
        {
            rb.simulated = false;
        }
    }

    public virtual void Drop()
    {
        transform.SetParent(null);

        transform.position = transform.position + Vector3.down * 0.5f;


        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if(rb != null)
        {
            rb.simulated = true;
            rb.velocity = Vector2.zero;
        }
    }

    public virtual void Interact(PlayerInventory inventory)
    {
        if(inventory.HasItem())
            return;

        inventory.PickUp(this);
    }
}