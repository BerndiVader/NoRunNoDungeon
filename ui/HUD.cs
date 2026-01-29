using Godot;
using System;
using System.Collections.Generic;

public class HUD : Control
{
    private static readonly ImageTexture live_full=ResourceLoader.Load<ImageTexture>("res://image/0x72/frames/ui_heart_full.png");
    private static readonly ImageTexture live_empty=ResourceLoader.Load<ImageTexture>("res://image/0x72/frames/ui_heart_empty.png");
    private static readonly List<WeakReference>lives=new List<WeakReference>();

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        SetProcess(false);
        SetProcessInput(false);
    }

    public void UpdateLives()
    {
    }



}
