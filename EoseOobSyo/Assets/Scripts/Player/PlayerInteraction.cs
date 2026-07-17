using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private float interactRange = 1f;

    private PlayerInventory inventory;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void Interact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);

        foreach(Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if(interactable != null)
            {
                ItemBase item = hit.GetComponent<ItemBase>();

                if(item != null && item == inventory.GetItem())
                {
                    continue;
                }

                interactable.Interact(inventory);
                return;
            }
        }

        if(inventory.HasItem())
        {
            inventory.Drop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}