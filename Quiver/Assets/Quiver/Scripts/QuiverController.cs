/*
 * Copyright (c) 2015, Jacob Cooper (coop3683@outlook.com)
 * Date: 2/17/2015
 * License: BSD (3-clause license)
 */ 
 
using UnityEngine;
using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace VTL.Quiver
{
	[System.Serializable]

	public class QuiverController : MonoBehaviour 
	{
		//README - Place this script on an object in the scene. Set the terrainLayer to the same layer as the terrain collider object.
		//Quite a bit of front end processing could be mitigated by preprocessing the data files and setting vectors outside the filter to vector2.zero
		//The value could then just be ignored instead of comparing magnitudes

		public Vector2 areaDimensions; //the dimensions of the terrain
		public float heightOffset = 1.0f;
		public LayerMask terrainLayer;

		public float arrowSize;
		public Transform arrowPrefab;
		public RectTransform simTimeControls;
		public Transform fileIndicator;
		public Transform vectorLabels;


		public float magnitudeScaling;
		public float magnitudeBias; // positive numbers push high end into clamp, negative numbers push low end into clamp, 0 is unaltered | operating on normalized # (0 to 1)
		public Vector2 magnitudeFilterRange; //if 0,0 ignore filter, x = lower threshold, y = upper threshold

		public Color colorSimple;
		public bool colorUseImage;
		public Texture2D colorMapImage;
		public bool colorReverseMap;

		public List<TextAsset> dataFiles = new List<TextAsset> ();
		public List<List<Vector2>> dataFrames = new List<List<Vector2>> ();
		public List<Transform> arrows = new List<Transform> ();

		private UnityEngine.UI.Slider timeSlider;
		private VTL.SimTimeControls.TimeSlider timeSliderScript;
		private QuiverSTCOverlay vectorOverlay;

		private bool error = false;
		private bool dataFinished = false;
		private bool spawned = false;
		private bool spawnMultiple = false;
		private int spawnFile = 0;
		private Vector2 arrowSpacing;
		private float currentPosition = 0;

		private List<List<List<Vector2>>> rawData = new List<List<List<Vector2>>> ();
		private List<Vector2> arrowPositions = new List<Vector2> ();
		private List<Vector2> magnitudeExtents = new List<Vector2> ();

		void Start ()
		{
			errorCheck ();

			if (error)
				return;

			//retrieve the slider with the TimeSlider script on it
			timeSliderScript = simTimeControls.GetComponentInChildren<VTL.SimTimeControls.TimeSlider>();
			timeSlider = timeSliderScript.gameObject.GetComponent<UnityEngine.UI.Slider>();

			Transform t;
			RectTransform rt;
			float f;

			//create markers indicating the files on the SimTimeControl slider
			for (int i = 0; i < dataFiles.Count; i++) {

				t = Instantiate (fileIndicator);
				t.SetParent(timeSlider.transform.GetChild(1) ,false); //dump into fill area
				rt = t.gameObject.GetComponent<RectTransform> ();
		
				if (i == 0)  //first indicator has no changes
				{
					t.name = "fileInd_first";
				} else if (i == dataFiles.Count - 1) //last indicator is pushed to the far end of the slider
				{
					rt.anchorMin = new Vector2 (1, 0.5f);
					rt.anchorMax = new Vector2 (1, 0.5f);
					rt.pivot = new Vector2 (-1, 0.5f);
					t.name = "fileInd_last";
				} else if (i > 0 && i < dataFiles.Count - 1)// middle indicators are distributed according to i/#files-1
				{
					f = (float)i / ((float)dataFiles.Count - 1f);

					rt.anchorMin = new Vector2 (f, 0.5f);
					rt.anchorMax = new Vector2 (f, 0.5f);
					rt.pivot = new Vector2 (-f, 0.5f);
					t.name = "fileInd_" + i;
				}

			}

			//create additional ui texts to display the current file and timestamp
			t = Instantiate (vectorLabels);
			t.SetParent (timeSlider.transform.parent, false);
			vectorOverlay = t.gameObject.GetComponent<QuiverSTCOverlay> ();

			//load the files and then sort them
			LoadFiles ();

			//spawn the arrows after loading the files. This will wait until data sorting is completed in a separate thread
			StartCoroutine ("Spawn");

		}//- end Start

		void Update()
		{
			if (error) 
				return;

			//Find the relative position of the slider and translate that into the relative position in the files
			currentPosition = (timeSlider.value / timeSlider.maxValue) * ((dataFiles.Count == 1) ? 1 : (float)dataFiles.Count - 1.0f);

			//Send the current file name to the STC overlays
			vectorOverlay.fileNameStr = dataFiles[Mathf.FloorToInt (currentPosition)].name;

			if(spawned && timeSliderScript.IsPlaying)
				UpdateArrows (currentPosition);

		}//- end Update

		IEnumerator Spawn () 
		{
			spawned = false;

			while (!dataFinished) 
			{
				yield return 0; //wait a frame and then check again
			}

			if (spawned && !spawnMultiple) 
				ClearArrows ();

			RaycastHit hit;
			Transform newArrow;
			Vector3 origin = transform.position; //the GameObject this script is attached to acts as the world origin for arrow spawn
			Component[] renderers;
			float arrowSpacer = (arrowSpacing.x < arrowSpacing.y) ? arrowSpacing.x : arrowSpacing.y;

			for (int a = 0; a < dataFrames[spawnFile].Count; a++) 
			{
				float newMagnitude = dataFrames[spawnFile][a].sqrMagnitude;

				if (Physics.Raycast (new Vector3 ((origin.x - (arrowPositions [a].x * arrowSpacing.x)), 2000, (origin.z + (arrowPositions [a].y * arrowSpacing.y))), -Vector3.up, out hit, 4000, terrainLayer)) {

					//Spawn - instantiate
					newArrow = (Transform)Instantiate (arrowPrefab, (hit.point + Vector3.up * heightOffset), Quaternion.identity);
					newArrow.name += "_" + arrowPositions [a].x + "_" + arrowPositions [a].y + "_" + "null";
					newArrow.transform.SetParent (this.transform);
					newArrow.localScale = Vector3.one * arrowSize;
					arrows.Add (newArrow);
				

					//Modify - scale

					float magnitudeNormal = Mathf.Clamp ((newMagnitude - magnitudeExtents [spawnFile].y) / (magnitudeExtents [spawnFile].x - magnitudeExtents [spawnFile].y) + (magnitudeBias), 0, 1);
					float newZ = magnitudeNormal * ((arrowSize / 2) + (arrowSpacer * 0.9f));

					newArrow.localScale = new Vector3 (arrowSize, arrowSize, (arrowSize / 2) + newZ);

					//Modify - name
					string[] nameSub = newArrow.name.Split (new string[] { "_" }, StringSplitOptions.None);
					nameSub [nameSub.Length - 1] = (dataFrames [spawnFile] [a].magnitude * magnitudeScaling).ToString ();
					newArrow.name = String.Join ("_", nameSub);
				
					//Modify - color
					renderers = newArrow.GetComponentsInChildren<Renderer> ();
					foreach (Renderer r in renderers) {
						if (colorUseImage) {
							if (!colorReverseMap)
								r.material.color = colorMapImage.GetPixel ((int)(magnitudeNormal * (colorMapImage.width - 1)), 1);
							else
								r.material.color = colorMapImage.GetPixel ((int)((1 - magnitudeNormal) * (colorMapImage.width - 1)), 1);
						} else
							r.material.color = colorSimple;
					}
				
					//Modify - direction
			
					if (Physics.Raycast (new Vector3 (newArrow.position.x + (dataFrames [spawnFile] [a].x * 1.1f), 2000, newArrow.position.z + (dataFrames [spawnFile] [a].y * 1.1f)), -Vector3.up, out hit, 4000, terrainLayer)) {					
						newArrow.transform.LookAt ((hit.point + Vector3.up * heightOffset));
					}
				}
			}

			spawned = true;

			yield return null;

		}//- end Spawn

		public void UpdateArrows(float currentFrame)
		{
			int file = Mathf.FloorToInt (currentFrame);
			int fileNext = (currentFrame == dataFiles.Count - 1) ? file : file + 1;
			Debug.Log ("file: " + file + " fileNext: " +fileNext);
			float pos = currentFrame - file;

			RaycastHit hit;
			Component[] renderers;
			float arrowSpacer = (arrowSpacing.x < arrowSpacing.y) ? arrowSpacing.x : arrowSpacing.y; //use the smaller of x or y as the determining factor in the magnitude clamp

			for (int a = 0; a < arrows.Count; a++) 
			{
				Vector3 startScale = Vector3.zero;
				Color startColor = Color.black;
				Quaternion startRotation = Quaternion.identity;

				Vector3 endScale = Vector3.zero;
				Color endColor = Color.black;
				Quaternion endRotation = Quaternion.identity;

				//Setup Initials ------------------------------------------------------------------------------------------------------------------------------------------------
				//Start - scale
				float newMagnitude = dataFrames [file] [a].sqrMagnitude;
				float magnitudeNormal = Mathf.Clamp ((newMagnitude - magnitudeExtents [file].y) / (magnitudeExtents [file].x - magnitudeExtents [file].y) + (magnitudeBias), 0, 1);
				float newZ = magnitudeNormal * ((arrowSize / 2) + (arrowSpacer * 0.9f));

				startScale = new Vector3 (arrowSize, arrowSize, (arrowSize / 2) + newZ);

				//Start - color
				if (colorUseImage) 
				{
					if (!colorReverseMap)
						startColor = colorMapImage.GetPixel((int)(magnitudeNormal*(colorMapImage.width-1)),1);
					else
						startColor = colorMapImage.GetPixel((int)((1-magnitudeNormal)*(colorMapImage.width-1)),1);
				} else
					startColor = colorSimple;

				//Start - direction

				if (Physics.Raycast (new Vector3 (arrows [a].position.x + (dataFrames [file] [a].x * 1.1f), 2000, arrows [a].position.z + (dataFrames [file] [a].y * 1.1f)), -Vector3.up, out hit, 4000, terrainLayer)) 
				{
						startRotation = Quaternion.LookRotation (arrows [a].position - (hit.point + Vector3.up * heightOffset));
				}


				//Setup Finals ------------------------------------------------------------------------------------------------------------------------------------------------
				//End - scale
				newMagnitude = dataFrames [fileNext] [a].sqrMagnitude;
				magnitudeNormal = Mathf.Clamp ((newMagnitude - magnitudeExtents [fileNext].y) / (magnitudeExtents [fileNext].x - magnitudeExtents [fileNext].y) + (magnitudeBias), 0, 1);
				newZ = magnitudeNormal * ((arrowSize / 2) + (arrowSpacer * 0.9f));

				endScale = new Vector3 (arrowSize, arrowSize, (arrowSize / 2) + newZ);

				//End - color
				if (colorUseImage) 
				{
					if (!colorReverseMap)
						endColor = colorMapImage.GetPixel((int)(magnitudeNormal*(colorMapImage.width-1)),1);
					else
						endColor = colorMapImage.GetPixel((int)((1-magnitudeNormal)*(colorMapImage.width-1)),1);
				} else
					endColor = colorSimple;

				//End - direction

				if (Physics.Raycast (new Vector3 (arrows [a].position.x + (dataFrames [fileNext] [a].x * 1.1f), 2000, arrows [a].position.z + (dataFrames [fileNext] [a].y * 1.1f)), -Vector3.up, out hit, 4000, terrainLayer)) 
				{
					endRotation = Quaternion.LookRotation (arrows [a].position - (hit.point + Vector3.up * heightOffset));
				}

				//Apply Lerps
				arrows [a].localScale = Vector3.Lerp (startScale, endScale, pos);
				renderers = arrows[a].GetComponentsInChildren<Renderer> ();
				foreach (Renderer r in renderers) 
				{
					r.material.color = Color.Lerp (startColor, endColor, pos);
				}
				arrows [a].rotation = Quaternion.Lerp (startRotation, endRotation, pos);

				//Modify - name
				string[] nameSub = arrows [a].name.Split (new string[] { "_" }, StringSplitOptions.None);
				nameSub [nameSub.Length - 1] = (Mathf.Lerp(dataFrames [file] [a].magnitude ,dataFrames [fileNext] [a].magnitude, pos)*magnitudeScaling).ToString ();
				arrows [a].name = String.Join ("_", nameSub);
			}

		}//- end UpdateArrows

		public void ClearArrows()
		{
			foreach (Transform child in this.transform) 
			{
				Destroy (child.gameObject);
			}

		}//- end ClearArrows

		public void LoadFiles ()
		{
			// this method takes a great deal of time frontloading and can only occur in the main thread so expect a delay before the simulation begins
			for (int f = 0; f < dataFiles.Count; f++) 
			{
				rawData.Add (VectorFieldResourceLoader.Load (dataFiles[f].name));
			}
			//Thread to async filter data sets and unify them in one list broken into files
			//Actually two lists: one for vector data and one for position synced by index
			System.Threading.ThreadPool.QueueUserWorkItem (new System.Threading.WaitCallback (ProcessData), new object{});

		}//- end LoadFiles

		public void ProcessData (object state)
		{
			// this threaded method sorts through the raw data and filters out points by relative magnitude. Note that it is sqrMagnitude (x^2+y^2) to save unecessary steps
			// the magnitudeFilterRange uses X as the minium cutoff and Y as the maximum cutoff

			float magX = 0;
			float magY = 0;

			float newMagnitude = 0;

			arrowSpacing = new Vector2 ((1f / ((float)rawData [0].Count-1f)) * areaDimensions.x, (1f / ((float)rawData [0] [0].Count-1f)) * areaDimensions.y);
			Debug.Log ("Arrow Spacing: " + arrowSpacing);

			for (int f = 0; f < rawData.Count; f++) 
			{	//files
				dataFrames.Add(new List<Vector2>());
				magnitudeExtents.Add (Vector2.zero);

				for (int x = 0; x < rawData[f].Count; x++) 
				{
					for (int y = 0; y < rawData[f] [x].Count; y++) 
					{
						newMagnitude = rawData [f] [x] [y].sqrMagnitude;

						if (f == 0) //only run on first file
						{
							//This loop ensures arrow consistancy across all files; if an arrow does not match filters across all files, it is not spawned
							int test = 0;

							for (int n = 0; n < rawData.Count; n++) 
							{ 
								float testmag = rawData [n] [x] [y].sqrMagnitude;
								if (magnitudeFilterRange.x < testmag * magnitudeScaling && testmag * magnitudeScaling < magnitudeFilterRange.y)  //Magnitude filter
								test++;
							}

							if (test == rawData.Count)
								arrowPositions.Add(new Vector2(x,y));
						}

						if (arrowPositions.Exists(e => e == new Vector2(x,y)))
						{
							dataFrames[f].Add (rawData [f][x][y]);


							if (newMagnitude	> magnitudeExtents [f].x)
								magX = newMagnitude;
							else
								magX = magnitudeExtents [f].x;
							
							if (newMagnitude < magnitudeExtents [f].y)
								magY = newMagnitude;
							else
								magY = magnitudeExtents [f].y;

							magnitudeExtents [f] = new Vector2 (magX, magY);	// x == highest,  == lowest	
						}
					}
				}
			}
			dataFinished = true;

		}//- end ProcessData

		void errorCheck()
		{
			//Errors
			if (terrainLayer == LayerMask.GetMask ("Nothing")) 
			{
				Debug.LogError ("No terrainLayer selected!");
				error = true;
			}
			if (!arrowPrefab) 
			{
				Debug.LogError ("No arrowPrefab selected!");
				error = true;
			}
			if (!simTimeControls) 
			{
				Debug.LogError ("No simTimeController selected!");
				error = true;
			}
			if (!fileIndicator)
			{
				Debug.LogError ("No File Indicator selected!");
				error = true;
			}
			if (!vectorLabels)
			{
				Debug.LogError ("No Vector Labels selected!");
				error = true;
			}
			if (dataFiles.Count == 0) 
			{
				Debug.LogError ("No files found in Data Files!");
				error = true; return;
			}
			if (!colorMapImage) 
			{
				Debug.LogError ("No Color Map selected!");
				error = true;
			}
			if (areaDimensions.x == 0 || areaDimensions.y == 0) 
			{
				Debug.LogError ("No Area Dimensions set, cannot be 0s!");
				error = true;
			}
			Debug.Log (UnityEditor.AssetDatabase.GetAssetPath (dataFiles [0]));
		}//- end ErrorCheck

	}//- end class
}