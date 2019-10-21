using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoMa
{
    [RequireComponent(typeof(MovementController))]
    public class Follower : MonoBehaviour
    {
        public GameObject Dot;

        private List<GameObject> _dots = new List<GameObject>();

        public void Draw(Trajectory.Snippet snippet)
        {
            foreach (GameObject dot in _dots)
            {
                Destroy(dot);
            }

            _dots.Clear();

            foreach (Trajectory.Point point in snippet.points)
            {
                // Translate
                Vector2 dotPosition = new Vector2(
                    point.x + this.transform.position.x,
                    point.z + this.transform.position.z
                    );

                // Rotate
                dotPosition.Rotate(Quaternion.Inverse(this.transform.rotation).eulerAngles.z);

                GameObject dot = Instantiate(Dot);
                _dots.Add(dot);
                dot.transform.position = dotPosition;
            }
        }
    }
}
