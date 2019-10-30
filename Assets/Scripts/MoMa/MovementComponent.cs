
using Rewired;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoMa
{
    public class MovementComponent
    {
        public const float DefaultDampTime = 2f;
        public const float StopDampTime = 3f;
        public const float Speed = 3f;
        public const string ModelName = "Model";

        public int playerId = 0;

        private Transform _transform;
        [SerializeField]
        private Vector3 _velocity = new Vector3();
        private Vector3 _direction = new Vector3(1, 0, 0);
        private Player _player;
        private Transform _model;

        public MovementComponent(Transform transform)
        {
            this._transform = transform;
            _player = ReInput.players.GetPlayer(playerId);
            _model = _transform.Find(ModelName);
        }

        // Update is called once per frame
        public void Update()
        {
            // Get current input
            _direction.x = _player.GetAxisRaw("Move Horizontal");
            _direction.z = _player.GetAxisRaw("Move Vertical");
            _direction.Normalize();

            // Move to target position (modifies the velocity)
            _transform.position = Step(
                _transform.position,
                _direction,
                ref _velocity
                );

            // Rotate to face the direction moving
            Vector2 direction = _velocity.GetXZVector2().normalized;
            float rotationAngle = Vector2.SignedAngle(Vector2.up, direction);
            _model.eulerAngles = new Vector3(0, -rotationAngle, 0);
        }

        public List<Vector3> GetFuture(int afterFrames)
        {
            // Initialize the simulated position and velocity to the value of the current ones
            Vector3 simulatedPosition = _transform.position;
            Vector3 simulatedVelocity = _velocity;
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
                future.Add(simulatedPosition);
            }

            return future;
        }

        private Vector3 Step(Vector3 current, Vector3 inputVector, ref Vector3 currentVelocity)
        {
            Vector3 destination = Vector3.SmoothDamp(
                current,
                current + inputVector * Speed,
                ref currentVelocity,
                inputVector == Vector3.zero ?
                    StopDampTime :
                    DefaultDampTime
                );

            //currentVelocity.x = (float)Math.Round(currentVelocity.x, 3);
            //currentVelocity.y = (float)Math.Round(currentVelocity.y, 3);
            //currentVelocity.z = (float)Math.Round(currentVelocity.z, 3);

            return destination;
        }
    }
}