using Godot;
using System;

public class MovingPlatform0 : StaticBody2D
{

    [Export] public Vector2 Direction=new Vector2(0f,-1f);
    [Export] public float Speed=20f;
    [Export] public float MaxSpeed=20f;
    [Export] public int Length=10;
    [Export] public float LerpFactor=4f;
    [Export] public bool Linear=true;
    [Export] public float Shrink=0f;

    int maxLength;
    int currentLength;
    Vector2 dir,startPos;
    float currentShrink;
    Tween tween;

    CollisionShape2D collisionController;

    public override void _Ready()
    {
        dir=Direction.Normalized();
        startPos=new Vector2(Position);
        maxLength=Length*16;
        currentLength=maxLength/2;
        startPos-=dir*currentLength;
        currentShrink=0f;
        tween=(Tween)GetNode("Tween");

        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
    }

    public override void _PhysicsProcess(float delta) {

        if(!Linear){
            if(Mathf.Abs(Mathf.Abs((startPos+dir*maxLength).Length())-Position.Length())<=LerpFactor) {
                dir*=-1;
            }
            Position=Position.LinearInterpolate(startPos+dir*maxLength,delta*Speed);
        } else {
            if(currentLength>=maxLength) {
                currentLength=0;
                dir*=-1;
            }
            Position+=dir*(Speed*delta);
            currentLength++;
        }
        Position+=new Vector2(0,currentShrink*delta);
    }

    public void _on_VisibilityNotifier2D_screen_entered() {
        SetProcess(true);
        SetPhysicsProcess(true);
    }

    public void _on_VisibilityNotifier2D_screen_exited() {
        QueueFree();
    }

}
