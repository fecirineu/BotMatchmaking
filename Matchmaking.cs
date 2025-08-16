using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class Matchmaking : MonoBehaviourPunCallbacks
{

    public string serverURL= "";
    private string currentRoomName;
    public bool isReady = false;
    private bool matchFound = false;
    private string targetRoomToJoin;

    public void SetReady(bool ready)
    {
        isReady = ready;

        if (ready)
        {
            currentRoomName = "Room_" + Random.Range(1000, 9999);

            string allPlayers = "";
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                allPlayers += p.NickName + ",";
            }
            allPlayers = allPlayers.TrimEnd(',');

            StartCoroutine(SendMatchmaking("set_ready", currentRoomName, allPlayers));
            StartCoroutine(CheckForMatch());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(SendMatchmaking("unset_ready", null, PhotonNetwork.LocalPlayer.NickName));
        }
    }

    IEnumerator SendMatchmaking(string action, string room = null, string playerList = null)
    {if (!PhotonNetwork.IsMasterClient) yield return null;  // alterei aqui
        WWWForm form = new WWWForm();
        form.AddField("action", action);
        form.AddField("playerId", PhotonNetwork.LocalPlayer.NickName);
        if (room != null) form.AddField("roomName", room);
        if (playerList != null) form.AddField("players", playerList);

        using (UnityWebRequest www = UnityWebRequest.Post(serverURL, form))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
                Debug.Log("Matchmaking error: " + www.error);
            else
                Debug.Log("Matchmaking response: " + www.downloadHandler.text);
        }
    }

    public IEnumerator CheckForMatch()
    {
        while (isReady && !matchFound)
        {
            yield return new WaitForSeconds(3f);

            WWWForm form = new WWWForm();
            form.AddField("action", "check_room");
            form.AddField("playerId", PhotonNetwork.LocalPlayer.NickName);

            using (UnityWebRequest www = UnityWebRequest.Post(serverURL, form))
            {
                yield return www.SendWebRequest();

                Debug.Log("Resposta do servidor: " + www.downloadHandler.text);

                if (www.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<MatchResponse>(www.downloadHandler.text);

                    if (response.status == "found")
                    {
                        Debug.Log("Match found! Joining room: " + response.room);
                        matchFound = true;
                        targetRoomToJoin = response.room;

                        if (PhotonNetwork.InRoom)
                        {
                            PhotonNetwork.LeaveRoom();
                        }
                        else
                        {
                            TryJoinRoom();
                        }

                        yield break;
                    }
                    else
                    {
                        Debug.Log("No match yet, waiting...");
                    }
                }
                else
                {
                    Debug.LogError("Error checking room: " + www.error);
                }
            }
        }
    }

    private void TryJoinRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Conectando ao servidor...");
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        if (PhotonNetwork.NetworkClientState == ClientState.JoinedLobby)
        {
            Debug.Log("Pronto para entrar na sala: " + targetRoomToJoin);
            PhotonNetwork.JoinOrCreateRoom(targetRoomToJoin, new RoomOptions { MaxPlayers = 6 }, TypedLobby.Default);
        }
        else if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer)
        {
            Debug.Log("Entrando no lobby...");
           
        }
        else
        {
            Debug.Log("Esperando conexão estar pronta...");
            StartCoroutine(WaitForReadyThenJoin());
        }
    }

    IEnumerator WaitForReadyThenJoin()
    {
        while (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
        {
            yield return null;
        }

        PhotonNetwork.JoinOrCreateRoom(targetRoomToJoin, new RoomOptions { MaxPlayers = 6 }, TypedLobby.Default);
    }
   
    public override void OnJoinedLobby()
    {
        Debug.Log("✅ Entrou no Lobby.");
        if (matchFound && !string.IsNullOrEmpty(targetRoomToJoin))
        {
            TryJoinRoom();
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Saiu da sala. Voltando ao lobby...");
        isReady = false;
        PhotonNetwork.LeaveLobby();
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Saiu do lobby. Esperando reconectar...");
        StartCoroutine(WaitForReadyThenJoin());
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("✅ Entrou na sala: " + PhotonNetwork.CurrentRoom.Name);
        if (PhotonNetwork.CurrentRoom.Name.Contains("_"))
        {
            PhotonNetwork.LoadLevel("GameOnline");
        }
    }

    [System.Serializable]
    private class MatchResponse
    {
        public string status;
        public string room;
    }
}
