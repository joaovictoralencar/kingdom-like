#if UNITY_EDITOR
//  Editor only. Guarded so the file also stays out of player builds if this
//  package ever ends up inside a user-provided .asmdef, where Unity's
//  "Editor folder" rule does not apply.
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;


namespace LuxURPEssentials
{
	[InitializeOnLoad]
	public class LuxURPEssentialsShowWelcome : MonoBehaviour
	{
	    
		static LuxURPEssentialsShowWelcome()
		{
		//	To show it at start up
			EditorApplication.update += Update;
		}


	    static void Update()
		{
			EditorApplication.update -= Update;

			if( !EditorApplication.isPlayingOrWillChangePlaymode )
			{
				var hide = EditorPrefs.GetBool("LuxURPEssentialsDoNotShowWelcome");
				if(!hide)
				{
					LuxURPEssentialsWelcome.Init();
				}
			}
		}
	}
}
#endif // UNITY_EDITOR
