using Godot;
using System;
using System.Collections.Generic;
using System.IO; // thêm dòng này để dùng Directory

public partial class MusicScreen : Control
{
	private TextureRect musicBackground;
	private ItemList musicList;
	private TextureButton playButton, pauseButton, nextButton, previouButton, refreshButton;
	private AudioStreamPlayer musicPlayer;

	private List<string> playlist = new List<string>();
	private int currentIndex = -1;
	private float pausedPosition = 0f;

	public override void _Ready()
	{
		refreshButton = GetNode<TextureButton>("RefreshButton");
		musicBackground = GetNode<TextureRect>("MusicBackground");
		musicList = GetNode<ItemList>("MusicList");
		playButton = GetNode<TextureButton>("PlayButton");
		pauseButton = GetNode<TextureButton>("PauseButton");
		nextButton = GetNode<TextureButton>("NextButton");
		previouButton = GetNode<TextureButton>("PreviouButton");
		musicPlayer = GetNode<AudioStreamPlayer>("MusicPlayer");

		musicPlayer.Finished += OnMusicFinished;
		refreshButton.Pressed += OnRefreshPressed;
		playButton.Pressed += OnPlayPressed;
		pauseButton.Pressed += OnPausePressed;
		nextButton.Pressed += OnNextPressed;
		previouButton.Pressed += OnPreviousPressed;

		LoadMusicFromSystem();
		SetDefaultBackground();
		pauseButton.Visible = false;
	}

	private void SetDefaultBackground()
	{
		var texture = GD.Load<Texture2D>("res://Assets/Background/HD-wallpaper-spotify-apps-music.jpg");
		if (texture != null)
			musicBackground.Texture = texture;
	}

	private void LoadMusicFromSystem()
	{
		playlist.Clear();
		musicList.Clear();

		string musicFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic);

		if (!Directory.Exists(musicFolder))
		{
			GD.Print("Không mở được thư mục Music");
			return;
		}

		string[] files = Directory.GetFiles(musicFolder);

		foreach (string file in files)
		{
			if (file.EndsWith(".mp3") || file.EndsWith(".ogg") || file.EndsWith(".wav"))
			{
				playlist.Add(file);
				musicList.AddItem(Path.GetFileName(file));
			}
		}
	}

	private void OnPlayPressed()
	{
		int selected = musicList.GetSelectedItems().Length > 0 ? musicList.GetSelectedItems()[0] : currentIndex;
		if (selected == -1) return;

		if (pausedPosition > 0f && selected == currentIndex)
		{
			musicPlayer.Play(pausedPosition);
			pausedPosition = 0f;
		}
		else
		{
			PlayMusic(selected);
		}

		playButton.Visible = false;
		pauseButton.Visible = true;
	}

	private void OnPausePressed()
	{
		if (musicPlayer.Playing)
		{
			pausedPosition = musicPlayer.GetPlaybackPosition();
			musicPlayer.Stop();
		}

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
		int next = (currentIndex + 1) % playlist.Count;
		PlayMusic(next);
	}

	private void PlayMusic(int index)
	{
		if (index < 0 || index >= playlist.Count) return;

		string path = playlist[index];

		// Đọc file nhạc dạng byte[]
		var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
		if (file == null)
		{
			GD.PrintErr("Không đọc được file: " + path);
			return;
		}

		byte[] data = file.GetBuffer((long)file.GetLength());
		file.Close();

		// Tạo AudioStreamMP3 từ byte[]
		var stream = new AudioStreamMP3();
		stream.Data = data;

		musicPlayer.Stream = stream;
		musicPlayer.Play();

		currentIndex = index;
		musicList.Select(index);
	}
	private void OnRefreshPressed()
	{
		LoadMusicFromSystem();
	}
}
