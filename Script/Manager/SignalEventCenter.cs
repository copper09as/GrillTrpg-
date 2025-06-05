using Godot;
using System;
using System.Collections.Generic;

public partial class SignalEventCenter : Node
{
    [Signal]
    private delegate void StartGameEventHandler();
    [Signal]
    private delegate void StartGameIntEventHandler(int i);
    [Signal]
    private delegate void UpdateRoomUiEventHandler();
    public static SignalEventCenter Instance;
    public override void _Ready()
    {
        base._Ready();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
        }
    }
    // 注册普通事件
    public void RegisterEvent(Node ob, string signalName, uint flags = 0)
    {
        var callable = new Callable(ob, signalName);
        if (!IsConnected(signalName, callable))
        {
            Connect(signalName, callable, flags);
        }
    }
    // 取消注册事件
    public void UnregisterEvent(Node ob, string signalName)
    {
        var callable = new Callable(ob, signalName);
        if (!IsConnected(signalName, callable))
        {
            Disconnect(signalName, callable);
        }
    }
    // 触发事件
    public void TriggerEvent(string signalName)
    {
        EmitSignal(signalName);
    }
    public void TriggerEvent(string signalName, params Variant[] args)
    {
        EmitSignal(signalName, args);
    }
}
