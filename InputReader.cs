using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(PlayerInput))]
public class InputReader : MonoBehaviour
{
	public event Action<Vector2> OnLookEvent;
	public event Action<Vector2> OnMoveEvent;
	public event Action OnJumpEvent;
	public event Action OnRunPressEvent;
	public event Action OnRunReleaseEvent;

	private PlayerInput _playerInput;

	public PlayerInput PlayerInput { get => _playerInput; }

	private void Awake()
	{
		_playerInput = GetComponent<PlayerInput>();
	}

	public void OnLook(InputAction.CallbackContext context)
	{
		OnLookEvent?.Invoke(context.ReadValue<Vector2>());
	}

	public void OnMove(InputAction.CallbackContext context)
	{
		OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.phase == InputActionPhase.Performed)
			OnJumpEvent?.Invoke();
	}

	public void OnRun(InputAction.CallbackContext context)
	{
		if (context.phase == InputActionPhase.Started)
		{
			OnRunPressEvent?.Invoke();
		}

		if (context.phase == InputActionPhase.Canceled)
		{
			OnRunReleaseEvent?.Invoke();
		}
	}
}
