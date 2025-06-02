using Godot;

public class ServeNetServe : NetServe
{
    private static int connectTimes = 0;
    public ServeNetServe(MultiplayerApi Multiplayer, int port, int max) : base(Multiplayer)
    {
        if (connectTimes == 0)
        {
            Multiplayer.PeerConnected += OnPlayerConnected;
            Multiplayer.PeerDisconnected += OnPlayerDisconnected;
        }
        var peer = new ENetMultiplayerPeer();
        if (peer.CreateServer(port, max) == Error.Ok)
        {
            Multiplayer.MultiplayerPeer = peer;
            if (Multiplayer.IsServer())
            {
                SceneChangeManager.Instance.ChangeScene(StringResource.ServeMainGame);
                GD.Print("服务器已成功创建并运行。");
            }
            SignalEventCenter.Instance.TriggerEvent(StringResource.UpdateRoomUi);
        }
        connectTimes += 1;
    }

    private void OnPlayerConnected(long id)
    {
        if (Multiplayer.IsServer())
        {
            NetManager.Instance.netServe.players.Add((int)id);
            GD.Print(id);
        }
    }
    private void OnPlayerDisconnected(long id)
    {
        if (Multiplayer.IsServer())
        {
            NetManager.Instance.netServe.players.Remove((int)id);
            RoomManager.Instance.LeaveRoom((int)id);
            SignalEventCenter.Instance.TriggerEvent(StringResource.UpdateRoomUi);
        }
    }
}