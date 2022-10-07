using Godot;
using System;

public class World : Node
{
	public static World instance;
	public static Viewport root;

	public static void Init(Viewport viewPort)
	{
		root=viewPort;
	}	

	public static void mergeMaps(Level newLevel, Level nextLevel) 
	{
		int x=33,y=18;
		int lx=((int)newLevel.GetUsedRect().End.x);

		for(int xx=0;xx<x;xx++) 
		{
			for(int yy=0;yy<y;yy++)
			{
				Vector2 autoTile=nextLevel.GetCellAutotileCoord(xx,yy);
				newLevel.SetCell(lx+xx,yy,nextLevel.GetCell(xx,yy),false,false,false,autoTile);
			}
		}
	}

	public static void changeScene(PackedScene newScene)
	{
			var currentScene=root.GetTree().CurrentScene;
			root.AddChild(newScene.Instance());
			root.RemoveChild(currentScene);
			if(currentScene.GetType().Name.Equals("World"))
			{
				((World)currentScene)._Free();
			}
			else
			{
				if(!currentScene.IsQueuedForDeletion())
				{
					currentScene.CallDeferred("queue_free");
				}
			}
	}

	public static void quit() 
	{
		Console.Write("Wait for worker to finish...");
		Worker.stop=true;
		Worker.instance.WaitToFinish();
		while(Worker.instance.IsActive())
		{
			Console.Write(".");
			OS.DelayMsec(1);
		}
		Console.WriteLine(" done!");

		if(instance!=null)
		{
			instance._Free();
		}

		root.GetTree().Quit();
	}

	 
	public Vector2 RESOLUTION=new Vector2(512f,288f);
	public int stage;
	public Level level,newLevel,cachedLevel;
	public TileSet tileSet;
	public Background background;
	public Player player;
	public Renderer renderer;
	public InputController input;

	int currentLevel,nextLevel;

	public Gamestate state, oldState;
	public override void _Ready()
	{
		if(ResourceUtils.isMobile)
		{
			GetNode("WorldEnvironment").QueueFree();
		}

		input=ResourceUtils.getInputController(this);

		stage=0;
		renderer=GetNode<Renderer>("Renderer");

		tileSet=(TileSet)ResourceUtils.tilesets[(int)MathUtils.randomRange(0,ResourceUtils.tilesets.Count)];
		currentLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		cacheLevel((int)MathUtils.randomRange(0,ResourceUtils.levels.Count));
		mergeMaps(level,cachedLevel);
		player=(Player)ResourceUtils.player.Instance();
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
				Level oldLevel=level;
				level=newLevel;
				newLevel=null;
				level.Position=new Vector2(-(Mathf.Abs(oldLevel.Position.x)-(oldLevel.pixelLength-512)),0);
				oldLevel.freeLevel();
				state=Gamestate.RUNNING;
				tick(delta);
				break;
			}
			default:
			{
				if(input.getPause())
				{
					GetTree().Paused^=true;
					if(GetTree().Paused)
					{
						oldState=state;
						state=Gamestate.PAUSED;
						Pause pause=(Pause)ResourceUtils.pause.Instance();
						pause.PauseMode=PauseModeEnum.Process;
						renderer.AddChild(pause);
					}
				}
				else if(input.getQuit())
				{
					quit();
					return;
				}
				
				if(state!=Gamestate.PAUSED)
				{
					tick(delta);
				}
				break;
			}            
		}

	}

	public override void _Notification(int what)
	{
		if(what==MainLoop.NotificationWmQuitRequest)
		{
			quit();
		}
		base._Notification(what);
	}

	public override void _EnterTree()
	{
		instance=this;
		GetTree().CurrentScene=this;
	}

	void tick(float delta) 
	{
		level.MoveLocalX((level.direction.x*level.Speed)*delta,true);
		level.MoveLocalY((level.direction.y*level.Speed)*delta,true);

		Vector2 position=level.Position;

		if((state==Gamestate.RUNNING)&&Mathf.Abs(position.x)>=(level.pixelLength)-528)
		{
			state=Gamestate.SCENE_CHANGE;
			stage++;
			if(stage>=ResourceUtils.levels.Count) stage=0;
				Worker.prepareLevel=true;
		}
	}

	public void restartGame(bool lvl=false)
	{
		state=Gamestate.RESTART;
		renderer.RemoveChild(level);
		level.Free();
		if(!lvl) currentLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		cacheLevel((int)MathUtils.randomRange(0,ResourceUtils.levels.Count));
		mergeMaps(level,cachedLevel);
		renderer.AddChild(level);
		renderer.RemoveChild(player);
		player.CallDeferred("queue_free");
		player=(Player)ResourceUtils.player.Instance();
		renderer.AddChild(player);
		state=Gamestate.RUNNING;
	}

	public void prepareLevel()
	{
		if(cachedLevel==null) cacheLevel((int)MathUtils.randomRange(0,ResourceUtils.levels.Count));
		newLevel=(Level)cachedLevel.Duplicate();
		currentLevel=nextLevel;
		cacheLevel((int)MathUtils.randomRange(0,ResourceUtils.levels.Count));
		mergeMaps(newLevel,cachedLevel);
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

	public void _Free()
	{
		state=Gamestate.RESTART;
		if(cachedLevel!=null)
		{
			cachedLevel.CallDeferred("freeLevel");
			if(newLevel!=null)
			{
				newLevel.CallDeferred("freeLevel");
			}
		}
		CallDeferred("queue_free");
		World.instance=null;
	}
}
