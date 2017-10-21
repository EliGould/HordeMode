using UnityEngine;
using UnityEditor;

public static class ForceRecompile
{
	static string[] disallowedPathComps = new string[]
	{
		"Editor",
		"Standard Assets",
		"Pro Standard Assets",
		"Plugins",
		"WebPlayerTemplates"
	};

	[MenuItem("SA/Force Recompile")]
	static void MenuRun()
	{
		string[] guids = AssetDatabase.FindAssets("t:monoscript");

		if(guids == null || guids.Length == 0) { return; }

		foreach(string guid in guids)
		{
			string path = AssetDatabase.GUIDToAssetPath(guid);

			bool validPath = true;
			// Want to target the main assembly
			foreach(string disallowed in disallowedPathComps)
			{
				if(path.Contains(disallowed))
				{
					validPath = false;
					break;
				}
			}

			if(validPath)
			{
				Dbg.LogRelease("Force Recompiling by poking: {0}", path);
				AssetDatabase.ImportAsset(path);
				break;
			}
		}
	}
}
