
using Godot;
using System;

public partial class StartGameBtn : TextureButton,IStartGame
{
    [Export] private OptionButton offerOption;
    public override void _Ready()
    {
        base._Ready();
        Pressed += OnStartGame;
        UpdateOffer();
        
    }
    private void OnStartGame()
    {
        if (offerOption.Selected == -1)
        {
            GD.Print("未选定");
            return;
        }
        SignalEventCenter.Instance.RegisterEvent(this, StringResource.StartGame,(uint)ConnectFlags.OneShot);
        NetManager.Instance.StartGameLocal(GameManager.Instance.roomId, offerOption.Selected);
        
    }
    private void UpdateOffer()
    {
        foreach (var i in GameDataCenter.Instance.gameOfferData)
        {
            offerOption.AddItem(i.Name);
        }
    }
    public void StartGame()
    {
        this.Hide();
    }
}
