using Godot;
using System;

public class DesktopInput : InputController
{

    public override bool getLeft()
    {
        return Input.IsKeyPressed((int)KeyList.A);
    }
    public override bool getRight()
    {
        return Input.IsKeyPressed((int)KeyList.D);
    }
    public override bool getUp()
    {
        return Input.IsKeyPressed((int)KeyList.W);
    }
    public override bool getDown()
    {
        return Input.IsKeyPressed((int)KeyList.S);
    }
    public override bool getJump()
    {
        return Input.IsKeyPressed((int)KeyList.Up);
    }
    public override void _free()
    {
        return;
    }
    public override bool getPause()
    {
        return Input.IsActionJustPressed("ui_pause");
    }

    public override bool getQuit()
    {
        return Input.IsKeyPressed((int)KeyList.Escape);
    }
}
