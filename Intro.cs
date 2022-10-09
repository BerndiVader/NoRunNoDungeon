using Godot;
using System;

public class Intro : Node
{
	private static InputController input;
	
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
			input._free();
			World.changeScene(ResourceUtils.world);
		}
		else if(input.getQuit())
		{
			input._free();
			input=null;
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
