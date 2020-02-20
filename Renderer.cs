using Godot;
using System;

public class Renderer : Camera2D
{
    [Export] public double ShakeMax=6d;
    public double shake;
    
    public override void _Ready()
    {
        shake=0d;
    }

    public override void _Process(float delta)
    {
        if(shake!=0d) applyShake();
    }

    private void applyShake()
    {
        shake=Math.Min(shake,ShakeMax);
        if(shake>=0.3d)
        {
            Vector2 offset=new Vector2(0,0);
            offset.x=(float)MathUtils.randomRange(-shake,shake);
            offset.y=(float)MathUtils.randomRange(-shake,shake);
            Position=offset;
            shake*=0.9d;
        } 
        else if(shake>0d)
        {
            shake=0d;
            Position=new Vector2(0f,0f);
        }
    }
}
