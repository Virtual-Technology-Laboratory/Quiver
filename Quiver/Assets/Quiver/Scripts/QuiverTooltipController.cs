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

	public class QuiverTooltipController : MonoBehaviour 
	{
		public LayerMask mask;
		public Transform tooltip;

		private QuiverTooltip vAT;

		// Use this for initialization
		void Start () 
		{
			if (mask == LayerMask.NameToLayer("Nothing")) 
			{
				Debug.Log ("No LayerMask for vector objects assigned to Tooltip Controller!");
				return;
			}		
			if (!tooltip) 
			{
				Debug.Log ("No Tooltip prefab assigned to Tooltip Controller!");
				return;
			}

			Transform t = Instantiate (tooltip);
			t.SetParent (GameObject.Find ("Canvas").GetComponent<RectTransform>(), false);

			vAT = t.GetComponent<QuiverTooltip> ();
			vAT.newString = "";

		}//- end Start
		
		// Update is called once per frame
		void Update () 
		{
			RaycastHit hit;

			if (Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out hit, 1000, mask)) 
			{
				vAT.newString = hit.transform.name;
			} 
			else 
			{
				vAT.newString = "";
			}

		}//- end Update

	}//- end class

}
