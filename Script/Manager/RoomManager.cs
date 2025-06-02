using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RoomManager : Node, IStartGame, IUpdateRoomUi
{
    public static RoomManager Instance { get; private set; }
    public Dictionary<int, Room> rooms;//服务端使用
    
    //public List<int> players = new List<int>();//本地使用，本地同房间的玩家
    [Export] public ItemList roomList;//服务端使用
    [Export] public ItemList playerList;
    [Export] public LineEdit roomId;
    [Export] public Button EnterRoomBtn;
    [Export] private Button ExitRoomBtn;
    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        rooms = new Dictionary<int, Room>();
        if (EnterRoomBtn != null)
        {
            EnterRoomBtn.Pressed += OnEnterRoom;
        }
        if (!Multiplayer.IsServer())
        {
            SignalEventCenter.Instance.RegisterEvent(this, StringResource.StartGame);
            ExitRoomBtn.Pressed += ExitRoomPress;
        }
        SignalEventCenter.Instance.RegisterEvent(this, StringResource.UpdateRoomUi);
    }
    private void ExitRoomPress()
    {
        NetManager.Instance.RpcId(1, "ExitRoom", Multiplayer.GetUniqueId());
         
    }
    public override void _Process(double delta)
    {
        base._Process(delta);
    }
    private void OnEnterRoom()//向服务端发送申请加入房间
    {
        if (!Multiplayer.IsServer())
        {
            int roomid = int.Parse(roomId.Text);
            int peerId = Multiplayer.GetUniqueId();
            NetManager.Instance.RpcId(1, "EnterRoom", roomid, peerId);
        }
    }
    public void PlayerEnterRoom()
    {
        EnterRoomBtn.Hide();
        roomId.Hide();
    }
    public void StartGame()
    {

        if (IsInstanceValid(playerList))
            playerList.Hide();
        if (IsInstanceValid(ExitRoomBtn))
            ExitRoomBtn.Hide();
    }
    public void UpdateRoomUi()
    {
        var onlinePlayers = NetManager.Instance.netServe.players;
        playerList.Clear();
        if (roomList != null)
        {
            roomList.Clear();
            foreach (var room in rooms.Values)
            {
                roomList.AddItem("房间号" + room.id.ToString());
            }
        }
        // 更新玩家数量
        //count.Text = onlinePlayers.Count.ToString();

        // 添加所有在线玩家的 ID
        foreach (var playerId in onlinePlayers)
        {
            playerList.AddItem(playerId.ToString());
        }
    }
    /// <summary>
    /// 请求者加入指定房间,仅服务端处理
    /// </summary>
    /// <param name="roomId"></param>
    /// <param name="peerId"></param>
    public Room EnterRoom(int roomId, int peerId)
    {
        GD.Print(peerId + "进入" + roomId + "房间");
        Room room;
        rooms.TryGetValue(roomId, out room);
        if (room == null)
        {
            room = CreateRoom(roomId, roomId.ToString());
            room.hostId = peerId;
        }
        rooms[roomId] = room;
        room.players.Add(peerId);
        return room;
    }
    public bool TryGetRoom(int roomId, out Room room)
    => rooms.TryGetValue(roomId, out room);
    private Room CreateRoom(int roomId, string roomName)
    {
        var room = new Room()
        {
            id = roomId,
            name = roomName,
            players = new List<int>()
        };
        return room;
    }
    public int LeaveRoom(int playerId, bool isQueue = true)
    {
        int roomId = -1;
        lock (rooms) // 确保线程安全
        {
            // 1. 遍历所有房间寻找该玩家
            List<int> roomsToRemove = new List<int>();
            foreach (var roomEntry in rooms)
            {
                Room room = roomEntry.Value;
                if (room.players.Contains(playerId))
                {
                    // 2. 从房间中移除玩家
                    roomId = room.id;
                    room.players.Remove(playerId);
                    GD.Print($"玩家 {playerId} 已从房间 {room.id} 退出");

                    // 3. 检查并标记空房间
                    if (room.players.Count == 0)
                    {
                        GD.Print($"房间 {room.id} 已无玩家，标记销毁");
                        roomsToRemove.Add(room.id);
                    }
                    break;
                }
            }
            // 4. 销毁所有空房间
            foreach (int i in roomsToRemove)
            {
                rooms.Remove(i);
                GD.Print($"房间 {i} 已销毁");
            }
        }
        NetManager.Instance.RpcId(playerId, "SyncLeaveRoom", playerId);//向这个退出房间的玩家发出退出请求
        if (Instance.rooms.ContainsKey(roomId))//如果服务端房间存在
        {
            foreach (var existingId in Instance.rooms[roomId].players)
            {
                NetManager.Instance.RpcId(existingId, "SyncLeaveRoom", playerId);//向其他玩家发送这个id
                NetManager.Instance.RpcId(playerId, "SyncLeaveRoom", existingId);
            }
            GD.Print("房间存在，向所有房间内玩家发送广播");
        }
        else
        {
            //NetManager.Instance.RpcId(playerId, "SyncLeaveRoom", playerId);
            GD.Print("房间不存在，不发送广播");
        }
        var node = NetManager.Instance.GetNodeOrNull(playerId.ToString());
        if (isQueue)
            node.QueueFree();
        return roomId;
    }
    public class Room()
    {
        public int id;
        public int hostId;
        public string name;
        public List<int> players;
        public bool start;
    }
}