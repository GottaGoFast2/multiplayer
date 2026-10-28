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

    private Lobby hostLobby;       //the lobby the host made

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


    private void SetButtons(bool on)
    {

        createGameBtn.interactable = on;  //Prevents the buttons from being pressed before relay happens (not here but below).
        joinGameBtn.interactable = on;
    }
    async void CreateGame() //Only the host creates the relay
    {
        SetButtons(false);

            //Relays to 4 other users (not counting host)
            var allocation = await RelayService.Instance.CreateAllocationAsync(4);

            //Allocation id is one of the things needed inside GetJoinCode
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            //Make a public lobby and store the join code inside it
            var options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Data = new Dictionary<string, DataObject>
            {
                { "JoinCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) } //allows client to read join code.
            };
            hostLobby = await LobbyService.Instance.CreateLobbyAsync("Game", 5, options); //5 people including the host
            InvokeRepeating(nameof(SendTicker), 15f, 15f); //Sends tick ever 15 seconds to allow people to join after some time period.

            //Need the Unity Transport for the relay (sending and receiving of network data)
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new RelayServerData(allocation, "dtls")); //Both host a client will have dtls to make the data safe.
            NetworkManager.Singleton.StartHost();

            joinCodeText.text = "Join Code: " + joinCode;
            menuPanel.SetActive(false);
        
    }

    async void JoinGame()
    {
        SetButtons(false);

            Lobby found = await LobbyService.Instance.QuickJoinLobbyAsync();
            //Look for a lobby


            string joinCode = found.Data["JoinCode"].Value;
            //Reads the join code the host has


            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            //Use the code so that the client can join the host relay.

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>(); //Need the Unity Transport for the relay (sending and receiving of network data)
            transport.SetRelayServerData(new RelayServerData(joinAllocation, "dtls")); //The encription to keep players data safe.
            NetworkManager.Singleton.StartClient();

            menuPanel.SetActive(false);
    }

    private void SendTicker()
    {
        LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id); //Create ticker so that the player can join after a long time.
    }

}