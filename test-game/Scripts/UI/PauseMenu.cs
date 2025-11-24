using Godot;
using System;

public partial class PauseMenu : Control
{
	public override void _Ready()
	{
		Visible = false;
		ProcessMode = ProcessModeEnum.Always;

		GetNode<Button>("MenuBox/ResumeButton").Pressed += OnResumePressed;
		GetNode<Button>("MenuBox/QuitButton").Pressed += OnQuitPressed;

		// Đảm bảo menu hiện lên khi được add
	}

	private void OnResumePressed()
	{
		GetTree().Paused = false;
		Visible = false; // Xoá menu khỏi tree
	}

	private void OnQuitPressed()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://Scene/main_menu.tscn");
	}
}
