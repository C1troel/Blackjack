using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestNetworkUI : MonoBehaviour
{
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startClientButton;

    [SerializeField] private GameObject buttonsPanel;

    [SerializeField] private GameObject menuCamera;
    void Start()
    {
        startHostButton.onClick.AddListener(call: () =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                OnSuccessConnection();
                print("Host started");
            }
            else
                print("Host failed");
        });

        startClientButton.onClick.AddListener(call: () =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                OnSuccessConnection();
                print("Client started");
            }
            else
                print("Client failed");
        });

        startServerButton.onClick.AddListener(call: () =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                OnSuccessConnection();
                print("Server started");
            }
            else
                print("Server failed");
        });
    }

    private void OnSuccessConnection()
    {
        buttonsPanel.SetActive(false);
        menuCamera.SetActive(false);
    }
}
