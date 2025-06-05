using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class GameManager : Node, IStartGame
{
    public static GameManager Instance { get; private set; }
    public int roomId;
    public Player player;
    public bool IsHost = false;
    public override void _Ready()
    {
        base._Ready();
        if (Instance == null && !Multiplayer.IsServer())
        {
            Instance = this;
            SignalEventCenter.Instance.RegisterEvent(this, StringResource.StartGame);
        }

    }
    public override void _ExitTree()
    {
        base._ExitTree();
        if (Instance == this)
        {
            Instance = null;
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ServeSendMsg(long senderPeerId, int roomId, string message)
    {
        foreach (var id in RoomManager.Instance.rooms[roomId].players)
        {
            RpcId(id, MethodName.ClientUpdateMessage, senderPeerId, message);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void ClientUpdateMessage(long playerId, string message)
    {
        ChatManager.Instance.UpdateMessage(message);
    }
    public void SendLetter(string message)//本地发送信件
    {
        GD.Print("发送信件");
        GD.Print(message);
        RpcId(1, MethodName.FinishLetter, roomId, Multiplayer.GetUniqueId(), message);

    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void FinishLetter(int roomId, int peerId, string message)//服务端接受并且转发信件
    {
        RpcId(RoomManager.Instance.rooms[roomId].hostId, MethodName.SyncUpdateLetter, peerId, message);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void SyncUpdateLetter(int peerId, string message)//收信人接受信件
    {
        GrillHost.Instance.ReceiveMessage(peerId, message);
    }
    public void EnterRoom()
    {
        var GameType = IsHost ? "Start" : "Chose";
        ResManager.Instance.CreateInstance<StartGameBtn>(StringResource.StartGameBtnTscn, GetNode("/root/MainGame2"), GameType);
    }
    public void StartGame()
    {
        var GameType = IsHost ? "HostGame" : "GrillGame";
        ResManager.Instance.CreateInstance<Control>(StringResource.TestGameTscn, GetNode("/root/MainGame2"), GameType);
    }
    public void FinishScore(int peerId, int score)
    {
        RpcId(1, MethodName.SendScore, peerId, score);
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void SendScore(int peerId, int score)
    {
        RpcId(peerId, MethodName.SyncUpdateScore, score);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void SyncUpdateScore(int score)
    {
        GrillGameManager.Instance.ReceiveLetter(score);
    }
}
