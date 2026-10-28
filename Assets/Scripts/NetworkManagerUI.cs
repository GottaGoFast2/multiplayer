using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button hostBtn;
    [SerializeField] private Button clientBtn;
    [SerializeField] private Button serverBtn;
    private Menus startButton;
    /*
    private void Awake()
    {
        startButton.StartGame().onClick.AddListener(() => {   //Lambda so we dont have to create another function (makes it easier and quicker to create empty function here)
            NetworkManager.Singleton.StartHost();
        });
        clientBtn.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
        serverBtn.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
        });
        

    }
    */ //Might not need it here
}
