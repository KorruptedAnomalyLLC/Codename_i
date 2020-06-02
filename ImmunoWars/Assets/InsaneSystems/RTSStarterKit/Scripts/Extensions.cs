﻿using System.Collections.Generic;
using UnityEngine;

namespace InsaneSystems.RTSStarterKit
{
	public static class Extensions
	{
		public static List<Transform> GetAllChilds(this Transform parent, bool isRecursive = false)
		{
			var childs = new List<Transform>();

			for (int i = 0; i < parent.childCount; i++)
			{
				var child = parent.GetChild(i);
				childs.Add(child);

				if (isRecursive)
					childs.AddRange(child.GetAllChilds(isRecursive));
			}

			return childs;
		}
	}
}