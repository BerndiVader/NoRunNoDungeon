using Godot;
using System;

public class XButton : TouchScreenButton
{
    bool jPressed;

    public override void _Ready()
    {
        jPressed=false;
        
    }

    public bool justPressed()
    {
        if(IsPressed())
        {
            if(!jPressed)
            {
                return jPressed=true;
            }
            return false;
        }
        return jPressed=false;
    }

}
