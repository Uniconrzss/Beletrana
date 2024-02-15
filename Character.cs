using Godot;
using System;
using System.Reflection;

public partial class Character : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;
	public const float mouseSensitivity = 0.005f;
	public Node3D neck;
	public Camera3D camera;

	public PackedScene bulletScene;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	

	public override void _Ready()
	{
		neck = (Node3D)this.GetChild(2);	
		camera = (Camera3D)neck.GetChild(0);
		bulletScene = GD.Load<PackedScene>("res://Bullet.tscn");
	}
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
		if (@event is InputEventMouseButton)
		{
			Input.MouseMode = Input.MouseModeEnum.Captured;
			

		}
		else if (@event.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		if (Input.MouseMode == Input.MouseModeEnum.Captured)
		{
			if (@event is InputEventMouseMotion)
			{
				InputEventMouseMotion mouseMotion = (InputEventMouseMotion)@event;
				neck.RotateY(-mouseMotion.Relative.X * mouseSensitivity);
				float intendedYMovement = -mouseMotion.Relative.Y * mouseSensitivity;
				if (camera.Rotation.X > -1.3 && camera.Rotation.X < 1.3 && camera.Rotation.X+intendedYMovement > -1.3 && camera.Rotation.X+intendedYMovement < 1.3)
				{
					camera.RotateX(intendedYMovement);
				}
			}
		}
    }
    
	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
			velocity.Y -= gravity * (float)delta;

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
			velocity.Y = JumpVelocity;

		if (Input.IsActionJustPressed("shoot"))
			Shoot();

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forwards", "move_backwards");
		Vector3 direction = (neck.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	public void Shoot()
	{
		//Fire Bullet
		RigidBody3D bullet = (RigidBody3D)bulletScene.Instantiate();

		Node3D game = (Node3D)GetParent();
		game.AddChild(bullet);

		bullet.Position = this.Position + new Vector3(0,0.5f,0);
		bullet.Rotation = new Vector3(camera.Rotation.X,neck.Rotation.Y,1.2f);

		Vector2 motionFB = new Vector2(0,-1);
		Vector3 bulletDirection = (bullet.Transform.Basis * new Vector3(motionFB.X, 0, motionFB.Y)).Normalized();
		bullet.Position += bulletDirection;
		float bulletSpeed = 30f;

		if (bulletDirection != Vector3.Zero)
		{
			bullet.SetAxisVelocity(new Vector3(bulletDirection.X * bulletSpeed,0,bulletDirection.Z * bulletSpeed));
		}
		else
		{
			bullet.SetAxisVelocity(new Vector3(Mathf.MoveToward(bullet.LinearVelocity.X, 0, 1),0,Mathf.MoveToward(bullet.LinearVelocity.Z, 0, 1)));
		}
	}
}
