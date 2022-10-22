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
		Node currentScene=root.GetTree().CurrentScene;
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
				currentScene.QueueFree();
			}
		}
		Worker.instance.gc();
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
	public Level level;
	private Level cachedLevel;
	public TileSet tileSet;
	private Background background;
	public Player player;
	public Renderer renderer;
	public InputController input;
	private int currentLevel,nextLevel;
	public Gamestate state,oldState;

	public override void _Ready()
	{
		if(ResourceUtils.isMobile)
		{
			GetNode("WorldEnvironment").QueueFree();
		}

		input=ResourceUtils.getInputController(this);
		renderer=GetNode<Renderer>("Renderer");

		tileSet=(TileSet)ResourceUtils.tilesets[(int)MathUtils.randomRange(0,ResourceUtils.tilesets.Count)];
		currentLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		nextLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		cachedLevel=(Level)ResourceUtils.levels[nextLevel].Instance();
		mergeMaps(level,cachedLevel);
		player=(Player)ResourceUtils.player.Instance();
		Player.LIVES=3;
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
			case Gamestate.SCENE_CHANGE:
			{
				if(level!=null&&level.IsInsideTree())
				{
					tick(delta);
				}
				break;
			}
			case Gamestate.SCENE_CHANGED:
			{
				if(level.IsInsideTree())
				{
					state=Gamestate.RUNNING;				
					tick(delta);
				}
				break;
			}
			case Gamestate.RUNNING:
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
					CallDeferred(nameof(restartGame),false);
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

	private void tick(float delta) 
	{
		level.MoveLocalX((level.direction.x*level.Speed)*delta,true);
		level.MoveLocalY((level.direction.y*level.Speed)*delta,true);

		if((state==Gamestate.RUNNING)&&Mathf.Abs(level.Position.x)>=(level.pixelLength)-528)
		{
			state=Gamestate.SCENE_CHANGE;
			Worker.status=Worker.Status.PREPARELEVEL;
		}
	}

	public void restartGame(bool keepLevel=false)
	{
		Worker.instance.gc();
		state=Gamestate.RESTART;
		renderer.RemoveChild(level);
		if(!keepLevel)
		{
			currentLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		}
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		mergeMaps(level,cachedLevel);
		renderer.AddChild(level);
		renderer.RemoveChild(player);
		player.QueueFree();
		player=(Player)ResourceUtils.player.Instance();
		renderer.AddChild(player);
		state=Gamestate.RUNNING;
	}

	public void prepareLevel()
	{
		Level newLevel;
		currentLevel=nextLevel;
		nextLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		if(cachedLevel!=null)
		{
			newLevel=cachedLevel;
		}
		else
		{
			newLevel=(Level)ResourceUtils.levels[currentLevel].Instance();
		}
		cachedLevel=(Level)ResourceUtils.levels[nextLevel].Instance();
		mergeMaps(newLevel,cachedLevel);
		renderer.CallDeferred("add_child",newLevel);
		newLevel.Position=new Vector2(-(Mathf.Abs(level.Position.x)-(level.pixelLength-512)),0);
		while(!newLevel.IsInsideTree())
		{
			OS.DelayMsec(1);
		}
		newLevel.Position=new Vector2(-(Mathf.Abs(level.Position.x)-(level.pixelLength-512)),0);
		renderer.CallDeferred("remove_child",level);
		level=newLevel;
		newLevel=null;
		state=Gamestate.SCENE_CHANGED;
	}

	public void _Free()
	{
		state=Gamestate.RESTART;
		CallDeferred("queue_free");
		if(cachedLevel!=null)
		{
			cachedLevel.freeLevel();
		}
		input._free();
		World.instance=null;
	}
}
