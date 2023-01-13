using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ExperimentalPolygonCollider
{

    [CustomEditor(typeof(ExperimentalPolygonCollider))]
    public class ExperimentalPolygonColliderEditor : Editor {


        public ExperimentalPolygonCollider experimentalPolygonCollider;

        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();
            experimentalPolygonCollider = target as ExperimentalPolygonCollider;


            if (GUILayout.Button("Update collider"))

                experimentalPolygonCollider.RecomputePolygon();
        }
    }
}
