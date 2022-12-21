using Godot;
using System;

public class Buttons : Node2D
{
    public XButton jump,x,o,c;
    private Vector2 oPosition;
    public override void _Ready()
    {
        jump=(XButton)GetNode("A");
        x=(XButton)GetNode("X");
        o=(XButton)GetNode("O");
        c=(XButton)GetNode("0");
        SetPhysicsProcess(false);
        SetProcessInput(false);
        oPosition=Position;
    }

    public override void _Process(float delta)
    {
        Vector2 camCenter=PlayerCamera.instance.GetCameraScreenCenter();
        Vector2 camZoom=PlayerCamera.instance.Zoom;

        Scale=2*camZoom;
        
        Vector2 position=oPosition*0.5f*camZoom.x;
        Position=position+camCenter;
    }

}
