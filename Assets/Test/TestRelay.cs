using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestRelay : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_InputField inputField;

    [SerializeField] private GameObject buttonsPanel;

    [SerializeField] private GameObject menuCamera;

    [SerializeField] private Image errorImage;

    [SerializeField] private Canvas playerHUD;

    [SerializeField] private TMP_InputField joinCodeField;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();


    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"{joinCode}");

            joinCodeField.text = joinCode;

            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();
            OnSuccessConnection();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            ShowErrorSprite();
        }
    }

    public async void JoinRelay()
    {

        string joinCode = inputField.text;

        try
        {
            Debug.Log("Joining relay with " + joinCode);

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
            OnSuccessConnection();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            ShowErrorSprite();
        }
    }

    private void OnSuccessConnection()
    {
        buttonsPanel.SetActive(false);
        menuCamera.SetActive(false);
        playerHUD.gameObject.SetActive(true);
    }

    private void ShowErrorSprite()
    {
        errorImage.gameObject.SetActive(true);
    }
}
