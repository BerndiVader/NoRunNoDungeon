using Godot;
using System;
using System.Threading;

public class World : Node
{
     
    [Export] public Vector2 RESOLUTION=new Vector2(512f,288f);
    public int stage;
    public Level level,newLevel,cachedLevel,oldLevel;
    public TileSet tileSet;
    public Background background;
    public Player player;
    public Renderer renderer;

    public InputController input;

    int currentLevel,nextLevel;

    public Gamestate state;
    public override void _Ready()
    {
        WorldUtils.world=this;
        stage=0;
        renderer=(Renderer)GetNode("Renderer");
        ResourceUtils.Init();

        tileSet=(TileSet)ResourceUtils.tilesets[0];
        currentLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
        level=(Level)ResourceUtils.levels[currentLevel].Instance();
        cacheLevel((int)MathUtils.randomRange(0,ResourceUtils.levels.Count));
        WorldUtils.mergeMaps(level,cachedLevel);
        player=(Player)ResourceUtils.player.Instance();

        input=ResourceUtils.getInputController();

        OS.WindowSize=new Vector2(1024,576);

        background=(Background)ResourceUtils.background.Instance();
        state=Gamestate.RUNNING;

        renderer.AddChild(level);
        renderer.AddChild(player);
        renderer.AddChild(background);

    }


    public override void _Process(float delta)
    {
        switch(state) 
        {
            case Gamestate.RESTART:
            {
                break;
            }
            case Gamestate.SCENE_CHANGED:
            {
                renderer.RemoveChild(level);
                oldLevel=level;
                level=newLevel;
                newLevel=null;
                level.Position=new Vector2(-(Mathf.Abs(oldLevel.Position.x)-(oldLevel.pixelLength-512)),0);
                oldLevel.freeLevel();
                state=Gamestate.RUNNING;
                step(delta);
                break;
            }
            default:
            {
                step(delta);
                break;
            }
            
        }

    }

    void step(float delta) 
    {
        if(Input.IsKeyPressed((int)KeyList.Escape)) WorldUtils.quit();

        level.MoveLocalX((level.direction.x*level.Speed)*delta,true); 
        level.MoveLocalY((level.direction.y*level.Speed)*delta,true); 

        Vector2 position=level.Position;

        if((state==Gamestate.RUNNING)&&Mathf.Abs(position.x)>=(level.pixelLength)-528) 
        {
            state=Gamestate.SCENE_CHANGE;
            stage++;
            if(stage>=ResourceUtils.levels.Count) stage=0;
            System.Threading.Thread thread=new System.Threading.Thread(()=>prepareLevel());
            thread.Start();
        }
    }

    public void restartGame(bool lvl=false)
    {
        state=Gamestate.RESTART;
        renderer.RemoveChild(level);
        level.CallDeferred("queue_free");
        if(!lvl) currentLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
        level=(Level)ResourceUtils.levels[currentLevel].Instance();
        cacheLevel((int)MathUtils.randomRange(0,ResourceUtils.levels.Count));
        WorldUtils.mergeMaps(level,cachedLevel);
        renderer.AddChild(level);
        renderer.RemoveChild(player);
        player.CallDeferred("queue_free");
        player=(Player)ResourceUtils.player.Instance();
        renderer.AddChild(player);
        state=Gamestate.RUNNING;
    }

    void prepareLevel() 
    {
        if(cachedLevel==null) cacheLevel((int)MathUtils.randomRange(0,ResourceUtils.levels.Count));
        newLevel=(Level)cachedLevel.Duplicate();
        currentLevel=nextLevel;
        cacheLevel((int)MathUtils.randomRange(0,ResourceUtils.levels.Count));
        WorldUtils.mergeMaps(newLevel,cachedLevel);
        newLevel.Position=new Vector2(-(Mathf.Abs(level.Position.x)-(level.pixelLength-512)),0);
        renderer.AddChild(newLevel);
        newLevel.Position=new Vector2(-(Mathf.Abs(level.Position.x)-(level.pixelLength-512)),0);
        state=Gamestate.SCENE_CHANGED;
    }

    void cacheLevel(int nextStage) 
    {
        nextLevel=nextStage;
        if(cachedLevel!=null&&!cachedLevel.IsQueuedForDeletion()) cachedLevel.CallDeferred("freeLevel");
        cachedLevel=(Level)ResourceUtils.levels[nextStage].Instance();
    }

}
