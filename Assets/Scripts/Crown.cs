using Unity.Netcode;
using UnityEngine;

public class Crown : NetworkBehaviour
{
    // NetworkVariable to track if the crown/game has been won across the network
    public NetworkVariable<bool> IsCollected = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Visual Settings")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 50f, 0);

    [Header("UI")]
    [SerializeField] private GameObject victoryTextObject; // Drag your UI Text object here

    private void Update()
    {
        // Rotate the crown only if it hasn't been collected yet
        if (!IsCollected.Value)
        {
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only the Server checks for collision logic to prevent duplicate triggers
        if (!IsServer) return;

        // Check if a player touched the crown
        if (other.CompareTag("Player") || other.GetComponent<PlayerController>() != null)
        {
            if (!IsCollected.Value)
            {
                IsCollected.Value = true;
                OnCrownCollectedServer();
            }
        }
    }

    private void OnCrownCollectedServer()
    {
        Debug.Log("VICTORY!");

        // Trigger the visual changes across all connected clients
        CollectCrownClientRpc();
    }

    [ClientRpc]
    private void CollectCrownClientRpc()
    {
        // Enable the victory text
        if (victoryTextObject != null)
        {
            victoryTextObject.SetActive(true);
        }

        // Hide the crown mesh and disable its collider instead of turning off the whole GameObject
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }
}