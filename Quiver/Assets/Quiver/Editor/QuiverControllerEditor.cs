using UnityEngine;
using UnityEditor;
using System.Collections;

namespace VTL.Quiver
{
	[System.Serializable]

	[CustomEditor(typeof(QuiverController))]
	public class QuiverControllerEditor : Editor 
	{

		void OnSceneGUI()
		{
			QuiverController vAC = target as QuiverController;
			Vector3 ori = vAC.transform.position;
			Vector2 tar = vAC.areaDimensions;
			Vector3[] lineNode = new Vector3[]{ ori, new Vector3 (ori.x - tar.x, ori.y, ori.z), new Vector3 (ori.x, ori.y, ori.z+tar.y), new Vector3 (ori.x-tar.x, ori.y, ori.z+tar.y) };

			Handles.DrawLine (lineNode [0], lineNode [1]);
			Handles.DrawLine (lineNode [1], lineNode [3]);
			Handles.DrawLine (lineNode [3], lineNode [2]);
			Handles.DrawLine (lineNode [2], lineNode [0]);

		}//-end OnSceneGUI

	}//- end Class
}