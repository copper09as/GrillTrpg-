using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class WriteBox : Control
{
    [Export] private Godot.Collections.Array<WriteWord> writeWords;
    [Export] private TextureButton FinishBtn;
    [Export] private TextureButton HeartBtn;
    private static string[] letter;
    public int id;
    private static int count;
    private string message = "";
    public bool isFinish = false;
    public bool isOver = false;
    private bool canRollHeart = true;
    public int lvId;
    public override void _Ready()
    {
        base._Ready();
        FinishBtn.Pressed += OnFinishBtnPress;
        HeartBtn.Pressed += OnHeartBtnPress;
        letter = [.. Enumerable.Repeat("", 5)];
        count = 0;
        for (int i = 2; i < writeWords.Count; i++)
        {
            writeWords[i].Hide();
        }
    }
    private void OnFinishBtnPress()
    {
        if (IsButtonBlocked()) return;
        foreach (var i in writeWords)
        {
            i.IsFillInk = true;
            message += i.wordEdit.Text + " ";
        }
        RollPenmanship();
        count += 1;
        GD.Print(message);
        letter[id] = message;
        isOver = true;
        if (count >= 5)
        {
            string send = "";
            for (int i = 0; i < letter.Length; i++)
            {
                send += letter[i];
            }
            GameManager.Instance.SendLetter(send);
        }
        FinishBtn.Hide();
    }
    private void OnHeartBtnPress()
    {
        if (IsButtonBlocked()) return;
        if (!canRollHeart) return;
        GD.Print("开启华丽辞藻鉴定");
        if (RollHeart())
        {
            GrillGameManager.Instance.Score += lvId == 1 ? 1 : -1;
        }
        for (int i = 2; i < writeWords.Count; i++)
        {
            writeWords[i].Show();
        }
        canRollHeart = false;
        HeartBtn.Hide();
    }
    private void RollPenmanship()
    {
        for (int j = 0; j < GameDataCenter.Instance.CurrentPlayerData.Penmanship + 1; j++)
        {
            int value = CaculateTool.Roll(CaculateTool.D16);
            GD.Print("书法鉴定中");
            if (value >= 5)
            {
                GD.Print("书法鉴定成功");
                GrillGameManager.Instance.Score += 1;
                break;
            }
        }
    }
    private bool RollHeart()
    {
        for (int j = 0; j < GameDataCenter.Instance.CurrentPlayerData.Heart + 1; j++)
        {
            int value = CaculateTool.Roll(CaculateTool.D16);
            GD.Print("心灵鉴定中");
            if (value >= 5)
            {
                GD.Print("心灵鉴定成功");
                return true;
            }
        }
        return false;
    }
    private bool IsButtonBlocked() => !isFinish || isOver;

}
