using Godot;
using System;

public class Camera2D : Godot.Camera2D
{
    [Export] public double ShakeMax=6d;
    public double shake;

    public override void _Ready()
    {
        shake=0d;
    }

    public override void _Process(float delta)
    {
        applyShake();
    }

    private void applyShake(){
        shake=Math.Min(shake,ShakeMax);
        if(shake>=0.3d){
            Vector2 offset=Offset;
            offset.x=(float)MathUtils.randomRange(-shake,shake);
            offset.y=(float)MathUtils.randomRange(-shake,shake);
            Offset=offset;
            shake*=0.9d;
        } else if(shake>0d){
            shake=0d;
            Offset=new Vector2(0f,0f);
        }
    }
}
