using System;
using Godot;

public class ClientNetServe : NetServe
{
    private static int connctTimes = 0;

    public ClientNetServe(MultiplayerApi Multiplayer, string ip, int port) : base(Multiplayer)
    {
        if (connctTimes == 0)
        {
            Multiplayer.ConnectedToServer += OnConnectedToServer;
            Multiplayer.ConnectionFailed +=OnConnectionFailed;
            Multiplayer.ServerDisconnected += OnServerDisconnected;
        }
        var peer = new ENetMultiplayerPeer();
        if (peer.CreateClient(ip, port) == Error.Ok)
        {
            GD.Print("正在尝试连接到服务器...");
            Multiplayer.MultiplayerPeer = peer;
            GD.Print("正在初始化");
        }
        connctTimes += 1;
    }
    private void OnConnectedToServer()
    {
        GD.Print("成功连接到服务器。");
        SceneChangeManager.Instance.ChangeScene(StringResource.MainGame);
        NetManager.Instance.RpcId(1, "LoadGameManager", Multiplayer.GetUniqueId());
    }
    private void OnConnectionFailed()
    {
        GD.Print("连接到服务器失败。");
    }
    private void OnServerDisconnected()
    {
        GD.Print("与服务器断开连接。");
        SceneChangeManager.Instance.ChangeScene(StringResource.ConnectTscn);
    }
}