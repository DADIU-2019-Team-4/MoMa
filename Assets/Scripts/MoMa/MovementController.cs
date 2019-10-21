
using Rewired;
using System.Collections.Generic;
using UnityEngine;

namespace MoMa
{
    public class MovementController : MonoBehaviour
    {
        public const float DefaultDampTime = 5f;
        public const float StopDampTime = 2f;
        public const float Speed = 3f;
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

            // Simulate stap (modifies the velocity)
            SimulateStep(
                transform.position,
                _direction,
                ref _velocity
                );

            // Move to target position
            transform.position += _velocity * Speed;

            // Rotate to face the direction moving
            Vector2 direction = _velocity.GetXZVector2().normalized;
            float rotationAngle = Vector2.SignedAngle(Vector2.up, direction);
            _model.eulerAngles = new Vector3(0, -rotationAngle, 0);
        }

        public List<Vector3> GetFuture(int afterFrames)
        {
            // Initialize the simulated position to the value of the current one
            Vector3 simulatedPosition = new Vector3(
                transform.position.x,
                transform.position.y,
                transform.position.z
                );
            Vector3 simulatedVelocity = new Vector3(_velocity.x, _velocity.y, _velocity.z);
            List<Vector3> future = new List<Vector3>();

            for (int i = 0; i < afterFrames; i++)
            {
                // Calculate next position
                SimulateStep(
                    simulatedPosition,
                    _direction,
                    ref simulatedVelocity
                    );
                simulatedPosition += simulatedVelocity * Speed;

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

        private void SimulateStep(Vector3 current, Vector3 inputVector, ref Vector3 currentVelocity)
        {
            Vector3.SmoothDamp(
                current,
                current + inputVector,
                ref currentVelocity,
                inputVector == Vector3.zero ?
                    StopDampTime :
                    DefaultDampTime
                );
        }
    }
}