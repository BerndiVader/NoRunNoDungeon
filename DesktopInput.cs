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
    public override bool getJump()
    {
        return Input.IsKeyPressed((int)KeyList.Up);
    }
}
