using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public enum PlayerType
    {
        Player1,
        Player2
    }

    [Header("플레이어 설정")]
    [SerializeField]
    private PlayerType playerType;

    [SerializeField]
    private float interactRange = 1f;

    private PlayerInventory inventory;

    private KeyCode interactKey;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();

        SetInteractKey();
    }

    private void SetInteractKey()
    {
        switch(playerType)
        {
            case PlayerType.Player1:
                interactKey = KeyCode.E;
                break;

            case PlayerType.Player2:
                interactKey = KeyCode.Slash;
                break;
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void Interact()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactRange
        );

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
        Gizmos.DrawWireSphere(
            transform.position,
            interactRange
        );
    }
}