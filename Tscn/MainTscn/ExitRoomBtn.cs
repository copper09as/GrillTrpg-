using Godot;
using System;

public partial class ExitRoomBtn : Button
{
    public override void _Ready()
    {
        base._Ready();
        this.Pressed += ExitRoom;
    }
    public void ExitRoom()
    {
        NetManager.Instance.RpcId(1, "ExitRoom", Multiplayer.GetUniqueId());
        SceneChangeManager.Instance.ChangeScene(StringResource.MainGame);
    }

}
