using Godot;
using System;

public partial class MainCharacter : CharacterBody2D
{
	// ====== MOVEMENT SETTINGS ======
	private bool isClimbing = false;
	public const float WalkSpeed = 50f;
	public const float RunSpeed = 150f;
	public const float JumpVelocity = -200f;
	private int jumpCount = 0;
	private int maxJumpCount = 2; // nhảy tối đa 2 lần

	private float currentSpeed = WalkSpeed;

	// ====== COMPONENTS ======
	private AnimatedSprite2D anim;
	private AudioStreamPlayer2D shootSound;
	private AudioStreamPlayer2D reloadSound;
	private AudioStreamPlayer2D attackSound;
	private AudioStreamPlayer2D hitZombieSound;

	// ====== STATE FLAGS ======
	private bool FacingRight = true;
	private bool isAttacking = false;
	private bool isReloading = false;
	private bool isShooting = false;
	private bool isHurt = false;
	private bool isDead = false;

	// ====== AMMO ======
	public const int Capacity = 30;
	private int currentAmmo = 30;
	private int maxAmmo = 120;

	public int CurrentAmmo => currentAmmo;
	public int MaxAmmo => maxAmmo;

	[Export] public PackedScene BulletScene;

	// ====== HEALTH ======
	private int currentHP = 4;
	private const int maxHP = 4;
	private int lives = 3; // số mạng ban đầu
	private const int maxLives = 3;
	public int GetHP() => currentHP;

	public override void _Ready()
	{
		var camera = GetNodeOrNull<Camera2D>("../CharacterBody2D/Camera2D");
		var tileMapLayer = GetNodeOrNull<TileMapLayer>("../TileMap/TileMapLayer");

		if (camera == null || tileMapLayer == null)
		{
			GD.PrintErr("Không tìm thấy Camera2D hoặc TileMapLayer!");
			return;
		}

		var usedRect = tileMapLayer.GetUsedRect();
		var cellSize = tileMapLayer.TileSet.TileSize;

		camera.LimitLeft = (int)(usedRect.Position.X * cellSize.X);
		camera.LimitTop = (int)(usedRect.Position.Y * cellSize.Y);
		camera.LimitRight = (int)((usedRect.Position.X + usedRect.Size.X) * cellSize.X);
		camera.LimitBottom = (int)((usedRect.Position.Y + usedRect.Size.Y) * cellSize.Y);

		GD.Print($"Camera limits set: L={camera.LimitLeft}, R={camera.LimitRight}, T={camera.LimitTop}, B={camera.LimitBottom}");

		var gameOverLabel = GetNodeOrNull<Label>("../HUD/GameOver");
		if (gameOverLabel != null)
		{
			gameOverLabel.Visible = false;
		}
		UpdateLivesUI();

		var tileMap = GetNodeOrNull<Node>("TileMap");
		if (tileMap != null)
		{
			foreach (Node child in tileMap.GetChildren())
			{
				GD.Print("TileMap child: " + child.Name + " (" + child.GetType().ToString() + ")");
			}
		}
		shootSound = GetNode<AudioStreamPlayer2D>("ShootSound");
		reloadSound = GetNode<AudioStreamPlayer2D>("ReloadSound");
		attackSound = GetNode<AudioStreamPlayer2D>("AttackSound");
		hitZombieSound = GetNode<AudioStreamPlayer2D>("HitZombieSound");
		anim = GetNode<AnimatedSprite2D>("Sprite2D");
		

		anim.AnimationFinished += OnAnimationFinished;
		AddToGroup("player");
	}
	public override void _Process(double delta)
{
	if (Input.IsActionJustPressed("ui_cancel"))
	{
		var pauseMenu = GetTree().Root.GetNode<PauseMenu>("Node/HUD/PauseMenu");

		if (!GetTree().Paused)
		{
			GetTree().Paused = true;
			pauseMenu.Visible = true;
		}
		else
		{
			GetTree().Paused = false;
			pauseMenu.Visible = false;
		}
	}
}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;
		// ====== Ladder check ======
		isClimbing = CheckLadder();

		if (isClimbing)
		{
			velocity.Y = Input.GetAxis("ui_up", "ui_down") * WalkSpeed;
			if (Mathf.Abs(velocity.Y) < 1f)
				velocity.Y = 0;
		}
		else
		{
			// Gravity
			if (!IsOnFloor())
			{
				object gravityObj = ProjectSettings.GetSetting("physics/2d/default_gravity");
				float gravity = 400f;
				if (gravityObj is float g)
					gravity = g;
				velocity.Y += gravity * (float)delta;
			}

			// Jump
			if (Input.IsActionJustPressed("ui_accept") && jumpCount < maxJumpCount && !isDead)
			{
				velocity.Y = JumpVelocity;
				jumpCount++;

				anim.Stop();
				anim.Play("Jump");
 				// ✅ chạy animation mỗi lần nhảy
			}
		}

		// Run or Walk
		float dir = Input.GetAxis("ui_left", "ui_right");
		bool running = Input.IsActionPressed("run");
		currentSpeed = running ? RunSpeed : WalkSpeed;
		velocity.X = dir * currentSpeed;

		// Flip sprite + cập nhật FacingRight
		if (dir != 0)
		{
			anim.FlipH = dir < 0;
			FacingRight = dir > 0;

			var gunPoint = GetNode<Node2D>("Sprite2D/GunPoint");
			gunPoint.Position = new Vector2(FacingRight ? +22.485f : -22.485f, gunPoint.Position.Y);
		}

		Velocity = velocity;
		MoveAndSlide();
		if (IsOnFloor())
			jumpCount = 0;

		// =============================
		// Combat input
		// =============================
		if (!isDead && !isShooting && !isAttacking && !isReloading)
		{
			if (Input.IsActionJustPressed("shoot") && currentAmmo > 0)
			{
				isShooting = true;
				ShootBullet();
				PlayAnim("Shooting");
				return;
			}

			if (Input.IsActionJustPressed("attack"))
			{
				isAttacking = true;
				DoAttack();
				PlayAnim("Attack");
				return;
			}

			if (Input.IsActionJustPressed("reload"))
			{
				isReloading = true;
				DoReload();
				PlayAnim("Reload");
				return;
			}
		}

		// =============================
		// Movement animation
		// =============================
		if (!isShooting && !isAttacking && !isReloading && !isDead)
		{
			if (isHurt)
			{
				PlayAnim("Hurt");
			}
			else if (!IsOnFloor())
			{
				PlayAnim("Jump");
			}
			else if (Mathf.Abs(Velocity.X) > 0 && Input.IsActionPressed("run"))
			{
				PlayAnim("Run");
			}
			else if (Mathf.Abs(Velocity.X) > 10)
			{
				PlayAnim("Walk");
			}
			else
			{
				PlayAnim("default");
			}
		}

	}

	private void PlayAnim(string name)
	{
		if (anim == null) return;

		if (anim.Animation != name)
		{
			try { anim.Play(name); }
			catch { GD.PrintErr($"Animation '{name}' does not exist!"); }
		}
	}

	private void OnAnimationFinished()
	{
		switch (anim.Animation)
		{
			case "Shooting": isShooting = false; PlayAnim("default"); break;
			case "Attack": isAttacking = false; PlayAnim("default"); break;
			case "Reload": isReloading = false; PlayAnim("default"); break;
			case "Hurt": isHurt = false; PlayAnim("default"); break;
			case "Dead": break;
		}
	}

	// ======================================
	// COMBAT METHODS
	// ======================================
	private void DoAttack()
	{
		GD.Print("Attack executed!");
		attackSound?.Play();
		var zombies = GetTree().GetNodesInGroup("zombie");
		foreach (var node in zombies)
		{
			if (node is ZombieEnemy zombie)
			{
				float attackRange = 20f;
				if (GlobalPosition.DistanceTo(zombie.GlobalPosition) <= attackRange)
				{
					zombie.TakeDamage(1);
					hitZombieSound?.Play();
					GD.Print("Hit zombie!");
				}
			}
		}
	}

	public void DoReload()
	{
		reloadSound?.Play();
		int missingAmmo = Capacity - currentAmmo;
		int ammoToLoad = Mathf.Min(missingAmmo, maxAmmo);
		currentAmmo += ammoToLoad;
		maxAmmo -= ammoToLoad;

		GD.Print($"Reload executed! CurrentAmmo: {currentAmmo}, MaxAmmo: {maxAmmo}");
	}

	private void ShootBullet()
	{
		if (BulletScene == null)
		{
			GD.PrintErr("BulletScene chưa được gán trong Inspector!");
			return;
		}
		shootSound?.Play();

		Bullet bullet = BulletScene.Instantiate<Bullet>();

		var muzzle = GetNode<Node2D>("Sprite2D/GunPoint");
		bullet.GlobalPosition = muzzle.GlobalPosition;

		bullet.Direction = FacingRight ? Vector2.Right : Vector2.Left;

		GetParent().AddChild(bullet);

		currentAmmo--;
		GD.Print("Shoot executed! Ammo left: " + currentAmmo);
	}

	// ======================================
	// DAMAGE METHODS
	// ======================================
	public void TakeDamage()
	{
		if (isDead) return;

		currentHP = Mathf.Max(currentHP - 1, 0);
		isHurt = true;
		PlayAnim("Hurt");
		UpdateHealthBar();

		SetPhysicsProcess(false); // Tạm dừng xử lý

		if (currentHP <= 0)
		{
			HandleDeath(); // gọi hàm mới
		}
		else
		{
			GetTree().CreateTimer(0.3).Timeout += () =>
			{
				SetPhysicsProcess(true);
			};
		}
	}

	public void Die()
	{
		if (isDead) return;

		isDead = true;
		PlayAnim("Dead");

		Velocity = Vector2.Zero;
		SetPhysicsProcess(false);

		GD.Print("Player has died.");
	}


	private void UpdateHealthBar()
	{
		var healthBars = GetTree().GetNodesInGroup("healthbar");
		if (healthBars.Count == 0) return;

		var bar = healthBars[0] as AnimatedSprite2D;

		string animName = currentHP switch
		{
			0 => "0 HP",
			1 => "1 HP",
			2 => "2 HP",
			3 => "3 HP",
			4 => "Full HP",
			_ => "0 HP"
		};

		bar.Animation = animName;
		bar.Frame = 0;
		bar.Stop();
	}

	// ======================================
	// LADDER CHECK
	// ======================================
	private bool CheckLadder()
	{
		var ray = GetNodeOrNull<RayCast2D>("LadderRayCast");
		if (ray == null)
		{
			GD.PrintErr("Không tìm thấy LadderRayCast!");
			return false;
		}

		// Nếu ray va chạm với tile thang (layer 2), trả về true
		return ray.IsColliding();
	}
	private void LoseLife()
	{
		lives--;

		UpdateLivesUI();

		if (lives > 0)
		{
			// Reset HP và hồi sinh
			currentHP = maxHP;
			Respawn();
		}
		else
		{
			GameOver();
		}
	}
	private void UpdateLivesUI()
	{
		var lifeCountLabel = GetNodeOrNull<Label>("../HUD/LifeCount");
		if (lifeCountLabel != null)
		{
			lifeCountLabel.Text = "x" + lives;
		}
	}
	private void GameOver()
	{
		GD.Print("Game Over!");

		var gameOverLabel = GetNodeOrNull<Label>("../HUD/GameOver");
		if (gameOverLabel != null)
		{
			gameOverLabel.Visible = true;
			gameOverLabel.Text = "GAME OVER";
		}

		// Sau vài giây quay về Menu
		GetTree().CreateTimer(3.0).Timeout += () =>
		{
			GetTree().ChangeSceneToFile("res://Scene/main_menu.tscn");
		};
	}
	private void Respawn()
	{
		currentHP = maxHP;
		Velocity = Vector2.Zero;

		var spawnPoint = GetNodeOrNull<Marker2D>("../SpawnPoint");
		if (spawnPoint != null)
		{
			GlobalPosition = spawnPoint.GlobalPosition;
		}
		else
		{
			GD.PrintErr("Không tìm thấy SpawnPoint!");
			GlobalPosition = new Vector2(100, 100);
		}

		SetPhysicsProcess(true);
		isDead = false; // reset trạng thái chết

		GD.Print("Player respawned. Lives: " + lives + ", HP reset to " + currentHP);
	}
	private void HandleDeath()
	{
		if (isDead) return;

		isDead = true;
		PlayAnim("Dead");
		Velocity = Vector2.Zero;
		SetPhysicsProcess(false);

		// Sau 3 giây mới xử lý tiếp
		GetTree().CreateTimer(3.0).Timeout += () =>
		{
			lives--;
			UpdateLivesUI();

			if (lives > 0)
			{
				currentHP = maxHP;
				Respawn();
			}
			else
			{
				GameOver();
			}
		};
	}
}
