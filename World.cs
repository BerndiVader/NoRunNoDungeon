using Godot;
using System;

public class World : Node
{
     
    int stage;
    Level level;
    Player player;
    public WorldUtils worldUtils;
    public CanvasModulate renderer;
    public override void _Ready()
    {
        stage=0;
        worldUtils=new WorldUtils(this);
        renderer=(CanvasModulate)GetNode("Renderer");
        ResourceUtils.Init();

        level=(Level)ResourceUtils.levels[stage].Instance();
        player=(Player)ResourceUtils.player.Instance();

        OS.WindowSize=new Vector2(1024,576);

        level.startingPoint=(Position2D)level.GetNode("StartingPoint");
        player.Position=level.startingPoint.Position;

        renderer.AddChild(level);
        renderer.AddChild(player);

    }

    public override void _Process(float delta)
    {

        if(Input.IsKeyPressed((int)KeyList.Escape)) {
            GetTree().Quit();
        }

        Vector2 position=level.Position;
        if(Mathf.Abs(position.x)>=(level.pixelLength)-512) {
            stage++;
            if(stage>=ResourceUtils.levels.Count) stage=0;
            startLevel();
        }
        level.MoveLocalX(-level.xSpeed*delta,false); 

    }

    public void restartGame() {
        stage=0;
        startLevel();
        player.Position=level.startingPoint.Position;
    }

    public void startLevel() {
        Level oldLevel=level;
        level=(Level)ResourceUtils.levels[stage].Instance();        
        level.startingPoint=(Position2D)level.GetNode("StartingPoint");
        renderer.AddChild(level);
        oldLevel.QueueFree();
    }

    public Level getCurrentLevel() {
        return this.level;
    }


}
