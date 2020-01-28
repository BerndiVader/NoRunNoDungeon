using Godot;
using System;

public class Level : TileMap
{
    [Export] public float xSpeed=200f;

    public Position2D startingPoint;

    public override void _Ready()
    {
        WorldUtils.setCurrentLevel(this);
    }

    public override void _Process(float delta) {

    }

}
