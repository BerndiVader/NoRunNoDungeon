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

}
