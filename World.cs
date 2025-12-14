using Godot;

public class World : Node
{
	public static World instance;
	public static Viewport root;
	public static Vector2 RESOLUTION=new Vector2(512f,288f);

	public static void Init(Viewport viewPort)
	{
		root=viewPort;
	}	

	public static void MergeMaps(Level newLevel, Level nextLevel) 
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

	public static void ChangeScene(PackedScene newScene)
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
		Worker.Gc();
	}

	public static void OnObjectExitedScreen(Node node)
	{
        if(PlayerCamera.instance.Zoom.x==1f)
        {
            node.QueueFree();
        }
	}

	public static void Quit() 
	{
		Worker.Stop();
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
	public CanvasLayer uiLayer;
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
		OnMusicFinishedPlaying();
		musicPlayer.Position=new Vector2(RESOLUTION.x*0.5f,RESOLUTION.y*0.5f);
		musicPlayer.Connect("finished",this,nameof(OnMusicFinishedPlaying));
		AddChild(musicPlayer);
		musicPlayer.Play();

		uiLayer=GetNode<CanvasLayer>(nameof(CanvasLayer));

		ResourceUtils.camera.Instance<PlayerCamera>();
		if(GameSettings.current.usage==Viewport.UsageEnum.Usage2d)
		{
			GetNode("WorldEnvironment").QueueFree();
		}
		input=ResourceUtils.GetInputController(uiLayer);
		renderer=GetNode<Renderer>("Renderer");
		

		tileSet=ResourceUtils.tilesets[(int)MathUtils.RandomRange(0,ResourceUtils.tilesets.Count)];
		currentLevel=(int)MathUtils.RandomRange(0,ResourceUtils.levels.Count);
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		nextLevel=(int)MathUtils.RandomRange(0,ResourceUtils.levels.Count);
		cachedLevel=(Level)ResourceUtils.levels[nextLevel].Instance();
		MergeMaps(level,cachedLevel);
		ResourceUtils.player.Instance();
		Player.LIVES=3;
		background=(Background)ResourceUtils.background.Instance();

		Player.instance.AddChild(PlayerCamera.instance);
		renderer.AddChild(level);
		renderer.AddChild(Player.instance);
		renderer.AddChild(background);
		
		SetGamestate(Gamestate.RUNNING);
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
			SetGamestate(Gamestate.SCENE_CHANGE);
			Worker.SetStatus(Worker.State.PREPARELEVEL);
		}
	}

	private void SceneRunning(float delta)
	{
		if(input.Pause())
		{
			GetTree().Paused^=true;
			if(GetTree().Paused)
			{
				lastState=state;
				SetGamestate(Gamestate.PAUSED);
				PauseUI pause=(PauseUI)ResourceUtils.pause.Instance();
				pause.PauseMode=PauseModeEnum.Process;
				instance.uiLayer.AddChild(pause);
			}
		}
		else if(input.Quit())
		{
			CallDeferred(nameof(RestartLevel),false);
			return;
		}
		tick(delta);
	}

	private void SceneChanged(float delta)
	{
		if(level.IsInsideTree())
		{
			SetGamestate(Gamestate.RUNNING);
			tick(delta);
		}
	}

	public void ResetGamestate()
	{
		SetGamestate(lastState);
	}

	private void SceneChange(float delta)
	{
		if(level!=null&&level.IsInsideTree())
		{
			tick(delta);
		}
	}

	private void SceneIdle(float delta) {}

	public void SetGamestate(Gamestate s)
	{
		state=s;
		switch(state)
		{
			case Gamestate.SCENE_CHANGED:
				goal=SceneChanged;
				break;
			case Gamestate.SCENE_CHANGE:
				goal=SceneChange;
				break;
			case Gamestate.RUNNING:
			case Gamestate.DIEING:
				goal=SceneRunning;
				break;
			default:
				goal=SceneIdle;
				break;
		}
	}

	public void RestartLevel(bool keepLevel=false)
	{
		Worker.Gc();
		SetGamestate(Gamestate.RESTART);
		renderer.RemoveChild(level);
		if(!keepLevel)
		{
			currentLevel=(int)MathUtils.RandomRange(0,ResourceUtils.levels.Count);
		}
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		MergeMaps(level,cachedLevel);
		renderer.AddChild(level);
		renderer.RemoveChild(Player.instance);
		Player.instance.RemoveChild(PlayerCamera.instance);
		Player.instance.QueueFree();
		ResourceUtils.player.Instance();
		Player.instance.AddChild(PlayerCamera.instance);
		renderer.AddChild(Player.instance);
		SetGamestate(Gamestate.RUNNING);
	}

	public void PrepareAndChangeLevel()
	{
		currentLevel=nextLevel;
		nextLevel=(int)MathUtils.RandomRange(0,ResourceUtils.levels.Count);
		Level newLevel=cachedLevel!=null?cachedLevel:(Level)ResourceUtils.levels[currentLevel].Instance();
		cachedLevel=(Level)ResourceUtils.levels[nextLevel].Instance();
		MergeMaps(newLevel,cachedLevel);
		renderer.CallDeferred("add_child", newLevel);

		while (!newLevel.IsInsideTree())
		{
			OS.DelayMsec(1);
		}

		Vector2 position=level.Position;
		renderer.CallDeferred("remove_child",level);
		newLevel.Position=new Vector2(-(Mathf.Abs(position.x)-(level.pixelLength-RESOLUTION.x))-16f,position.y);
		level=newLevel;
		SetGamestate(Gamestate.SCENE_CHANGED);
	}

	public void _Free()
	{
		SetGamestate(Gamestate.RESTART);
		CallDeferred("queue_free");
		if(cachedLevel!=null)
		{
			cachedLevel.FreeLevel();
		}
		input.Free();
		World.instance=null;
	}
	public override void _Notification(int what)
	{
		if(what==MainLoop.NotificationWmQuitRequest)
		{
			Quit();
		}
		base._Notification(what);
	}
	public override void _EnterTree()
	{
		instance=this;
		GetTree().CurrentScene=this;
	}

	private void OnMusicFinishedPlaying()
	{
		musicPlayer.Stream=ResourceUtils.ingameMusic[MathUtils.RandomRangeInt(0,ResourceUtils.ingameMusic.Count)];
	}	
}
