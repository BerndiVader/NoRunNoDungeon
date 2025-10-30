using Godot;
using System;

public class World : Node
{
	public static World instance;
	public static Viewport root;
	public static Vector2 RESOLUTION=new Vector2(512f,288f);

	public static void Init(Viewport viewPort)
	{
		root=viewPort;
	}	

	public static void mergeMaps(Level newLevel, Level nextLevel) 
	{
		int lx=(int)newLevel.GetUsedRect().End.x;

		for(int x=0;x<33;x++) 
		{
			for(int y=0;y<18;y++)
			{
				int cellValue=nextLevel.GetCell(x,y);
				Vector2 tileCoord=nextLevel.GetCellAutotileCoord(x,y);
				newLevel.SetCell(lx+x,y,cellValue,false,false,false,tileCoord);
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
		Worker.gc();
	}

	public static void onObjectExitedScreen(Node node)
	{
        if(PlayerCamera.instance.Zoom.x==1f)
        {
            node.QueueFree();
        }
	}

	public static void quit() 
	{
		Worker.stop();
		if(instance!=null)
		{
			instance._Free();
		}

		root.GetTree().Quit();
	}
	 
	public static Level level;
	private static Level cachedLevel;
	public TileSet tileSet;
	private Background background;
	public Renderer renderer;
	public InputController input;
	private int currentLevel,nextLevel;
	private static Gamestate lastState;
	public static Gamestate state;

	private delegate void Goal(float delta);
	private Goal goal;

	private AudioStreamPlayer2D musicPlayer;

	public override void _Ready()
	{
		musicPlayer=new AudioStreamPlayer2D();
		musicPlayer.Bus="Background";
		onMusicFinishedPlaying();
		musicPlayer.Position=new Vector2(RESOLUTION.x*0.5f,RESOLUTION.y*0.5f);
		musicPlayer.Connect("finished",this,nameof(onMusicFinishedPlaying));
		AddChild(musicPlayer);
		musicPlayer.Play();

		ResourceUtils.camera.Instance<PlayerCamera>();
		if(GameSettings.current.usage==Viewport.UsageEnum.Usage2d)
		{
			GetNode("WorldEnvironment").QueueFree();
		}
		input=ResourceUtils.getInputController(this);
		renderer=GetNode<Renderer>("Renderer");

		tileSet=ResourceUtils.tilesets[(int)MathUtils.randomRange(0,ResourceUtils.tilesets.Count)];
		currentLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		nextLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		cachedLevel=(Level)ResourceUtils.levels[nextLevel].Instance();
		mergeMaps(level,cachedLevel);
		ResourceUtils.player.Instance();
		Player.LIVES=3;
		background=(Background)ResourceUtils.background.Instance();

		Player.instance.AddChild(PlayerCamera.instance);
		renderer.AddChild(level);
		renderer.AddChild(Player.instance);
		renderer.AddChild(background);
		
		setGamestate(Gamestate.RUNNING);
	}

    public override void _Process(float delta)
    {
		goal(delta);
    }

	private void tick(float delta) 
	{
		float speedDelta=level.Speed*delta;
		level.MoveLocalX(level.direction.x*speedDelta);
		level.MoveLocalY(level.direction.y*speedDelta);

		if(state!=Gamestate.RUNNING) return;

		if(Mathf.Abs(level.Position.x)>=level.pixelLength-RESOLUTION.x)
		{
			setGamestate(Gamestate.SCENE_CHANGE);
			Worker.setStatus(Worker.State.PREPARELEVEL);
		}
	}

	private void sceneRunning(float delta)
	{
		if(input.getPause())
		{
			GetTree().Paused^=true;
			if(GetTree().Paused)
			{
				lastState=state;
				setGamestate(Gamestate.PAUSED);
				PauseUI pause=(PauseUI)ResourceUtils.pause.Instance();
				pause.PauseMode=PauseModeEnum.Process;
				Node2D node=new Node2D();
				node.ZIndex=VisualServer.CanvasItemZMax;
				node.AddChild(pause);
				World.instance.renderer.AddChild(node);
			}
		}
		else if(input.getQuit())
		{
			CallDeferred(nameof(restartLevel),false);
			return;
		}
		tick(delta);
	}

	private void sceneChanged(float delta)
	{
		if(level.IsInsideTree())
		{
			setGamestate(Gamestate.RUNNING);
			tick(delta);
		}
	}

	public void resetGamestate()
	{
		setGamestate(lastState);
	}

	private void sceneChange(float delta)
	{
		if(level!=null&&level.IsInsideTree())
		{
			tick(delta);
		}
	}

	private void sceneIdle(float delta) {}

	public void setGamestate(Gamestate s)
	{
		state=s;
		switch(state)
		{
			case Gamestate.SCENE_CHANGED:
				goal=sceneChanged;
				break;
			case Gamestate.SCENE_CHANGE:
				goal=sceneChange;
				break;
			case Gamestate.RUNNING:
			case Gamestate.DIEING:
				goal=sceneRunning;
				break;
			default:
				goal=sceneIdle;
				break;
		}
	}

	public void restartLevel(bool keepLevel=false)
	{
		Worker.gc();
		setGamestate(Gamestate.RESTART);
		renderer.RemoveChild(level);
		if(!keepLevel)
		{
			currentLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		}
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		mergeMaps(level,cachedLevel);
		renderer.AddChild(level);
		renderer.RemoveChild(Player.instance);
		Player.instance.RemoveChild(PlayerCamera.instance);
		Player.instance.QueueFree();
		ResourceUtils.player.Instance();
		Player.instance.AddChild(PlayerCamera.instance);
		renderer.AddChild(Player.instance);
		setGamestate(Gamestate.RUNNING);
	}

	public void prepareAndChangeLevel()
	{
		currentLevel=nextLevel;
		nextLevel=(int)MathUtils.randomRange(0,ResourceUtils.levels.Count);
		Level newLevel=cachedLevel!=null?cachedLevel:(Level)ResourceUtils.levels[currentLevel].Instance();
		cachedLevel=(Level)ResourceUtils.levels[nextLevel].Instance();
		mergeMaps(newLevel,cachedLevel);
		renderer.CallDeferred("add_child", newLevel);

		while (!newLevel.IsInsideTree())
		{
			OS.DelayMsec(1);
		}

		Vector2 position=level.Position;
		renderer.CallDeferred("remove_child",level);
		newLevel.Position=new Vector2(-(Mathf.Abs(position.x)-(level.pixelLength-RESOLUTION.x)+16),position.y);
		level=newLevel;
		setGamestate(Gamestate.SCENE_CHANGED);
	}

	public void _Free()
	{
		setGamestate(Gamestate.RESTART);
		CallDeferred("queue_free");
		if(cachedLevel!=null)
		{
			cachedLevel.freeLevel();
		}
		input._free();
		World.instance=null;
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

	private void onMusicFinishedPlaying()
	{
		musicPlayer.Stream=ResourceUtils.ingameMusic[MathUtils.randomRangeInt(0,ResourceUtils.ingameMusic.Count)];
	}	
}
