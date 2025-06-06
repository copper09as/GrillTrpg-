using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class NetManager : Node
{
    public static NetManager Instance { get; private set; }
    [Export] private LineEdit roomId;
    public NetServe netServe;
    private NetManager() { }
    public override void _Ready()
    {
        base._Ready();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            this.QueueFree();
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void EnterRoom(int roomId, int peerId)
    {
        if (RoomManager.Instance.rooms.ContainsKey(roomId))
        {

            if (RoomManager.Instance.rooms[roomId].players.Count >= 4 || RoomManager.Instance.rooms[roomId].start)
            {
                GD.Print(roomId.ToString() + "房间已满");
                return;
            }
        }
        var room = RoomManager.Instance.EnterRoom(roomId, peerId);
        RpcId(peerId, MethodName.SetPlayer, 1, room.hostId == peerId);//分配房主权限
        RpcId(peerId, MethodName.SyncEnterRoom, peerId, roomId);//自己加入自己的房间
        foreach (var existingId in RoomManager.Instance.rooms[roomId].players)
        {
            if (existingId != (int)peerId)
            {
                RpcId(existingId, MethodName.SyncEnterRoom, peerId, roomId);//自己进入别人的房间
                RpcId(peerId, MethodName.SyncEnterRoom, existingId, roomId);//别人进入自己房间
                RpcId(peerId, MethodName.SyncLoadPlayer, existingId);//自己这里加载别人
                RpcId(existingId, MethodName.SyncLoadPlayer, peerId);//别人加载自己
            }
        }
        SignalEventCenter.Instance.TriggerEvent(StringResource.UpdateRoomUi);
    }
    /// <summary>
    /// 加入房间
    /// </summary>
    /// <param name="目标玩家"></param>
    /// <param name="目标房间"></param>
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SyncEnterRoom(int peerId, int roomId)
    {
        GD.Print("进入" + roomId + "房间");
        if (peerId == Multiplayer.GetUniqueId())
        {
            GameManager.Instance.roomId = roomId;
            RoomManager.Instance.PlayerEnterRoom();
        }
        netServe.players.Add(peerId);
        SignalEventCenter.Instance.TriggerEvent(StringResource.UpdateRoomUi);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void SyncLoadPlayer(int peerId)
    {
        if (peerId != Multiplayer.GetUniqueId())
        {
            var playerGameManager = ResManager.Instance.CreateInstance<GameManager>(StringResource.GameManagerPath, this, peerId.ToString());
            playerGameManager.player = null;
        }
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void ExitRoom(int peerId)
    {
        RoomManager.Instance.LeaveRoom(peerId, false);
        SignalEventCenter.Instance.TriggerEvent(StringResource.UpdateRoomUi);
    }
    /// <summary>
    /// 玩家房间移除目标的玩家，并且删除目标玩家
    /// </summary>
    /// <param name="peerId"></param>
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    private void SyncLeaveRoom(int peerId)//用于目标id离开plays列表
    {
        try
        {
            netServe.players.Remove(peerId);//本地移除对应id
        }
        catch (Exception e)
        {
            GD.Print("本地移除id失败"+e.ToString());
        }
        if (peerId == Multiplayer.GetUniqueId())
        {
            SceneChangeManager.Instance.ChangeScene(StringResource.MainGame);
            GD.Print("不删除自己，然后重置房间场景");
        }
        else
        {
            var node = GetNodeOrNull(peerId.ToString());
            if(node != null)
                node.QueueFree();
            GD.Print("转接器已删除");
        }
        SignalEventCenter.Instance.TriggerEvent(StringResource.UpdateRoomUi);
        GD.Print($"玩家 {peerId} 已从房间 {GameManager.Instance.roomId} 退出");
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void LoadGameManager(int id)
    {
        ResManager.Instance.CreateInstance<GameManager>(StringResource.GameManagerPath, Instance, id.ToString());
        RpcId(id, MethodName.SyncGameManager, id);
        GD.Print("转接器已生成");
        SignalEventCenter.Instance.TriggerEvent(StringResource.UpdateRoomUi);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SyncGameManager(int id)
    {
        // 客户端仅生成本地副本，不设置权限
        ResManager.Instance.CreateInstance<GameManager>(StringResource.GameManagerPath, this, id.ToString());
        SignalEventCenter.Instance.TriggerEvent(StringResource.UpdateRoomUi);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SetPlayer(int id, bool isHost)
    {
        GameManager.Instance.IsHost = isHost;
        GameManager.Instance.EnterRoom();
    }
    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    public void StartGame(int roomId, int offer)
    {
        var room = RoomManager.Instance.rooms[roomId];
        if (room.players.Count <= 1)
        {
            return;
        }
        room.start = true;
        foreach (var existingId in room.players)
        {
            RpcId(existingId, MethodName.SyncStartGame, offer);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority)]
    public void SyncStartGame(int offer)
    {
        GameDataCenter.Instance.CurrentOfferData = GameDataCenter.Instance.gameOfferData[offer];
        SignalEventCenter.Instance.TriggerEvent(StringResource.StartGame);
    }
}
