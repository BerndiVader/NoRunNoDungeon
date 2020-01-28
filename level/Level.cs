using Godot;
using System;

public class Level : TileMap
{
    [Export] public float xSpeed=200f;

    public Position2D startingPoint;

    public override void _Ready()
    {
        resetView();
        WorldUtils.setCurrentLevel(this);
    }

    public override void _Process(float delta) {
        Vector2 position=this.GetPosition();
        if(Mathf.Abs(position.x)>=(156*16)-512) {
            float x=position.x+(156*16)-512;
            SetPosition(new Vector2(x,position.y));
        }

        MoveLocalX(-xSpeed*delta,false);
    }

    public void resetView() {
        SetPosition(new Vector2(0,-4));
    }

}
