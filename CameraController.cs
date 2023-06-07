using UnityEngine;

public class CameraController : MonoBehaviour
{
	[SerializeField] private float _borderUp = 65f;
	[SerializeField] private float _borderDown = -10f;
	[SerializeField] private InputReader _inputReader;

	private Vector3 _rotation;
	private Vector2 _inputLook;

	private bool IsMouse
	{
		get
		{
			return _inputReader.PlayerInput.currentControlScheme == "KeyboardMouse";
		}
	}

	private void OnEnable()
	{
		_inputReader.OnLookEvent += OnLook;
	}

	private void OnDisable()
	{
		_inputReader.OnLookEvent -= OnLook;
	}

	private void Update()
	{
		CameraRotation();
	}

	private void CameraRotation()
	{
		_rotation.x += _inputLook.y * (IsMouse ? 1f : Time.deltaTime);
		_rotation.y += _inputLook.x * (IsMouse ? 1f : Time.deltaTime);
		_rotation.z = transform.rotation.eulerAngles.z;

		if (_rotation.y > 360f) _rotation.y -= 360f;
		else if (_rotation.y < 360f) _rotation.y += 360f;

		if (_rotation.x > _borderUp) _rotation.x = _borderUp;
		else if (_rotation.x < _borderDown) _rotation.x = _borderDown;

		transform.rotation = Quaternion.Euler(_rotation);
	}

	private void OnLook(Vector2 inputLook)
	{
		_inputLook = inputLook;
	}
}
