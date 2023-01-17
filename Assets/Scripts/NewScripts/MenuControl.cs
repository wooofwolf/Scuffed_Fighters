using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using TMPro;

public class MenuControl : MonoBehaviour
{
    [SerializeField] private LobbyHandler _lobbyHandler;

    [SerializeField] private TMP_InputField _joinCodeInput;

    private readonly NetworkManager _networkManager = NetworkManager.Singleton;

    private void Start()
    {
        if (NetworkManager.Singleton.IsListening) return;

        CreateRelay();
    }

    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            _lobbyHandler.SetJoinCode(await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId));

            _networkManager.GetComponent<UnityTransport>().SetHostRelayData(allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData);

            _networkManager.StartHost();
            SceneTransitionHandler.Instance.RegisterCallbacks();
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }

    public async void JoinRelay()
    {
        try
        {
            _networkManager.Shutdown();

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(_joinCodeInput.text);
            _lobbyHandler.SetJoinCode(_joinCodeInput.text);

            _networkManager.GetComponent<UnityTransport>().DisconnectLocalClient();
            _networkManager.GetComponent<UnityTransport>().SetClientRelayData(joinAllocation.RelayServer.IpV4,
            (ushort)joinAllocation.RelayServer.Port,
            joinAllocation.AllocationIdBytes,
            joinAllocation.Key,
            joinAllocation.ConnectionData,
            joinAllocation.HostConnectionData);

            _networkManager.StartClient();

            Debug.Log("Lobby Joined.");
        }
        catch (RelayServiceException e)
        {
            Debug.LogException(e);
        }
    }
}
