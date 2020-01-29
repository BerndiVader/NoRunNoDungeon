using Godot;
using System;

public class Level : TileMap
{
    [Export] public float xSpeed=70f;
    public int mapLength;
    public int pixelLength;

    public Position2D startingPoint;

    public override void _Ready()
    {
        mapLength=((int)GetUsedRect().End.x)-1;
        pixelLength=mapLength*(int)this.CellSize.x;
    }

    public override void _Process(float delta) {

    }

}
