using Godot;
using System;
using System.Threading;

public class World : Node
{
     
    public int stage;
    public Level level;
    public TileSet tileSet;
    public Background background;
    public Level cachedLevel;
    public Player player;
    public Renderer renderer;
    bool change;
    public override void _Ready()
    {
        WorldUtils.world=this;
        stage=0;
        renderer=(Renderer)GetNode("Renderer");
        ResourceUtils.Init();

        tileSet=(TileSet)ResourceUtils.tilesets[0];
        level=(Level)ResourceUtils.levels[(int)MathUtils.randomRange(0,3)].Instance();
        cacheLevel((int)MathUtils.randomRange(0,3));
        WorldUtils.mergeMaps(level,cachedLevel);
        player=(Player)ResourceUtils.player.Instance();

        OS.WindowSize=new Vector2(1024,576);

        background=(Background)ResourceUtils.background.Instance();

        change=false;
        renderer.AddChild(level);
        renderer.AddChild(player);
        renderer.AddChild(background);

    }


    public override void _Process(float delta)
    {
        if(Input.IsKeyPressed((int)KeyList.Escape)) {
            WorldUtils.quit();
        }

        if(level==null) return;

        level.MoveLocalX(-level.xSpeed*delta,false); 
        Vector2 position=level.Position;
        if(!change&&Mathf.Abs(position.x)>=(level.pixelLength)-512) {
            change=true;
            stage++;
            if(stage>=ResourceUtils.levels.Count) stage=0;
            System.Threading.Thread thread=new System.Threading.Thread(()=>startLevel());
            thread.Start();
        }

    }

    public void restartGame() {
        stage=0;
        cachedLevel.Free();
        cachedLevel=(Level)ResourceUtils.levels[(int)MathUtils.randomRange(0,3)].Instance();
        startLevel(0f,true);
        player.Position=level.startingPoint.Position;
    }

    public void startLevel(float restX=0f,bool useX=false) {
        Level newLevel=(Level)cachedLevel.Duplicate();
        Level oldLevel=level;
        newLevel.Visible=false;
        cacheLevel((int)MathUtils.randomRange(0,3));
        WorldUtils.mergeMaps(newLevel,cachedLevel);
        renderer.AddChild(newLevel);
        newLevel.Position=new Vector2(useX?restX:-(Mathf.Abs(level.Position.x)-(level.pixelLength-512)),0);
        newLevel.Visible=true;
        level.SetProcess(false);
        level.Visible=false;
        newLevel.Position=new Vector2(useX?restX:-(Mathf.Abs(level.Position.x)-(level.pixelLength-512)),0);
        renderer.RemoveChild(oldLevel);
        level=newLevel;
        oldLevel.Free();
        change=false;
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
