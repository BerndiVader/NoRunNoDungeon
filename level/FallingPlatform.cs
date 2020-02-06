using Godot;
using System;

public class FallingPlatform : StaticBody2D
{

    bool falling=false;
    [Export] public int TimeSpan=20;
    int time;

    float shake=2f,shakeMax=6f,speed=0f;
    Vector2 startPosition;

    CollisionShape2D collisionController;

    public override void _Ready()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessInput(false);
        startPosition=Position;
        time=TimeSpan;
        collisionController=(CollisionShape2D)GetNode("CollisionShape2D");
    }

    public override void _PhysicsProcess(float delta) {
        if(falling){
            if(time<=0) {
                speed=Mathf.Min(speed+=10f,200f);
                MoveLocalY(speed*delta,false);
            } else {
                applyShake();
            }
            time--;
        }
        
    }

    public void _on_VisibilityNotifier2D_screen_entered() {
        SetProcess(true);
        SetPhysicsProcess(true);
    }

    public void _on_VisibilityNotifier2D_screen_exited() {
        QueueFree();
    }

    public void _on_Area2D_body_entered(Node body) {
        if(!body.IsInGroup("Players")||falling) return;
        Player player=(Player)body;
        falling=true;
    }

    public void _on_Area2D_body_exited(Node body) {
        if(!body.IsInGroup("Players")) return;
        Player player=(Player)body;
    }

    void applyShake() {
        shake=Math.Min(shake,shakeMax);
        Vector2 offset=new Vector2(0,0);
        offset.x=(float)MathUtils.randomRange(-shake,shake);
        offset.y=(float)MathUtils.randomRange(-shake,shake);
        Position=startPosition+offset;
        shake*=0.9f;
    }
}
