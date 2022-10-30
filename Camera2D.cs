using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
    private float rotation;
    private float rot=0.001f;
    public int direction;
    public override void _Ready()
    {
        rotation=Rotation;
        direction=0;
    }

    public override void _Process(float delta)
    {
        if(direction!=0)
        {
            if((direction==-1&&Rotation>-0.02f)||(direction==1&&Rotation<0.02f))
            {
                Rotation+=rot*Mathf.Sign(direction);
            }
        }
        else if(Rotation!=0)
        {
            Rotation-=rot*Mathf.Sign(Rotation);
        }
    }

}
