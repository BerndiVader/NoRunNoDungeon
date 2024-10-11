using Godot;
using System;

public class Renderer : CanvasModulate
{
    [Export] private double ShakeMax=6d;
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
            Position=new Vector2((float)MathUtils.randomRange(-shake,shake),(float)MathUtils.randomRange(-shake,shake));
            shake*=0.9d;
        } 
        else if(shake>0d)
        {
            shake=0d;
            Position=Vector2.Zero;
        }
    }
}
