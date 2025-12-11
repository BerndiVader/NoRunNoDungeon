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
        if(shake!=0d) ApplyShake();
    }

    private void ApplyShake()
    {
        shake=Math.Min(shake,ShakeMax);
        if(shake>=0.3d)
        {
            Position=new Vector2((float)MathUtils.RandomRange(-shake,shake),(float)MathUtils.RandomRange(-shake,shake));
            shake*=0.9d;
        } 
        else if(shake>0d)
        {
            shake=0d;
            Position=Vector2.Zero;
        }
    }
}
