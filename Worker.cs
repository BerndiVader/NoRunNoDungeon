using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class Worker : Thread
{
    public static Worker instance;
	public static readonly ConcurrentStack<WeakReference>placeholders=new ConcurrentStack<WeakReference>();
	public static readonly ConcurrentDictionary<ulong,WeakReference<Node2D>>cleanup_candits=new ConcurrentDictionary<ulong, WeakReference<Node2D>>();


	public enum State
	{
		IDLE,
		PREPARELEVEL,
		QUITTING,
	}
	private static State state;
	private delegate void Goal();
	private static Goal goal;
	private static bool quit;
	private static float CLEANUP_DISTANCE_SQRT;
	private static ulong NEXT_CLEANUP=0;

	public static void Start()
	{
		instance=new Worker();
	}

	public Worker() : base()
	{
		SetStatus(State.IDLE);
		quit=false;
		Start(this,nameof(Runner));
		CLEANUP_DISTANCE_SQRT=(World.RESOLUTION.x+World.RESOLUTION.y)*0.5f;
		CLEANUP_DISTANCE_SQRT*=CLEANUP_DISTANCE_SQRT;
		CLEANUP_DISTANCE_SQRT*=2f;
	}

	private void Runner()
	{
		while(!quit)
		{
			goal();
		}
	}

	private static void InstantiatePlaceholder(Placeholder placeholder)
	{
		try
		{
			if(!IsInstanceValid(placeholder)||placeholder.isDisposed||placeholder.IsQueuedForDeletion())
			{
				return;
			}
			if(placeholder.IsInsideTree())
			{
				InstancePlaceholder iPlaceholder=placeholder.GetChild<InstancePlaceholder>(0);
				string instancePath=iPlaceholder.GetInstancePath();
				if(!ResourceLoader.HasCached(instancePath))
				{
					ResourceLoader.Load(instancePath);
				}
				placeholder.EmitSignal("Create",iPlaceholder,true);
			}
			else
			{
				GD.Print("Placeholder not in tree anymore: "+placeholder);
				placeholder.CallDeferred("queue_free");
			}
		}
		catch(Exception e)
		{
			GD.Print("Instantiate Placeholder failed: "+e);
		}
	}

	private static void PrepareAndChangeLevel()
	{
		placeholders.Clear();
		cleanup_candits.Clear();
		World.instance.PrepareAndChangeLevel();
		SetStatus(State.IDLE);
	}

	private static void Idle()
	{
		if(placeholders.TryPop(out WeakReference result)&&result.IsAlive&&result.Target is Placeholder p)
		{
			InstantiatePlaceholder(p);
		}
		else if(cleanup_candits.Count>0&&World.state<Gamestate.BONUS&&Time.GetTicksMsec()>NEXT_CLEANUP)
		{
			Cleanup();
		}
		else
		{
			OS.DelayMsec(20);
		}
	}

	public static void CleanupCandit(Node2D candit)
	{
		cleanup_candits.TryAdd(candit.GetInstanceId(),new WeakReference<Node2D>(candit));
	}

	public static async void ForceCleanupAsync()
	{
		await Task.Run(delegate()
		{
			Cleanup();
		});
	}

	private static void Cleanup()
	{
		NEXT_CLEANUP=Time.GetTicksMsec()+10000;
		Vector2 cam=PlayerCamera.instance.GlobalPosition;

		foreach(var entry in cleanup_candits)
		{
			if(!entry.Value.TryGetTarget(out Node2D candit)||!IsInstanceValid(candit)||candit.IsQueuedForDeletion())
			{
				cleanup_candits.TryRemove(entry.Key,out _);
				continue;
			}

			if(candit.GlobalPosition.DistanceSquaredTo(cam)<CLEANUP_DISTANCE_SQRT)
			{
				continue;
			}

			if(cleanup_candits.TryRemove(entry.Key,out _))
			{
				candit.CallDeferred("queue_free");
			}
		}

	}
	
	private static void Quitting()
    {
		OS.DelayMsec(1);
    }

	public static void Stop()
	{
        SetStatus(State.QUITTING);
		quit=true;
		GD.Print("Wait for worker to finish...");
		instance.WaitToFinish();
		while(instance.IsActive())
		{
			GD.Print(".");
			OS.DelayMsec(1);
		}
		GD.Print("Done!");		
	}

	public static void SetStatus(State s)
	{
		switch(s)
		{
			case State.PREPARELEVEL:
				goal=PrepareAndChangeLevel;
				break;
			case State.IDLE:
				goal=Idle;
				break;
			case State.QUITTING:
				goal=Quitting;
				break;
		}
		state=s;
	}

	public static async void Gc()
	{
		await Task.Run(delegate()
		{
			GC.Collect();
		});
	}
}
