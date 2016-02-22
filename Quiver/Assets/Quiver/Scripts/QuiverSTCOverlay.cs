/*
 * Copyright (c) 2015, Jacob Cooper (coop3683@outlook.com)
 * Date: 2/17/2015
 * License: BSD (3-clause license)
 */ 
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace VTL.Quiver
{
	[System.Serializable]

	public class QuiverSTCOverlay : MonoBehaviour 
	{

		public Text fileName;
		public Text fileDate;

		public string fileNameStr;

		void Start()
		{
			fileName.text = "";
			fileDate.text = "";

		}//- end Start

		void OnGUI()
		{
			fileName.text = "Current File: " + fileNameStr;
			fileDate.text = "File Time: No Date/Time Found";//overwrite if found

			string[] nameSub = fileNameStr.Split (new string[] { "_" }, System.StringSplitOptions.None);

			if (nameSub [nameSub.Length - 1].Length == 12) {
				object[] dateBits = new object[] { "File Time", nameSub [nameSub.Length - 1].Substring (4, 2), nameSub [nameSub.Length - 1].Substring (6, 2), 
					nameSub [nameSub.Length - 1].Substring (0, 4), nameSub [nameSub.Length - 1].Substring (8, 2), nameSub [nameSub.Length - 1].Substring (10, 2)
				};
				fileDate.text = string.Format ("{0}: {4}:{5} GMT, {1}/{2}/{3}", dateBits);
			}

		}//- end OnGUI

	}//- end Class

}
