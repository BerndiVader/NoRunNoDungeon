using Godot;
using System;

public class BossTestTree : WalkingTree,IKeepOnScreen
{
    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);
        KeepOnScreen();
    }

    public void KeepOnScreen()
    {
        PlayerCamera.instance.KeepPlayerAndTargetOnScreen(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        if(IsInstanceValid(PlayerCamera.instance))
        {
            PlayerCamera.instance.StopKeepingPlayerAndTargetOnScreen(this);
        }
    }
}
