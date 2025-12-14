using Godot;
using System;

public partial class ZombieEnemy : CharacterBody2D
{
	[Export] public float Speed = 20f;
	[Export] public int MaxHP = 3;
	[Export] public float AttackCooldown = 1.5f;

	private int currentHP;
	private AnimatedSprite2D anim;
	private Node2D player;
	private Timer attackTimer;
	private bool isPlayerInRange = false;
	private float gravity = 400f;

	private int patrolDirection = -1; // -1: trái, 1: phải

	public override void _Ready()
	{
		AddToGroup("zombie");

		currentHP = MaxHP;
		anim = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		// Tìm player theo group
		if (player == null)
		{
			var players = GetTree().GetNodesInGroup("player");
			if (players.Count > 0)
				player = players[0] as Node2D;
		}

		// Timer để cooldown tấn công
		attackTimer = new Timer();
		attackTimer.WaitTime = AttackCooldown;
		attackTimer.OneShot = true;
		AddChild(attackTimer);

		// Kết nối Area2D
		var area = GetNode<Area2D>("Area2D");
		area.BodyEntered += OnBodyEntered;
		area.BodyExited += OnBodyExited;
	}

	public override void _PhysicsProcess(double delta)
	{
		var rayLeft = GetNode<RayCast2D>("RayDownLeft");
		var rayRight = GetNode<RayCast2D>("RayDownRight");
		var rayForwardLeft = GetNode<RayCast2D>("RayForwardLeft");
		var rayForwardRight = GetNode<RayCast2D>("RayForwardRight");

		bool hasGroundLeft = rayLeft.IsColliding();
		bool hasGroundRight = rayRight.IsColliding();
		bool isBlockedLeft = rayForwardLeft.IsColliding();
		bool isBlockedRight = rayForwardRight.IsColliding();

		// Nếu đi trái mà không có gạch hoặc bị chặn → quay phải
		if (!isPlayerInRange)
		{
			if (patrolDirection == -1 && (!hasGroundLeft || isBlockedLeft))
				patrolDirection = 1;

			if (patrolDirection == 1 && (!hasGroundRight || isBlockedRight))
				patrolDirection = -1;
		}

		if (currentHP <= 0)
			return;

		if (player == null)
		{
			var players = GetTree().GetNodesInGroup("player");
			if (players.Count > 0)
				player = players[0] as Node2D;
		}

		Vector2 velocity = Velocity;
		velocity.Y += gravity * (float)delta;

		if (isPlayerInRange && player != null)
		{
			Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
			velocity.X = direction.X * Speed;
			anim.FlipH = direction.X < 0;
			anim.Play("Walk");

			if (GlobalPosition.DistanceTo(player.GlobalPosition) < 20f)
				TryAttack(player);
		}
		else
		{
			velocity.X = patrolDirection * Speed;
			anim.FlipH = patrolDirection < 0;
			anim.Play("Walk");
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void OnBodyEntered(Node body)
	{
		if (body.IsInGroup("player"))
			isPlayerInRange = true;
	}

	private void OnBodyExited(Node body)
	{
		if (!IsInsideTree()) return;
		if (body.IsInGroup("player"))
			isPlayerInRange = false;
	}

	private void TryAttack(Node body)
	{
		if (attackTimer.IsStopped())
		{
			// Tầm đánh (có thể chỉnh tuỳ ý, ví dụ 40f)
			float attackRange = 40f;

			if (body is MainCharacter playerChar)
			{
				// Kiểm tra khoảng cách
				float distance = GlobalPosition.DistanceTo(playerChar.GlobalPosition);

				if (distance <= attackRange)
				{
					attackTimer.Start();
					anim.Play("Attack");

					// Tính hướng từ zombie tới player
					Vector2 attackDirection = (playerChar.GlobalPosition - GlobalPosition).Normalized();

					// Gọi TakeDamage với hướng knockback
					playerChar.TakeDamage(attackDirection);

					GD.Print("Zombie attacked player!");
				}
			}
		}
	}

	public void TakeDamage(int amount, Vector2 attackDirection)
	{
		currentHP -= amount;

		if (currentHP > 0)
		{
			anim.Play("Hurt");
			FlashRed(); // chớp đỏ
			ApplyKnockback(attackDirection, 150f); // bật lùi

			SetPhysicsProcess(false);
			GetTree().CreateTimer(0.3).Timeout += () =>
			{
				SetPhysicsProcess(true);
				anim.Play("Walk");
			};
		}
		else
		{
			Die();
		}
	}

	private void Die()
	{
		Velocity = Vector2.Zero;
		anim.Play("Dead");
		SetProcess(false);
		SetPhysicsProcess(true);

		GetTree().CreateTimer(2).Timeout += () =>
		{
			QueueFree();
		};
	}
	private async void FlashRed()
	{
		anim.Modulate = new Color(1, 0, 0); // đỏ
		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
		anim.Modulate = Colors.White;       // trả lại màu
	}

	public void ApplyKnockback(Vector2 direction, float force)
	{
		Velocity = direction.Normalized() * force;
		MoveAndSlide();
	}
}
