using Godot;
using Godot.Collections;
using System;

public static class CaculateTool
{
    public static Array<int> D16 = CaculateDice(1, 6);
    private static Array<int> CaculateDice(int times, int type)//2,6
    {
        int cap = times * (type - 1) + 1;//计算可能数，例如1D6为1*5+1=6，2D6=2*5+1=11，3D6=3*5+1=16
        Array<int> ans = new Array<int>();
        for (int i = 0; i < cap; i++)
        {
            ans.Add(i + times);
        }
        return ans;
    }
    private static Array<int> CaculateDice(int times, int type, int ex)
    {
        int cap = times * (type - 1) + 1;
        Array<int> ans = new Array<int>();
        for (int i = 0; i < cap; i++)
        {
            ans.Add(i + times + ex);
        }
        return ans;
    }
    public static int Roll(Array<int> dice)
    {
        return dice.PickRandom();
    }
}
