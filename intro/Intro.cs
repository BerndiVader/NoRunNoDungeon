using Godot;
using System;

public class Intro : Node
{
	private static InputController input;
	static AudioStreamMP3 music=ResourceLoader.Load<AudioStreamMP3>("res://sounds/title/Dark Age 11 Heroic Victory - 1.mp3");
	AudioStreamPlayer2D musicPlayer=new AudioStreamPlayer2D();
	
	public override void _Ready()
	{
		musicPlayer.Bus="Master";
		musicPlayer.Stream=music;
		musicPlayer.Position=new Vector2(256f,146f);
		AddChild(musicPlayer);
		musicPlayer.Play();

		AddChild(ResourceUtils.camera.Instance<PlayerCamera>());		
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
			Worker.stop();
		}
		base._Notification(what);
	}

}
