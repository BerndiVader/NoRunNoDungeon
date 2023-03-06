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
        return Input.IsActionJustPressed("ui_up");
    }
    public override bool getAttack()
    {
        return Input.IsActionJustPressed("ui_right");
    }
    public override bool getChange()
    {
        return Input.IsActionJustPressed("ui_down");
    }
    public override bool getPause()
    {
        return Input.IsActionJustPressed("ui_pause");
    }
    public override bool getQuit()
    {
        return Input.IsActionJustPressed("ui_cancel");
    }

    public override bool getJustLeft()
    {
        return Input.IsActionJustPressed("key_a");
    }

    public override bool getJustRight()
    {
        return Input.IsActionJustPressed("key_d");
    }

    public override bool getJustUp()
    {
        return Input.IsActionJustPressed("key_w");
    }

    public override bool getJustDown()
    {
        return Input.IsActionJustPressed("key_s");
    }

    public override bool getJustAccept()
    {
        return Input.IsActionJustPressed("ui_accept");
    }

    public override void _free()
    {
        return;
    }

}
