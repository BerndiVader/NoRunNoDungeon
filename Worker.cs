using Godot;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class Worker : Thread
{
    public static Worker instance;
	public static ConcurrentStack<WeakReference>placeholders;

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
	private static int timeouted=40;

	public static void Start()
	{
		instance=new Worker();
	}

	public Worker() : base()
	{
		placeholders=new ConcurrentStack<WeakReference>();
		SetStatus(State.IDLE);
		quit=false;
		Start(this,nameof(Runner));
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
			if(placeholder.isDisposed||placeholder.IsQueuedForDeletion())
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
				placeholder.EmitSignal("Create",iPlaceholder);
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
		World.instance.PrepareAndChangeLevel();
		SetStatus(State.IDLE);
	}

	private static void Idle()
	{
		if(placeholders.TryPop(out WeakReference result)&&result.IsAlive&&result.Target is Placeholder p)
		{
			InstantiatePlaceholder(p);
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
