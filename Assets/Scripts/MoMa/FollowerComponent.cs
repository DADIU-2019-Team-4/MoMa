using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoMa
{
    public class FollowerComponent
    {
        private const string PlayerTag = "Player"; // The Tag that the Player's GameObject has in the game

        private Transform _model;
        private List<GameObject> _dots = new List<GameObject>();

        private GameObject CreateDot()
        {
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            return dot;
        }

        public FollowerComponent(Transform model)
        {
            this._model = model;
        }

        public void Draw(Trajectory.Snippet snippet)
        {
            foreach (GameObject dot in _dots)
            {
                GameObject.Destroy(dot);
            }

            _dots.Clear();

            foreach (Trajectory.Point point in snippet.points)
            {
                GameObject dot = CreateDot();
                dot.transform.position = new Vector3(
                    point.x + this._model.position.x,
                    0,
                    point.z + this._model.position.z
                    );
                dot.transform.rotation = this._model.rotation;
                _dots.Add(dot);
            }
        }
    }
}
