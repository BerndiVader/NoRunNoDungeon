using Godot;
using System;

public class World : Node
{
     
    public Level level;
    public Player player;
    public WorldUtils worldUtils;
    public override void _Ready()
    {
        worldUtils=new WorldUtils(this);

        PackedScene pLevel=(PackedScene)ResourceLoader.Load("res://level/Level.tscn");
        PackedScene pPlayer=(PackedScene)ResourceLoader.Load("res://Player.tscn");
        level=(Level)pLevel.Instance();
        player=(Player)pPlayer.Instance();

        OS.SetWindowSize(new Vector2(1024,576));
        
        level.startingPoint=(Position2D)level.GetNode("StartingPoint");
        player.SetPosition(level.startingPoint.Position);

        AddChild(level);
        AddChild(player);

    }

    public override void _Process(float delta)
    {
        if(Input.IsKeyPressed((int)KeyList.Escape)) {
            GetTree().Quit();
        }

    }

    public void restart() {
        player.SetPosition(level.startingPoint.Position);
        level.resetView();
    }
}
