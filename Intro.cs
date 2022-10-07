using Godot;
using System;

public class Intro : Node
{
	static InputController input;
	
	public override void _Ready()
	{
		input=ResourceUtils.getInputController(this);
		GetTree().CurrentScene=this;
		GetTree().Paused=false;
	}

	public override void _Process(float delta)
	{
		if(input.getJump())
		{
			World.changeScene(ResourceUtils.world);
		}
		else if(input.getQuit())
		{
			World.quit();
		}
	}

	public override void _Notification(int what)
	{
		if(what==MainLoop.NotificationWmQuitRequest)
		{
			Worker.stop=true;
			Worker.instance.WaitToFinish();
		}
		base._Notification(what);
	}

}
