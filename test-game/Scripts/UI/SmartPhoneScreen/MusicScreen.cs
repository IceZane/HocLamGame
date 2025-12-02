using Godot;
using System;
using System.Collections.Generic;

public partial class MusicScreen : Control
{
	private TextureRect musicBackground;
	private ItemList musicList;
	private TextureButton playButton, pauseButton, nextButton, previouButton;
	private AudioStreamPlayer musicPlayer;

	private List<string> playlist = new List<string>();
	private int currentIndex = -1;

	public override void _Ready()
	{
		musicBackground = GetNode<TextureRect>("MusicBackground");
		musicList = GetNode<ItemList>("MusicList");
		playButton = GetNode<TextureButton>("PlayButton");
		pauseButton = GetNode<TextureButton>("PauseButton");
		nextButton = GetNode<TextureButton>("NextButton");
		previouButton = GetNode<TextureButton>("PreviouButton");
		musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");
		
		musicPlayer.Finished += OnMusicFinished;

		playButton.Pressed += OnPlayPressed;
		pauseButton.Pressed += OnPausePressed;
		nextButton.Pressed += OnNextPressed;
		previouButton.Pressed += OnPreviousPressed;

		LoadMusic();
		SetDefaultBackground();
		pauseButton.Visible = false;
	}

	private void LoadMusic()
	{
		var dir = DirAccess.Open("res://CustomMusic");
		if (dir == null) return;

		dir.ListDirBegin();
		string file = dir.GetNext();
		while (file != "")
		{
			if (!dir.CurrentIsDir() && (file.EndsWith(".ogg") || file.EndsWith(".wav") || file.EndsWith(".mp3")))
			{
				playlist.Add(file);
				musicList.AddItem(file);
			}
			file = dir.GetNext();
		}
		dir.ListDirEnd();
	}

	private void SetDefaultBackground()
	{
		var texture = GD.Load<Texture2D>("res://Assets/Background/HD-wallpaper-spotify-apps-music.jpg");
		if (texture != null)
			musicBackground.Texture = texture;
	}

	private void PlayMusic(int index)
	{
		if (index < 0 || index >= playlist.Count) return;

		var stream = GD.Load<AudioStream>("res://CustomMusic/" + playlist[index]);
		if (stream != null)
		{
			musicPlayer.Stream = stream;
			musicPlayer.Play();
			currentIndex = index;
			musicList.Select(index);
		}
	}

	private void OnPlayPressed()
	{
		int selected = musicList.GetSelectedItems().Length > 0 ? musicList.GetSelectedItems()[0] : currentIndex;
		if (selected == -1) return;
		PlayMusic(selected);
		
		playButton.Visible = false;
		pauseButton.Visible = true;
	}

	private void OnPausePressed()
	{
		if (musicPlayer.Playing)
			musicPlayer.Stop();
			
		pauseButton.Visible = false;
		playButton.Visible = true;


	}

	private void OnNextPressed()
	{
		int next = (currentIndex + 1) % playlist.Count;
		PlayMusic(next);
	}

	private void OnPreviousPressed()
	{
		int prev = (currentIndex - 1 + playlist.Count) % playlist.Count;
		PlayMusic(prev);
	}
	private void OnMusicFinished()
	{
		// Tự động chuyển sang bài tiếp theo
		int next = (currentIndex + 1) % playlist.Count;
		PlayMusic(next);
	}
}
