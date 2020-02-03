using Godot;
using System;

public class World : Node
{
     
    public int stage;
    public Level level;
    public Background background;
    public Level cachedLevel;
    public Player player;
    public Renderer renderer;
    public override void _Ready()
    {
        WorldUtils.world=this;
        stage=0;
        renderer=(Renderer)GetNode("Renderer");
        ResourceUtils.Init();

        level=(Level)ResourceUtils.levels[(int)MathUtils.randomRange(0,3)].Instance();
        cacheLevel((int)MathUtils.randomRange(0,3));
        WorldUtils.mergeMaps(level,cachedLevel);
        player=(Player)ResourceUtils.player.Instance();

        OS.WindowSize=new Vector2(1024,576);

        background=(Background)ResourceUtils.background.Instance();

        renderer.AddChild(level);
        renderer.AddChild(player);
        renderer.AddChild(background);

    }


    public override void _Process(float delta)
    {

        if(Input.IsKeyPressed((int)KeyList.Escape)) {
            WorldUtils.quit();
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
        cachedLevel.Free();
        cachedLevel=(Level)ResourceUtils.levels[(int)MathUtils.randomRange(0,3)].Instance();
        startLevel();
        player.Position=level.startingPoint.Position;
    }

    public void startLevel(float restX=0f) {
        level.SetProcess(false);
        Level oldLevel=level;
        level=(Level)cachedLevel.Duplicate();
        level.Position=new Vector2(restX,0);
        cacheLevel((int)MathUtils.randomRange(0,3));
        WorldUtils.mergeMaps(level,cachedLevel);
        renderer.AddChild(level);
        renderer.RemoveChild(oldLevel);
        oldLevel.Free();
    }

    public void cacheLevel(int nextStage) {
        if(cachedLevel!=null) cachedLevel.Free();
        if(nextStage>ResourceUtils.levels.Count-1) nextStage=0;
        cachedLevel=(Level)ResourceUtils.levels[nextStage].Instance();
    }

    public Level getCurrentLevel() {
        return this.level;
    }


}
