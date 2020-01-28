using Godot;
using System;

public class Level : TileMap
{
    [Export] public float xSpeed=200f;

    public World world;
    public Position2D startingPoint;

    public override void _Ready()
    {

    }

    public override void _Process(float delta) {
        Vector2 position=this.GetPosition();
        if(Mathf.Abs(position.x)>=1936-512) {
            float x=position.x+1936-512;
            SetPosition(new Vector2(x,position.y));
        }

        MoveLocalX(-xSpeed*delta,false);
    }

}
