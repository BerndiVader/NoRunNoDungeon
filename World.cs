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
		Worker.Gc();
		Node currentScene=root.GetTree().CurrentScene;
		root.AddChild(newScene.Instance());
		root.RemoveChild(currentScene);		
		if(currentScene.GetType().Name.Equals("World"))
		{
			((World)currentScene)._Free();
		}
		else if(!currentScene.IsQueuedForDeletion())
		{
			currentScene.QueueFree();
		}
	}

	public static void OnObjectExitedScreen(Node node)
	{
        if(PlayerCamera.instance.Zoom.x==1f&&state!=Gamestate.BONUS)
        {
            node.CallDeferred("queue_free");
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
		if(GameSettings.current.Usage==Viewport.UsageEnum.Usage2d)
		{
			GetNode("WorldEnvironment").QueueFree();
		}
		else if(GetNodeOrNull("WorldEnvironment") is WorldEnvironment env)
		{
			env.Environment.GlowEnabled=GameSettings.current.Glow;
        }
		input=ResourceUtils.GetInputController(uiLayer);
		renderer=GetNode<Renderer>("Renderer");
		
		tileSet=ResourceUtils.tilesets[MathUtils.RandomRange(0,ResourceUtils.tilesets.Count)];
		currentLevel=MathUtils.RandomRange(0,ResourceUtils.levels.Count);
		level=(Level)ResourceUtils.levels[currentLevel].Instance();
		nextLevel=MathUtils.RandomRange(0,ResourceUtils.levels.Count);
		cachedLevel=(Level)ResourceUtils.levels[nextLevel].Instance();
		MergeMaps(level,cachedLevel);

		ResourceUtils.player.Instance();
		Player.LIVES=3;
		Player.instance.AddChild(PlayerCamera.instance);

		background=(Background)ResourceUtils.background.Instance();

		renderer.AddChild(level);
		renderer.AddChild(Player.instance);
		renderer.AddChild(background);
		
		SetGamestate(Gamestate.RUNNING);
	}

    public override void _PhysicsProcess(float delta)
    {
		goal(delta);
    }

	private void Tick(float delta) 
	{
		float speedDelta=level.Speed*delta;
		level.MoveLocalX(level.direction.x*speedDelta);
		level.MoveLocalY(level.direction.y*speedDelta);
		level.lastDirection=level.direction;

		if(state!=Gamestate.RUNNING) return;

		if(Mathf.Abs(level.Position.x)>=level.pixelLength-RESOLUTION.x)
		{
			SetGamestate(Gamestate.SCENE_CHANGE);
			Worker.SetStatus(Worker.State.PREPARELEVEL);
			return;
		}

		if(level.direction.y!=0f)
		{
			if(level.Position.y<0f)
			{
				level.Position=new Vector2(level.Position.x,0f);
				if(level.settings.autoRestore)
				{
					level.DEFAULT_SETTING.Restore();
				}
				else
				{
					level.direction=Vector2.Zero;
				}
			}
			else if(level.Position.y+RESOLUTION.y>level.pixelHeight.y)
			{
				level.Position=new Vector2(level.Position.x,level.pixelHeight.y-RESOLUTION.y);
				if(level.settings.autoRestore)
				{
					level.DEFAULT_SETTING.Restore();
				}
				else
				{
					level.direction=Vector2.Zero;
				}
			}
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
		Tick(delta);
	}

	private void SceneChanged(float delta)
	{
		if(level.IsInsideTree())
		{
			SetGamestate(Gamestate.RUNNING);
			Tick(delta);
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
			Tick(delta);
		}
	}

	private void SceneIdle(float delta) {}

	public void SetGamestate(Gamestate s)
	{
		lastState=state;
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
			case Gamestate.BONUS:
				goal=SceneRunning;
				break;
			default:
				goal=SceneIdle;
				break;
		}
	}

	public void RestoreGamestate()
	{
		state=lastState;
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
			case Gamestate.BONUS:
				goal=SceneRunning;
				break;
			default:
				goal=SceneIdle;
				break;
		}
	}

	public void RestartLevel(bool keepLevel=false)
	{
		SetGamestate(Gamestate.RESTART);
		renderer.RemoveChild(level);
		if(!keepLevel)
		{
			Worker.Gc();
			currentLevel=MathUtils.RandomRange(0,ResourceUtils.levels.Count);
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
		nextLevel=MathUtils.RandomRange(0,ResourceUtils.levels.Count);
		Level newLevel=cachedLevel!=null?cachedLevel:(Level)ResourceUtils.levels[currentLevel].Instance();
		cachedLevel=(Level)ResourceUtils.levels[nextLevel].Instance();

		MergeMaps(newLevel,cachedLevel);
		renderer.CallDeferred("add_child",newLevel);
		int timeout=0;
		int timeouted=40;
		while(!newLevel.IsInsideTree()&&timeout<timeouted)
		{
			timeout++;
			OS.DelayMsec(1);
		}
		if(timeout>=timeouted)
		{
			GD.Print("Add new level to tree timeout.");
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
		if(cachedLevel!=null)
		{
			cachedLevel.FreeLevel();
		}
		input.Free();
		instance=null;
		QueueFree();
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
		musicPlayer.Stream=ResourceUtils.ingameMusic[MathUtils.RandomRange(0,ResourceUtils.ingameMusic.Count)];
	}	
}
