using Unity.Netcode;
using UnityEngine;

public class Door : NetworkBehaviour
{
    [SerializeField] private PressurePlate pressurePlate;
    [SerializeField] private Vector3 openOffset = new Vector3(0, -3f, 0); // How far the door moves/lowers when opened

    private Vector3 closedPosition;
    private Vector3 targetPosition;

    private void Start()
    {
        closedPosition = transform.position;
        targetPosition = closedPosition;

        if (pressurePlate != null)
        {
            // Subscribe to the pressure plate's state change
            pressurePlate.IsPressed.OnValueChanged += OnPlateStateChanged;

            // Set initial state in case it started pressed
            UpdateDoorState(pressurePlate.IsPressed.Value);
        }
    }

    private void Update()
    {
        // Smoothly slide the door towards its target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);
    }

    private void OnPlateStateChanged(bool previousValue, bool newValue)
    {
        UpdateDoorState(newValue);
    }

    private void UpdateDoorState(bool isPressed)
    {
        if (isPressed)
        {
            targetPosition = closedPosition + openOffset; // Open door
        }
        else
        {
            targetPosition = closedPosition; // Close door
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (pressurePlate != null)
        {
            pressurePlate.IsPressed.OnValueChanged -= OnPlateStateChanged;
        }
    }
}