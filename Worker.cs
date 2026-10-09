using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class Worker : Thread
{
    public static Worker instance;
	public static readonly ConcurrentStack<WeakReference>placeholders=new ConcurrentStack<WeakReference>();

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
	private static float DISTANCE_SQRT;
	private static ulong nextMarkedScan=0;

	public static void Start()
	{
		instance=new Worker();
	}

	public Worker() : base()
	{
		SetStatus(State.IDLE);
		quit=false;
		Start(this,nameof(Runner));
		DISTANCE_SQRT=(World.RESOLUTION.x+World.RESOLUTION.y)*0.5f;
		DISTANCE_SQRT*=DISTANCE_SQRT;
		DISTANCE_SQRT*=2f;
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
		World.MARKED_NODES.Clear();
		World.instance.PrepareAndChangeLevel();
		SetStatus(State.IDLE);
	}

	private static void Idle()
	{
		if(placeholders.TryPop(out WeakReference result)&&result.IsAlive&&result.Target is Placeholder p)
		{
			InstantiatePlaceholder(p);
		}
		else if(World.MARKED_NODES.Count>0&&World.state<Gamestate.BONUS&&OS.GetTicksMsec()>nextMarkedScan)
		{
			nextMarkedScan=OS.GetTicksMsec()+20000;

			foreach(var entry in World.MARKED_NODES)
			{
				WeakReference<Node2D>candit=entry.Value;
				if(candit.TryGetTarget(out Node2D target))
				{
					if(IsInstanceValid(target)&&!target.IsQueuedForDeletion())
					{
						if(target.GlobalPosition.DistanceSquaredTo(PlayerCamera.instance.GlobalPosition)>DISTANCE_SQRT)
						{
                			World.MARKED_NODES.TryRemove(entry.Key,out _);
							target.CallDeferred("queue_free");
						}
					}
					else
					{
						World.MARKED_NODES.TryRemove(entry.Key,out _);
					}
				}
				else
				{
                	World.MARKED_NODES.TryRemove(entry.Key,out _);
				}
			}
		}
		else
		{
			OS.DelayMsec(20);
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
