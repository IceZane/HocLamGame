using Godot;
using System;

public partial class MainCharacter : CharacterBody2D
{
	// ====== MOVEMENT SETTINGS ======
	public const float WalkSpeed = 100f;
	public const float RunSpeed = 250f;
	public const float JumpVelocity = -250f;

	private float currentSpeed = WalkSpeed;

	// ====== COMPONENTS ======
	private AnimatedSprite2D anim;

	// ====== STATE FLAGS ======
	private bool isAttacking = false;
	private bool isReloading = false;
	private bool isShooting = false;
	private bool isHurt = false;
	private bool isDead = false;

	// Example ammo for shooting
	public const int Capacity = 30;  // Súng chứa tối đa 30 viên

// Biến private lưu trữ dữ liệu thực tế
	private int currentAmmo = 30;    
	private int maxAmmo = 120;       

// Property public chỉ đọc (hoặc có thể thêm set nếu muốn)
	public int CurrentAmmo => currentAmmo;
	public int MaxAmmo => maxAmmo;
	
	private int currentHP = 4;
	private const int maxHP = 4;
	public int GetHP() => currentHP;



	public override void _Ready()
	{
		anim = GetNode<AnimatedSprite2D>("Sprite2D");
		AddToGroup("player");

		// Event: Animation finished
		anim.AnimationFinished += OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
{
	Vector2 velocity = Velocity;

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
	if (Input.IsActionJustPressed("ui_accept") && IsOnFloor() && !isDead)
		velocity.Y = JumpVelocity;

	// Run or Walk
	float dir = Input.GetAxis("ui_left", "ui_right");
	bool running = Input.IsActionPressed("run");
	currentSpeed = running ? RunSpeed : WalkSpeed;
	velocity.X = dir * currentSpeed;

	// Flip sprite
	if (dir != 0)
		anim.FlipH = dir < 0;
	Velocity = velocity;
	MoveAndSlide();
	

	// =============================
	// Combat input
	// =============================
	if (!isDead && !isShooting && !isAttacking && !isReloading)
	{
		if (Input.IsActionJustPressed("shoot") && currentAmmo > 0)
		{
			isShooting = true;
			ShootBullet();
			PlayAnim("Shooting"); // animation chạy 1 lần
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
	// Movement animation only if not in combat
	// =============================
	
if (!isShooting && !isAttacking && !isReloading && !isHurt && !isDead)
{
	if (!IsOnFloor())
		PlayAnim("Jump");
	else if (Mathf.Abs(Velocity.X) > 0 && Input.IsActionPressed("run"))
		PlayAnim("Run");
	else if (Mathf.Abs(Velocity.X) > 10)
		PlayAnim("Walk");
	else
		PlayAnim("default");
}

}
private void PlayAnim(string name)
{
	if (anim == null)
		return; // phòng trường hợp anim chưa được gán

	// Chỉ play nếu khác animation hiện tại
	if (anim.Animation != name)
	{
		try
		{
			anim.Play(name);
		}
		catch
		{
			GD.PrintErr($"Animation '{name}' does not exist!");
		}
	}
}


// Called when an animation finishes
private void OnAnimationFinished()
{
	switch (anim.Animation)
	{
		case "Shooting":
			isShooting = false;
			PlayAnim("default");
			break;

		case "Attack":
			isAttacking = false;
			PlayAnim("default");
			break;

		case "Reload":
			isReloading = false;
			PlayAnim("default");
			break;

		case "Hurt":
			isHurt = false;
			PlayAnim("default");
			break;

		case "Dead":
			// do nothing
			break;
	}
}



	// ======================================
	//       COMBAT METHODS
	// ======================================
	private void DoAttack()
{
	GD.Print("Attack executed!");

	// Tìm tất cả zombie trong scene
	var zombies = GetTree().GetNodesInGroup("zombie");

	foreach (var node in zombies)
	{
		if (node is ZombieEnemy zombie)
		{
			// Khoảng cách đánh cận chiến (tùy bạn chỉnh)
			float attackRange = 20f;

			if (GlobalPosition.DistanceTo(zombie.GlobalPosition) <= attackRange)
			{
				zombie.TakeDamage(1);
				GD.Print("Hit zombie!");
			}
		}
	}
}


	public void DoReload()
	{
		int missingAmmo = Capacity - currentAmmo; // số đạn cần để đầy súng
		int ammoToLoad = Mathf.Min(missingAmmo, maxAmmo); // không vượt quá dự trữ
		currentAmmo += ammoToLoad;
		maxAmmo -= ammoToLoad;

		GD.Print($"Reload executed! CurrentAmmo: {currentAmmo}, MaxAmmo: {maxAmmo}");
	}

	private void ShootBullet()
	{
		// Thêm logic bắn đạn ở đây
		currentAmmo--;
		GD.Print("Shoot executed! Ammo left: " + currentAmmo);
	}

	// ======================================
	//       EXTERNAL DAMAGE METHODS
	// ======================================
	public void TakeDamage()
	{
		if (isDead) return;

		currentHP = Mathf.Max(currentHP - 1, 0);
		PlayAnim("Hurt");

		UpdateHealthBar();

		if (currentHP <= 0)
			Die();
	}

	public void Die()
{
	if (isDead) return;

	isDead = true;
	PlayAnim("Dead");

	Velocity = Vector2.Zero;
	SetPhysicsProcess(false);

	// Tìm node HUD đúng theo cấu trúc scene
	var ui = GetNode<CanvasLayer>("../HUD");
	ui.Visible = false;

	GD.Print("Player has died.");
}

	
	private void UpdateHealthBar()
	{
		var healthBars = GetTree().GetNodesInGroup("healthbar");
		if (healthBars.Count == 0)
			return;

		var bar = healthBars[0] as AnimatedSprite2D;

		string animName = currentHP switch
		{
			0 => "0 HP",
			1 => "1 HP",
			2 => "2 HP",
			3 => "3 HP",     // nếu bạn chưa có "3 HP", dùng lại "2 HP"
			4 => "Full HP",
			_ => "0 HP"
		};

		bar.Animation = animName;
		bar.Frame = 0;
		bar.Stop();
	}
}
