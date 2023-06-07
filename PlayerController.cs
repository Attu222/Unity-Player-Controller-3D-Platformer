using UnityEngine;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private float _moveSpeed = 6f;
	[SerializeField] private float _runSpeed = 12f;
	[SerializeField] private float _rotationSpeed = 10f;
	[SerializeField] private float _moveSpeedChangeRate = 10f;
	[SerializeField] private float _runSpeedChangeRate = 10f;
	[SerializeField] private float _gravity = -30f;
	[SerializeField] private float _jumpHeight = 2f;
	[SerializeField] private int _extraJumps = 1;
	[SerializeField] private float _timeJumpIfNoGrounded = 0.25f;

	[SerializeField] private Transform _body;

	[SerializeField] private LayerMask _layerMaskGrounded;
	[SerializeField] private float _offsetGroundDetector = 0.8f;
	[SerializeField] private float _groundDetectorRadius = 0.4f;

	[SerializeField] private Transform _cameraTarget;
	[SerializeField] private InputReader _inputReader;

	private CharacterController _controller;

	private Vector2 _inputMove;
	private Vector3 _velocity;

	private float _verticalVelocity;
	private float _speed;
	private bool _isGrounded;
	private int _currentJump;
	private float _timeJump;
	private float _targetSpeed;
	private float _speedChangeRate;
	private float _targetRotation;
	private bool _isJump;
	private bool _isEventMove;
	private bool _isEventRun;

	public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
	public float RunSpeed { get => _runSpeed; set => _runSpeed = value; }
	public float RotationSpeed { get => _rotationSpeed; set => _rotationSpeed = value; }
	public float MoveSpeedChangeRate { get => _moveSpeedChangeRate; set => _moveSpeedChangeRate = value; }
	public float RunSpeedChangeRate { get => _runSpeedChangeRate; set => _runSpeedChangeRate = value; }
	public float Gravity1 { get => _gravity; set => _gravity = value; }
	public float JumpHeight { get => _jumpHeight; set => _jumpHeight = value; }
	public int ExtraJumps { get => _extraJumps; set => _extraJumps = value; }
	public float TimeJumpIfNoGrounded { get => _timeJumpIfNoGrounded; set => _timeJumpIfNoGrounded = value; }
	public Vector3 Velocity { get => _velocity; set => _velocity = value; }
	public float Speed { get => _speed; }
	public float SpeedChangeRate { get => _speedChangeRate; }
	
	public event Action<GroundInfo> GroundedEvent;
	public event Action EndGroundedEvent;
	public event Action JumpEvent;
	public event Action DoubleJumpEvent;
	public event Action RunEvent;
	public event Action WalkEvent;
	public event Action IdleEvent;

	public struct GroundInfo
	{
		public Vector3 point;
		public Vector3 normal;
		public Vector3 velocity;
		public Collider collider;
	}

	private void Awake()
	{
		_controller = GetComponent<CharacterController>();
		_currentJump = _extraJumps;
	}

	private void OnEnable()
	{
		_inputReader.OnMoveEvent += OnMove;
		_inputReader.OnJumpEvent += OnJump;
		_inputReader.OnRunPressEvent += OnRun;
		_inputReader.OnRunReleaseEvent += OnWalk;
	}

	private void OnDisable()
	{
		_inputReader.OnMoveEvent -= OnMove;
		_inputReader.OnJumpEvent -= OnJump;
		_inputReader.OnRunPressEvent -= OnRun;
		_inputReader.OnRunReleaseEvent -= OnWalk;
	}

	private void Start()
	{
		_targetSpeed = _moveSpeed;
		_speedChangeRate = _moveSpeedChangeRate;
	}

	private void Update()
	{
		Grounded();
		Gravity();
		Move();
		EventsMove();

		_velocity.y = _verticalVelocity;
		_controller.Move(_velocity * Time.deltaTime);
	}

	private void Grounded()
	{
		if (_isGrounded && !_controller.isGrounded && !_isJump)
		{
			_timeJump = Time.time + _timeJumpIfNoGrounded;
			EndGroundedEvent?.Invoke();
			return;
		}

		if (_isGrounded || !_controller.isGrounded) return;
		
		RaycastHit _hitInfoGround = new RaycastHit();
		bool isHit = Physics.SphereCast(transform.position + _controller.center, _groundDetectorRadius, Vector3.down, out _hitInfoGround, _offsetGroundDetector, _layerMaskGrounded);
		GroundInfo groundenInfo = new GroundInfo();
		if (isHit)
		{
			groundenInfo.point = _hitInfoGround.point;
			groundenInfo.normal = _hitInfoGround.normal;
			groundenInfo.collider = _hitInfoGround.collider;
			groundenInfo.velocity = _velocity;
		}
		GroundedEvent?.Invoke(groundenInfo);
	}

	private void Gravity()
	{
		_verticalVelocity += _gravity * Time.deltaTime;
		_isGrounded = _controller.isGrounded;

		if (!_isGrounded || _verticalVelocity >= 0) return;

		_isJump = false;
		_currentJump = _extraJumps;
		_verticalVelocity = -5f;
	}

	private void Move()
	{
		if(_inputMove.magnitude > 0f)
			_targetRotation = Mathf.Atan2(_inputMove.x, _inputMove.y) * Mathf.Rad2Deg + _cameraTarget.transform.eulerAngles.y;
		
		_body.rotation = Quaternion.Lerp(_body.rotation, Quaternion.Euler(0, Mathf.Round(_targetRotation * 1000f) / 1000f, 0), _rotationSpeed * Time.deltaTime);

		if (_inputMove == Vector2.zero) _speed = 0.0f;
		else _speed = _targetSpeed;

		if (!_isGrounded && _inputMove.magnitude <= 0f) return;
		
		Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
		Vector3 targetVelocity = targetDirection.normalized * _speed;
		_velocity = Vector3.Lerp(_velocity, targetVelocity, _speedChangeRate * Time.deltaTime);
		_velocity.x = Mathf.Round(_velocity.x * 1000f) / 1000f;
		_velocity.z = Mathf.Round(_velocity.z * 1000f) / 1000f;
	}
	
	private void EventsMove()
	{
		if (!_isGrounded) return;
		
		if (_inputMove.magnitude <= 0f)
		{
			if (_isEventMove || _isEventRun)
			{
				_isEventMove = false;
				_isEventRun = false;
				IdleEvent?.Invoke();
			}
			return;
		}

		if (_targetSpeed == _moveSpeed && !_isEventMove)
		{
			_isEventRun = false;
			_isEventMove = true;
			WalkEvent?.Invoke();
			return;
		}

		if (_targetSpeed == _runSpeed && !_isEventRun)
		{
			_isEventRun = true;
			_isEventMove = false;
			RunEvent?.Invoke();
		}
	}

	private void OnMove(Vector2 move)
	{
		_inputMove = move;
	}

	private void OnJump()
	{
		if (!_isGrounded && _timeJump < Time.time && _currentJump <= 0) return;

		_verticalVelocity = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
		_isJump = true;

		if (_isGrounded || _timeJump >= Time.time)
		{
			JumpEvent?.Invoke();
			_timeJump = 0;
			return;
		}

		_currentJump--;
		DoubleJumpEvent?.Invoke();
		_timeJump = 0;
	}

	private void OnRun()
	{
		_targetSpeed = _runSpeed;
		_speedChangeRate = _runSpeedChangeRate;
	}

	private void OnWalk()
	{
		_targetSpeed = _moveSpeed;
		_speedChangeRate = _moveSpeedChangeRate;
	}

	private void OnDrawGizmosSelected()
	{
		Vector3 _groundDetectorPosition;
		_controller = GetComponent<CharacterController>();

		_groundDetectorPosition = transform.position + _controller.center;
		_groundDetectorPosition.y -= _offsetGroundDetector;

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(_groundDetectorPosition, _groundDetectorRadius);
	}
}
