using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace MoMa
{
    public class CharacterController : MonoBehaviour
    {
        private MovementComponent _mc;
        private FollowerComponent _fc;
        private RuntimeComponent _rc;
        private AnimationComponent _ac;
        private Trajectory _trajectory = new Trajectory();
        private Transform _model;
        private int currentFrame = 0;

        void Start()
        {
            // We assume that the Character has the correct structure
            Transform character = this.gameObject.transform;
            this._model = character.GetChild(0).transform.GetChild(0);
            this._mc = new MovementComponent(character);
            this._fc = new FollowerComponent(this._model);
            this._rc = new RuntimeComponent();
            this._ac = new AnimationComponent(this._model);

            // Initialize Trajectory's past to the initial position
            for (int i = 0; i < Trajectory.Snippet.PastPoints; i++)
            {
                this._trajectory.points.Add(new Trajectory.Point(0f, 0f));
            }
        }

        void FixedUpdate()
        {
            Debug.Log("Update()");

            // Update MovementComponent
            _mc.Update();

            // When a new Trajectory.Point is reached, add it to history. Remove the oldest past point
            if (currentFrame % Feature.FramesPerPoint == 0)
            {
                this._trajectory.points.Add(new Trajectory.Point(this._model.position.x, this._model.position.z));
                this._trajectory.points.RemoveAt(0);

                // Reset current Frame
                currentFrame = 0;
            }

            currentFrame++;

            // When Animation.Clip is over, request a new one and draw the Trajectory.Snippet
            if (_ac.IsOver())
            {
                // Find and load next Animation.Clip
                Trajectory.Snippet snippet = GetCurrentSnippet();
                _ac.LoadClip(this._rc.QueryClip(snippet));

                // Draw current Trajectory.Snippet
                _fc.Draw(snippet);
            }

            // Play Animation.Frame
            _ac.Step();
        }

        private Trajectory.Snippet GetCurrentSnippet()
        {
            Trajectory.Snippet snippet;
            int futureFramesNumber = Feature.FramesPerPoint * Trajectory.Snippet.FuturePoints;

            // Get simulated future
            List<Vector3> futureFrames = this._mc.GetFuture(futureFramesNumber);

            // Convert the (many) Frames to (few) Point and add them to the Trajectory
            for (int i = 0; i < Trajectory.Snippet.FuturePoints; i++)
            {
                Trajectory.Point point = Trajectory.Point.getMedianPoint(futureFrames.GetRange(i * Feature.FramesPerPoint, Feature.FramesPerPoint));
                //Trajectory.Point point = new Trajectory.Point(futureFrames[i * Feature.FramesPerPoint + Feature.FramesPerPoint / 2].GetXZVector2());
                this._trajectory.points.Add(point);
            }

            // Compute the Trajectory Snippet
            snippet = this._trajectory.GetLocalSnippet(
                Trajectory.Snippet.PastPoints,
                this._model.position,
                this._model.rotation
                );

            // Remove future Points from Trajectory
            this._trajectory.points.RemoveRange(Trajectory.Snippet.PastPoints, Trajectory.Snippet.FuturePoints);

            return snippet;
        }
    }
}
