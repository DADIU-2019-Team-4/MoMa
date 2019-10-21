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
            Debug.Log(snippet.points.Length);

            foreach (GameObject dot in _dots)
            {
                Destroy(dot);
            }

            _dots.Clear();

            foreach (Trajectory.Point point in snippet.points)
            {
                GameObject dot = Instantiate(Dot);
                dot.transform.position = new Vector3(
                    point.x + this._model.position.x,
                    0.1f,
                    point.z + this._model.position.z
                    );
                _dots.Add(dot);
            }
        }
    }
}
