
using System.Collections.Generic;
using Godot;

public abstract class NetServe
{
    
    public List<int> players = new List<int>();//服务端使用，在线的所有玩家;客户端使用，同房间的所有玩家。
    protected MultiplayerApi Multiplayer;
    public NetServe(MultiplayerApi Multiplayer)
    {
        this.Multiplayer = Multiplayer;
    }


}