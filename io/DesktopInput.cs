using Godot;
using System;

public class DesktopInput : InputController
{

    public override bool Left()
    {
        return Input.IsKeyPressed((int)KeyList.A);
    }
    public override bool Right()
    {
        return Input.IsKeyPressed((int)KeyList.D);
    }
    public override bool Up()
    {
        return Input.IsKeyPressed((int)KeyList.W);
    }
    public override bool Down()
    {
        return Input.IsKeyPressed((int)KeyList.S);
    }
    public override bool Jump()
    {
        return Input.IsActionPressed("ui_up");
    }
    public override bool JustJump()
    {
        return Input.IsActionJustPressed("ui_up");
    }
    public override bool Attack()
    {
        return Input.IsActionPressed("ui_right");
    }
    public override bool JustAttack()
    {
        return Input.IsActionJustPressed("ui_right");
    }
    public override bool Change()
    {
        return Input.IsActionPressed("ui_down");
    }
    public override bool JustChange()
    {
        return Input.IsActionJustPressed("ui_down");
    }
    public override bool Pause()
    {
        return Input.IsActionJustPressed("ui_pause");
    }
    public override bool Quit()
    {
        return Input.IsActionJustPressed("ui_cancel");
    }

    public override bool JustLeft()
    {
        return Input.IsActionJustPressed("key_a");
    }

    public override bool JustRight()
    {
        return Input.IsActionJustPressed("key_d");
    }

    public override bool JustUp()
    {
        return Input.IsActionJustPressed("key_w");
    }

    public override bool JustDown()
    {
        return Input.IsActionJustPressed("key_s");
    }

    public override bool JustAccept()
    {
        return Input.IsActionJustPressed("ui_accept");
    }

    public override void Free()
    {
        return;
    }

}
