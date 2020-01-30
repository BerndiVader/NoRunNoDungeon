using Godot;
using System;

public class World : Node
{
     
    int stage;
    Level level;
    Level cachedLevel;
    Player player;
    public CanvasModulate renderer;
    public override void _Ready()
    {
        WorldUtils.setworld(this);
        stage=0;
        renderer=(CanvasModulate)GetNode("Renderer");
        ResourceUtils.Init();

        level=(Level)ResourceUtils.levels[stage].Instance();
        cacheLevel(stage+1);
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

        level.MoveLocalX(-level.xSpeed*delta,false); 
        Vector2 position=level.Position;
        if(Mathf.Abs(position.x)>=(level.pixelLength)-512) {
            stage++;
            if(stage>=ResourceUtils.levels.Count) stage=0;
            float restX=Mathf.Abs(position.x)-(level.pixelLength-512);
            startLevel(-restX);
        }

    }

    public void restartGame() {
        stage=0;
        cachedLevel=(Level)ResourceUtils.levels[stage].Instance();
        startLevel();
        player.Position=level.startingPoint.Position;
    }

    public void startLevel(float restX=0f) {
        Level oldLevel=level;
        level=cachedLevel;
        this.cacheLevel(stage+1);
        level.startingPoint=(Position2D)level.GetNode("StartingPoint");
        renderer.AddChild(level);
        level.Position=new Vector2(restX,0);
        oldLevel.QueueFree();
    }

    public void cacheLevel(int nextStage) {
        if(nextStage>ResourceUtils.levels.Count-1) nextStage=0;
        cachedLevel=(Level)ResourceUtils.levels[nextStage].Instance();
        WorldUtils.mergeMaps(level,cachedLevel);
    }

    public Level getCurrentLevel() {
        return this.level;
    }


}
