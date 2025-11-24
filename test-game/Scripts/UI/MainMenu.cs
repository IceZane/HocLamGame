using Godot;
using System;

public partial class MainMenu : Control
{
	private Label loadingLabel;

	public override void _Ready()
	{
		GetNode<Button>("ButtonPanel/StartButton").Pressed += OnStartPressed;
		GetNode<Button>("ButtonPanel/SettingButton").Pressed += OnSettingPressed;
		GetNode<Button>("ButtonPanel/QuitButton").Pressed += OnQuitPressed;

		loadingLabel = GetNode<Label>("LoadingLabel");
		loadingLabel.Visible = false;
	}

	private async void OnStartPressed()
	{
		GD.Print("Start button pressed!");
		loadingLabel.Visible = true;
		await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
		var transition = GetNode<Transition>("/root/Transition");
		await transition.PlayIn(); // chỉ chạy Transition_in

		GetTree().ChangeSceneToFile("res://Scene/Level1.tscn");
	}

	private void OnSettingPressed()
	{
		GD.Print("Setting button pressed!");
		GetTree().ChangeSceneToFile("res://Scene/setting_menu.tscn");
	}

	private void OnQuitPressed()
	{
		GD.Print("Quit button pressed!");
		GetTree().Quit();
	}
}
