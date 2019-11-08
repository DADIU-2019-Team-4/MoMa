using UnityEngine;
using UnityEditor;

namespace MoMa
{
    [CustomEditor(typeof(CharacterController))]
    [CanEditMultipleObjects]
    public class MoMaInspector : Editor
    {
        private CharacterController _cc;
        private bool _infoIsFolded;

        private void OnEnable()
        {
            //Debug.Log("MoMa Inspector enabled");
        }

        public override void OnInspectorGUI()
        {
            CharacterController cc = (CharacterController)target;

            _infoIsFolded = EditorGUILayout.BeginFoldoutHeaderGroup(_infoIsFolded, "Info");

            if (_infoIsFolded)
            {
                CharacterController.MaxTrajectoryDiff = EditorGUILayout.FloatField(CharacterController.MaxTrajectoryDiff);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            //EditorGUILayout.Space ();

            //playerStatsIsOpen = EditorGUILayout.BeginFoldoutHeaderGroup ( playerStatsIsOpen, "Player Stats" );

            //if ( playerStatsIsOpen ) {
            //	playerCharacter.playerStats.strength = EditorGUILayout.IntSlider ( playerCharacter.playerStats.strength, 1, 50 );
            //	playerCharacter.playerStats.magic = EditorGUILayout.IntSlider ( playerCharacter.playerStats.magic, 1, 50 );
            //	playerCharacter.playerStats.defence = EditorGUILayout.IntSlider ( playerCharacter.playerStats.defence, 0, 20 );
            //	playerCharacter.playerStats.magicDefence = EditorGUILayout.IntSlider ( playerCharacter.playerStats.magicDefence, 0, 20 );
            //}

            //EditorGUILayout.EndFoldoutHeaderGroup ();


            //PlayerCharacter playerCharacter = ( PlayerCharacter ) target;

            //playerInfoIsOpen = EditorGUILayout.BeginFoldoutHeaderGroup ( playerInfoIsOpen, "Player Info" );

            //if ( playerInfoIsOpen ) {
            //	playerCharacter.playerName = EditorGUILayout.TextField ( playerCharacter.playerName );
            //	playerCharacter.playerDescription = EditorGUILayout.TextArea ( playerCharacter.playerDescription );
            //}

            //EditorGUILayout.EndFoldoutHeaderGroup ();
        }
    }
}