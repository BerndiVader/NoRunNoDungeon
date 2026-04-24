using Godot;
using System;

public class XButton : TouchScreenButton
{
    protected bool pressed;

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        SetPhysicsProcess(false);
        pressed=false;
    }

    public bool JustPressed()
    {
        if(IsPressed())
        {
            if(!pressed)
            {
                return pressed=true;
            }
            return false;
        }
        return pressed=false;
    }

}
