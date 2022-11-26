using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthenticationManager : MonoBehaviour
{
    private enum AuthenticationState
    {
        INITIALIZED,
        AUTHENTICATING,
        AUTHENTICATED,
        ERROR
    }

    private AuthenticationState _authState = AuthenticationState.INITIALIZED;

    private void Awake()
    {
        UnityServices.InitializeAsync();
    }

    public async void Authenticate()
    {
        switch (_authState)
        {
            case AuthenticationState.AUTHENTICATED:
                Debug.LogWarning("Cannot authenticate, we are already authenticated.");
                return;
            case AuthenticationState.AUTHENTICATING:
                Debug.LogWarning("Authentication in progress, please wait.");
                return;
            default:
                await SignInAnonymouslyAsync();
                return;
        }
    }

    private async Task SignInAnonymouslyAsync()
    {
        _authState = AuthenticationState.AUTHENTICATING;

        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)  // Compare error code to AuthenticationErrorCodes
        {
            // Notify the player with the proper error message
            Debug.LogException(ex);
            _authState = AuthenticationState.ERROR;
        }
        catch (RequestFailedException ex)   // Compare error code to CommonErrorCodes
        {
            // Notify the player with the proper error message
            Debug.LogException(ex);
            _authState = AuthenticationState.ERROR;
        }

        GameManager.Instance.CurrentGameState = GameManager.GameState.LOBBY;
        SceneManager.LoadSceneAsync(1);
    }
}
