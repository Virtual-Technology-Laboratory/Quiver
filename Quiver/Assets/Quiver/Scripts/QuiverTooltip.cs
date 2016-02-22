/*
 * Copyright (c) 2015, Jacob Cooper (coop3683@outlook.com)
 * Date: 2/17/2015
 * License: BSD (3-clause license)
 */ 
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

namespace VTL.Quiver
{
	[System.Serializable]

	public class QuiverTooltip : MonoBehaviour 
	{
		public Text positionText;
		public Text magnitudeText;
		public GameObject positionObject;

		public string newString;

		void OnGUI () 
		{
			if (newString != "") 
			{
				positionObject.SetActive (true);
				string[] nameSub = newString.Split (new string[] { "_" }, StringSplitOptions.None);
				positionText.text = "Position: " + string.Format ("x={0}, y={1}", nameSub [nameSub.Length - 3], nameSub [nameSub.Length - 2]);
				magnitudeText.text = "Magnitude: " + nameSub [nameSub.Length - 1];
			} 
			else 
			{
				positionObject.SetActive (false);
			}

		}//- end OnGUI

	}//- end class

}