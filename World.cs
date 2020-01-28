using Godot;
using System;

public class World : Node
{
     
    public Level level;
    public Player player;
    public WorldUtils worldUtils;
    public PackedScene pLevel;
    public override void _Ready()
    {
        worldUtils=new WorldUtils(this);

        pLevel=(PackedScene)ResourceLoader.Load("res://level/Level.tscn");
        PackedScene pPlayer=(PackedScene)ResourceLoader.Load("res://Player.tscn");
        level=(Level)pLevel.Instance(); 
        player=(Player)pPlayer.Instance();

        OS.WindowSize=new Vector2(1024,576);
        
        level.startingPoint=(Position2D)level.GetNode("StartingPoint");
        player.Position=level.startingPoint.Position;

        AddChild(level);
        AddChild(player);

    }

    public override void _Process(float delta)
    {

        GD.Print(GetChildCount());

        if(Input.IsKeyPressed((int)KeyList.Escape)) {
            GetTree().Quit();
        }

        Vector2 position=level.Position;
        if(Mathf.Abs(position.x)>=(156*16)-512) {
            float x=position.x+(156*16)-512;
            resetLevel();
            level.Position=new Vector2(x,position.y);
        }
        level.MoveLocalX(-level.xSpeed*delta,false);        

    }

    public void restart() {
        resetLevel();
        player.Position=level.startingPoint.Position;
    }

    public void resetLevel() {
        Level oldLevel=level;
        level=(Level)pLevel.Instance();        
        level.Position=new Vector2(0,-4);
        level.startingPoint=(Position2D)level.GetNode("StartingPoint");
        AddChild(level);
        oldLevel.QueueFree();
    }


}
