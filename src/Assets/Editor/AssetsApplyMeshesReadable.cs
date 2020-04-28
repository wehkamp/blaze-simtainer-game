using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
	/// <summary>
	/// This class is used to make every mesh readable.
	/// When I was importing the SimpleApocalypse asset pack, I had the problem that every mesh was unreadable.
	/// Which caused Unity to crash when building the navmesh. Because the navmesh needs to be generated on run-time level due to the game being dynamic.
	/// </summary>
	internal class AssetsApplyMeshesReadable
	{
		[MenuItem("Assets/ Make all meshes readable")]
		internal static void MakeMeshesReadable()
		{
			string[] files =
				Directory.GetFiles("Assets/", "*.fbx.meta", SearchOption.AllDirectories); //"*" denotes all file format
			foreach (string filePath in files)
			{
				string fileText = File.ReadAllText(filePath);
				bool changes = false;
				string[] possibleStrings = {"isReadable: ", "IsReadable: "};

				foreach (string possibleString in possibleStrings)
				{
					if (!fileText.Contains(possibleString + "0")) continue;
					fileText = fileText.Replace(possibleString + "0", possibleString + "1");
					changes = true;
				}

				if (!changes) continue;
				Debug.Log($"Changing mesh for {filePath.Split('\\').Last()} Location: {filePath}");
				File.WriteAllText(filePath, fileText);
			}
			Debug.Log("Made all meshes readable");
		}
	}
}