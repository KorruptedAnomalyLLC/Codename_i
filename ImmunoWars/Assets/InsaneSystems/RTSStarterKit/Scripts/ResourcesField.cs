using System;
using System.Collections.Generic;
using InsaneSystems.RTSStarterKit.Controls;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public class ResourcesField : MonoBehaviour
	{
		public static List<ResourcesField> sceneResourceFields { get; private set; }

		void Awake()
		{
			if (sceneResourceFields == null)
				sceneResourceFields = new List<ResourcesField>();
			
			sceneResourceFields.Add(this);
		}

		public void OnMouseEnter()
		{
			if (Selection.selectedUnits.Count == 0 || !Selection.selectedUnits[0].data.isHarvester)
				return;
			
			var selectedHarvester = Selection.selectedUnits[0].GetModule<Harvester>();
			var needResourcesCursour = selectedHarvester.harvestedResources < selectedHarvester.MaxResources;
			
			if (needResourcesCursour)
				Cursors.SetResourcesCursor();
			else
				Cursors.SetRestrictCursor();
		}
		
		public void OnMouseExit()
		{
			Cursors.SetDefaultCursor();
		}

		void OnDestroy()
		{
			sceneResourceFields.Remove(this);
		}
	}
}