using Unity.Netcode;
using UnityEngine;

public class PressurePlate : NetworkBehaviour
{
    // Synchronizes the pressed state across all clients
    public NetworkVariable<bool> IsPressed = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private MeshRenderer plateRenderer;
    [SerializeField] private Color defaultColor = Color.red;
    [SerializeField] private Color pressedColor = Color.green;

    private int playerCount = 0;

    private void Start()
    {
        if (plateRenderer != null)
        {
            plateRenderer.material.color = defaultColor;
        }

        IsPressed.OnValueChanged += OnPlateStateChanged;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the Server/Host updates the network logic
        if (!IsServer) return;

        if (other.CompareTag("Player") || other.GetComponent<NetworkObject>() != null)
        {
            playerCount++;
            IsPressed.Value = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player") || other.GetComponent<NetworkObject>() != null)
        {
            playerCount--;
            if (playerCount <= 0)
            {
                playerCount = 0;
                IsPressed.Value = false;
            }
        }
    }

    private void OnPlateStateChanged(bool previousValue, bool newValue)
    {
        if (plateRenderer != null)
        {
            plateRenderer.material.color = newValue ? pressedColor : defaultColor;
        }
    }
}