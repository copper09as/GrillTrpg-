using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public partial class GameDataCenter : Node
{
    public static GameDataCenter Instance;
    [Export] private GrillOfferData currentOfferData;
    public GrillOfferData CurrentOfferData
    {
        get
        {
            return currentOfferData;
        }
        set
        {
            currentOfferData = value;
        }
    }
    [Export] public Godot.Collections.Array<GrillOfferData> gameOfferData;
    private GrillPlayerData currentPlayerData;
    private GrillPlayerData nullPlayerData;
    public GrillPlayerData CurrentPlayerData
    {
        get
        {
            if (currentPlayerData != null)
                return currentPlayerData;
            else if (gamePlayerData.Count > 0)
            {
                return gamePlayerData[0];
            }
            return nullPlayerData;

        }
        set
        {
            if(value != null)
                currentPlayerData = value;
        }

    }
    public List<GrillPlayerData> gamePlayerData;
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
        nullPlayerData = new GrillPlayerData
        {
            Name = "未命名",
            Language = 0,
            Penmanship = 1,
            Heart = 2
        };
        gamePlayerData = new List<GrillPlayerData>();
        UpdatePlayerList();
    }

    public void UpdatePlayerList()
    {
        gamePlayerData.Clear();

        const int MAX_PLAYERS = 3;

        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            string savePath = StringResource.GetPlayerSavePath(i);
            GrillPlayerData playerData = GameDataHandler.Load<GrillPlayerData>(savePath);

            if (playerData != null)
            {
                gamePlayerData.Add(playerData);
            }
        }
    }

}
