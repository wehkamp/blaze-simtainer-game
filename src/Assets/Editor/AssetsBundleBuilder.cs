using UnityEditor;

namespace Assets.Editor
{
	/// <summary>
	/// This class is used to build the asset bundles
	/// </summary>
	internal class AssetsBundleBuilder
	{
		[MenuItem("Assets/ Build AssetsBundles")]
		internal static void BuildAllAssetsBundles()
		{
			BuildPipeline.BuildAssetBundles("Assets/AssetBundles", BuildAssetBundleOptions.None,
				EditorUserBuildSettings.activeBuildTarget);
		}
	}
}