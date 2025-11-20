using Godot;
using System;

public partial class SettingMenu : Control
{
	private HSlider volumeSlider;
	private CheckBox fullscreenCheck;
	private Button applyButton;

	private double pendingVolume;
	private bool pendingFullscreen;

	private const string ConfigPath = "user://settings.cfg";

	public override void _Ready()
	{
		volumeSlider = GetNode<HSlider>("SettingPanel/VolumeSlider");
		fullscreenCheck = GetNode<CheckBox>("SettingPanel/FullscreenCheck");
		applyButton = GetNode<Button>("SettingPanel/ApplyButton");

		GetNode<Button>("SettingPanel/BackButton").Pressed += OnBackPressed;
		applyButton.Pressed += OnApplyPressed;

		LoadSettings();

		volumeSlider.Value = pendingVolume;
		fullscreenCheck.ButtonPressed = pendingFullscreen;

		volumeSlider.ValueChanged += value => pendingVolume = value;
		fullscreenCheck.Toggled += pressed => pendingFullscreen = pressed;
	}

	private void LoadSettings()
	{
		var config = new ConfigFile();
		Error err = config.Load(ConfigPath);
		if (err == Error.Ok)
		{
			pendingVolume = (double)(config.GetValue("Audio", "Volume", 50.0));
			pendingFullscreen = (bool)(config.GetValue("Display", "Fullscreen", false));
		}
		else
		{
			pendingVolume = 50;
			pendingFullscreen = false;
		}
	}

	private void SaveSettings()
	{
		var config = new ConfigFile();
		config.SetValue("Audio", "Volume", pendingVolume);
		config.SetValue("Display", "Fullscreen", pendingFullscreen);
		config.Save(ConfigPath);
	}

	private void OnApplyPressed()
	{
		float db = Mathf.Lerp(-40, 0, (float)(pendingVolume / 100.0));
		AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Master"), db);

		if (!Engine.IsEditorHint())
		{
			if (pendingFullscreen)
			{
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
				DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, true);
			}
			else
			{
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
				DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.Borderless, false);
			}
		}

		SaveSettings();
		GD.Print($"Settings applied. Volume: {pendingVolume}, Fullscreen: {pendingFullscreen}");
	}

	private void OnBackPressed()
	{
		GetTree().ChangeSceneToFile("res://Scene/main_menu.tscn");
	}
}
