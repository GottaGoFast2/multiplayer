using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class Menus : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private Button createGameBtn;
    [SerializeField] private Button joinGameBtn;
    [SerializeField] private TextMeshProUGUI joinCodeText;

    private Lobby hostLobby;       // the lobby the host made (empty until then)
    private float heartbeatTimer;  // counts down to the next "I'm still here" message

    private void Awake()
    {
        //Could also just use the .onClick in the inspector
        //Button to create game as host. Also creates relay
        createGameBtn.onClick.AddListener(CreateGame);

        //Button to join game as another player and client.
        joinGameBtn.onClick.AddListener(JoinGame);
    }

    async void Start()
    {
        //Prevents the buttons from being pressed so that the player receives their ID first
        SetButtons(false);

        await UnityServices.InitializeAsync(); //Communicates to the servers
        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //So each player can auto have a sign in (easy for us)
        Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId); //Test

        SetButtons(true); //Turns it back on after ID is received.
    }

    // Helper so we don't repeat the same two lines everywhere.
    private void SetButtons(bool on)
    {
        createGameBtn.interactable = on;
        joinGameBtn.interactable = on;
    }

    // ================= HOST =================
    async void CreateGame() //Only the host creates the relay
    {
        SetButtons(false);

        try
        {
            //Relays to 4 other users (not counting host)
            var allocation = await RelayService.Instance.CreateAllocationAsync(4);

            //Allocation id is one of the things needed inside GetJoinCode
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Make a public lobby and store the join code inside it
            var options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Data = new Dictionary<string, DataObject>
            {
                { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) }
            };
            hostLobby = await LobbyService.Instance.CreateLobbyAsync("Game", 5, options); // 5 = host + 4

            //Need the Unity Transport for the relay (sending and receiving of network data)
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls")); // must come BEFORE StartHost
            NetworkManager.Singleton.StartHost();

            joinCodeText.text = "Join Code: " + joinCode;
            menuPanel.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Create game failed: " + e.Message);
            SetButtons(true);
        }
    }

    // ================= CLIENT (player 2+) =================
    async void JoinGame()
    {
        SetButtons(false);

        try
        {
            // Find any open lobby (replaces typing a code)
            Lobby found = await LobbyService.Instance.QuickJoinLobbyAsync();

            // Read the join code the host stored in the lobby
            string joinCode = found.Data["JoinCode"].Value;

            // Use the code to join the host's relay
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            //Need the Unity Transport for the relay (sending and receiving of network data)
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls")); // must come BEFORE StartClient
            NetworkManager.Singleton.StartClient();

            menuPanel.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Join failed (is a host running?): " + e.Message);
            SetButtons(true);
        }
    }

    // ================= KEEP THE LOBBY ALIVE =================
    // Lobbies delete themselves after about 30 seconds of silence,
    // so the host pings every 15 seconds.
    private void Update()
    {
        if (hostLobby == null) return; // not a host, nothing to do

        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer <= 0f)
        {
            heartbeatTimer = 15f;
            LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
        }
    }

    // Delete the lobby when the host quits so nobody joins a dead game
    private async void OnApplicationQuit()
    {
        if (hostLobby != null)
        {
            try { await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id); }
            catch { }
        }
    }
}