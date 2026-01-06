using Godot;
using System;

public class XButton : TouchScreenButton
{
    bool jPressed;

    public override void _Ready()
    {
        SetProcess(false);
        SetProcessInput(false);
        SetPhysicsProcess(false);
        jPressed=false;
    }

    public bool JustPressed()
    {
        if(IsPressed())
        {
            if(!jPressed)
            {
                jPressed=true;
                return jPressed;
            }
            return false;
        }
        jPressed=false;
        return false;
    }

}
