using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoMa
{
    [RequireComponent(typeof(MovementController))]
    public class Follower : MonoBehaviour
    {
        private const string PlayerTag = "Player"; // The Tag that the Player's GameObject has in the game
        
        public GameObject Dot;

        private Transform _model;
        private List<GameObject> _dots = new List<GameObject>();

        public void Start()
        {
            GameObject go = GameObject.FindWithTag(PlayerTag);

            if (go == null)
            {
                Debug.LogError("Unable to find Model or MovementController");
                throw new Exception("Unable to find Model or MovementController");
            }
            else
            {
                this._model = go.transform.GetChild(0);
            }
        }

        public void Draw(Trajectory.Snippet snippet)
        {
            //Debug.DrawLine();

            foreach (GameObject dot in _dots)
            {
                Destroy(dot);
            }

            _dots.Clear();

            foreach (Trajectory.Point point in snippet.points)
            {
                // Translate
                Vector3 dotPosition = new Vector3(
                    point.x + this._model.position.x,
                    0,
                    point.z + this._model.position.z
                    );

                // Rotate
                //dotPosition.Rotate(Quaternion.Inverse(this._model.rotation).eulerAngles.x);

                GameObject dot = Instantiate(Dot);
                _dots.Add(dot);
                dot.transform.position = dotPosition;
            }
        }
    }
}
