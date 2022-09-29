using Godot;
using System;
using System.Collections.Generic;

public static class WorldUtils
{
	public static World world;
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
			var currentScene=WorldUtils.root.GetTree().CurrentScene;
			WorldUtils.root.AddChild(newScene.Instance());
			WorldUtils.root.RemoveChild(currentScene);
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
		if(world!=null&&world.cachedLevel!=null) world.cachedLevel.CallDeferred("queue_free");
		ResourceUtils.worker.stop=true;
		ResourceUtils.worker.WaitToFinish();
		while(ResourceUtils.worker.IsActive())
		{
			OS.DelayMsec(1);
		}
		root.GetTree().Quit();
	}
	
}
