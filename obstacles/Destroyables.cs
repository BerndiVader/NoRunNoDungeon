using Godot;
using System;

public class Destroyables : Area2D
{
    public override void _Ready()
    {
        Connect("body_entered",this,nameof(onHit));
    }

    private void onHit(Node node)
    {
        Vector2 local=World.level.ToLocal(GlobalPosition);
        Vector2 tile=World.level.WorldToMap(local);
        World.level.SetCellv(tile,-1);
    }

}
