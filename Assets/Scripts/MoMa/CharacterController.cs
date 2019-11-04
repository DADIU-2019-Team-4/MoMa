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
            this._model = character.GetChild(0);
            this._mc = new MovementComponent(character);
            this._fc = new FollowerComponent(this._model);
            this._rc = new RuntimeComponent(this._fc);
            this._ac = new AnimationComponent(this._model.GetChild(0));

            // Initialize Trajectory's past to the initial position
            for (int i = 0; i < RuntimeComponent.FeaturePastPoints; i++)
            {
                this._trajectory.points.Add(new Trajectory.Point(new Vector2(0f, 0f)));
            }
        }

        void FixedUpdate()
        {
            // Update MovementComponent
            _mc.Update();

            // Add Point to Trajectory, removing the oldest point
            if (currentFrame % RuntimeComponent.FramesPerPoint == 0)
            {
                this._trajectory.points.Add(new Trajectory.Point(new Vector2(this._model.position.x, this._model.position.z)));
                this._trajectory.points.RemoveAt(0);

                // Reset current Frame
                currentFrame = 0;
            }

            currentFrame++;

            // Load new Animation.Clip
            if (_ac.IsOver())
            {
                // Find and load next Animation.Clip
                Trajectory.Snippet snippet = GetCurrentSnippet();
                _ac.LoadClip(this._rc.QueryClip(snippet));

                // Draw current Trajectory.Snippet
                _fc.DrawPath(snippet);

                //Debug.Log(this._trajectory);
                //Debug.Log(snippet);
            }

            // Play Animation.Frame
            _ac.Step();
        }

        private Trajectory.Snippet GetCurrentSnippet()
        {
            Trajectory.Snippet snippet;
            int futureFramesNumber = RuntimeComponent.FramesPerPoint * RuntimeComponent.FeaturePoints;

            // Get simulated future
            List<Vector3> futureFrames = this._mc.GetFuture(futureFramesNumber);

            // Convert the (many) Frames to (few) Point and add them to the Trajectory
            for (int i = 0; i < RuntimeComponent.FeaturePoints; i++)
            {
                //Trajectory.Point point = Trajectory.Point.getMedianPoint(futureFrames.GetRange(i * Trajectory.FramesPerPoint, Trajectory.FramesPerPoint));
                //Trajectory.Point point = new Trajectory.Point(futureFrames[i * Feature.FramesPerPoint + Feature.FramesPerPoint / 2].GetXZVector2());
                Trajectory.Point point = new Trajectory.Point(futureFrames[ (i+1) * RuntimeComponent.FramesPerPoint - 1].GetXZVector2());
                this._trajectory.points.Add(point);
            }

            // Compute the Trajectory Snippet
            snippet = this._trajectory.GetLocalSnippet(
                RuntimeComponent.FeaturePastPoints - 1,
                this._model.rotation
                );

            // Remove future Points from Trajectory
            this._trajectory.points.RemoveRange(RuntimeComponent.FeaturePastPoints, RuntimeComponent.FeaturePoints);

            return snippet;
        }
    }
}
