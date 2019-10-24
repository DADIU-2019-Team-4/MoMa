
using Rewired;
using System.Collections.Generic;
using UnityEngine;

namespace MoMa
{
    public class MovementController : MonoBehaviour
    {
        public const float DefaultDampTime = 2f;
        public const float StopDampTime = 2f;
        public const float Speed = 7f;
        public const float MaxSpeed = 8.5f;
        public const string ModelName = "Model";

        public int playerId = 0;

        [SerializeField]
        private Vector3 _velocity = new Vector3();
        private Vector3 _direction;
        private Player _player;
        private Transform _model;

        void Awake()
        {
            _player = ReInput.players.GetPlayer(playerId);
            _model = transform.Find(ModelName);
        }

        // Update is called once per frame
        void Update()
        {
            // Get current input
            _direction.x = _player.GetAxisRaw("Move Horizontal");
            _direction.z = _player.GetAxisRaw("Move Vertical");
            _direction.Normalize();

            Debug.Log("direction = " + _direction);

            // Move to target position (modifies the velocity)
            transform.position = Step(
                transform.position,
                _direction,
                ref _velocity
                );

            Debug.Log("_velocity = " + _velocity + " magn: " + _velocity.magnitude);

            // Rotate to face the direction moving
            Vector2 direction = _velocity.GetXZVector2().normalized;
            float rotationAngle = Vector2.SignedAngle(Vector2.up, direction);
            _model.eulerAngles = new Vector3(0, -rotationAngle, 0);
        }

        public List<Vector3> GetFuture(int afterFrames)
        {
            // Initialize the simulated position and velocity to the value of the current ones
            Vector3 simulatedPosition = new Vector3(
                transform.position.x,
                transform.position.y,
                transform.position.z
                );
            Vector3 simulatedVelocity = new Vector3(
                _velocity.x,
                _velocity.y,
                _velocity.z
                );
            List<Vector3> future = new List<Vector3>();

            for (int i = 0; i < afterFrames; i++)
            {
                // Calculate next position
                simulatedPosition = Step(
                    simulatedPosition,
                    _direction,
                    ref simulatedVelocity
                    );

                // Add it to the list
                future.Add(
                    new Vector3(
                        simulatedPosition.x,
                        simulatedPosition.y,
                        simulatedPosition.z
                        )
                    );
            }

            return future;
        }

        private Vector3 Step(Vector3 current, Vector3 inputVector, ref Vector3 currentVelocity)
        {
            return Vector3.SmoothDamp(
                current,
                current + inputVector * Speed,
                ref currentVelocity,
                inputVector == Vector3.zero ?
                    StopDampTime :
                    DefaultDampTime,
                MaxSpeed
                );
        }
    }
}