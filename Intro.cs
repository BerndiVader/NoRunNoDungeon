using Godot;
using System;

public class Intro : Node
{
    static InputController input;
    
    public override void _Ready()
    {
        input=ResourceUtils.getInputController(this);
        GetTree().CurrentScene=this;
        GetTree().Paused=false;
    }

    public override void _Process(float delta)
    {
        if(input.getJump())
        {
            WorldUtils.changeScene(ResourceUtils.world);
        }
    }

    public override void _Notification(int what)
    {
        if(what==MainLoop.NotificationWmQuitRequest)
        {
            ResourceUtils.worker.stop=true;
            ResourceUtils.worker.WaitToFinish();
        }
        base._Notification(what);
    }

}
