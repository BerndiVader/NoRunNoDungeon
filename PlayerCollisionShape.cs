using Godot;
using System;

public class PlayerCollisionShape : CollisionShape2D
{

    Player player;

    public override void _Ready()
    {
        player=(Player)GetParent();
    }
}
