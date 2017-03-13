using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon;
using System;

public class Network : Photon.PunBehaviour
{
    public static Network Instance;

    public PhotonView pView;

    public Image connectedImage;

    void Awake()
    {
        Instance = this;
    }

	void Start ()
    {
        PhotonNetwork.ConnectUsingSettings("0.1");
	}

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        Debug.Log("failed to join a random room!");
        Debug.Log("creating room...");
        PhotonNetwork.CreateRoom(null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("joined room!");
        connectedImage.color = Color.green;
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("another player joined");
    }

    [PunRPC]
    public void StartedDrawing()
    {
        OnlinePlayer caster = RuneManager.Instance.rightCaster as OnlinePlayer;
        caster.StartedDrawing();
    }

    [PunRPC]
    public void FingerMoved(Vector3 position)
    {
        RuneManager.Instance.rightCaster.MoveTrailRenderer(position);
    }

    /// <summary>
    /// the non-host player has tried to cast a rune... the host receives this and decides its success
    /// </summary>
    /// <param name="runeName"></param>
    /// <param name="power"></param>
    [PunRPC]
    public void WantsToStartRune(string runeName, float power)
    {
        Rune rune = RuneManager.Instance.CloneRuneByName(runeName);
        rune.power = power;
        RuneManager.Instance.StartRune(rune, RuneManager.Instance.rightCaster);
    }

    /// <summary>
    /// the host says a rune has been countered - this passes that to the client
    /// </summary>
    [PunRPC]
    public void RuneCountered(bool isHost)
    {
        OnlinePlayer oCaster = RuneManager.Instance.rightCaster as OnlinePlayer;
        Player pCaster = RuneManager.Instance.leftCaster as Player;

        if (isHost)
        {
            oCaster.MyRuneWasCounteredCallback();
            pCaster.HideCurrentRuneCallback();
        }
        else
        {
            pCaster.MyRuneWasCounteredCallback();
            oCaster.HideCurrentRuneCallback();
        }
    }

    /// <summary>
    /// the host says a rune has been successfully created to the client
    /// </summary>
    /// <param name="runeName"></param>
    /// <param name="power"></param>
    /// <param name="isHost">if the rune being created is being cast by the host wizard or the client wizard</param>
    [PunRPC]
    public void RuneSuccessfullyCreated(string runeName, float power, bool isHost)
    {
        Debug.Log(Time.timeSinceLevelLoad + " - rune okayed by host!");
        Rune rune = RuneManager.Instance.CloneRuneByName(runeName);
        rune.power = power;

        if (isHost)
        {
            OnlinePlayer caster = RuneManager.Instance.rightCaster as OnlinePlayer;
            caster.AppliedRune(rune);
        }
        else
        {
            // confirmation our cast rune (left player) was a success
            Player caster = RuneManager.Instance.leftCaster as Player;
            caster.HostCreatedRune(rune);
        }
    }

    /// <summary>
    /// the host letting the client know a rune has been successfully cast (completed)
    /// the relevant caster should already have their activeRune set
    /// </summary>
    /// <param name="isHost"></param>
    [PunRPC]
    public void RuneWasCast(bool isHost)
    {
        if (isHost)
        {
            RuneManager.Instance.CastRune(RuneManager.Instance.rightCaster);
        }
        else
        {
            RuneManager.Instance.CastRune(RuneManager.Instance.leftCaster);
        }
    }

    [PunRPC]
    public void ClearRune()
    {
        OnlinePlayer caster = RuneManager.Instance.rightCaster as OnlinePlayer;
        caster.ClearRune();
        RuneManager.Instance.CancelRightRune();
    }
}
