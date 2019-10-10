using Rewired;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementController : MonoBehaviour
{
    public int playerId = 0;
    [SerializeField]
    private float _speed = 3f;

    private Vector3 _direction;
    private Player _player;
    private CharacterController _characterController;

    void Awake()
    {
        _player = ReInput.players.GetPlayer(playerId);
        _characterController = GetComponent<CharacterController>();
    }

    private void HandleInput()
    {
        _direction.x = _player.GetAxisRaw("Move Horizontal");
        _direction.z = _player.GetAxisRaw("Move Vertical");

        //_characterController.Move(_direction * _speed * Time.deltaTime);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    public Vector3 GetInput()
    {
        return _direction;
    }
}
