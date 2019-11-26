using UnityEditor;
using UnityEngine;
using FishingCactus;

public class HoudiniHdaFunctionality {

	[MenuItem( "FishingCactus/HoudiniHdaDuplicate" )]

	public static void onHoudiniHDADuplicate()
	{
		GameObject[] rootGO = Selection.gameObjects;
		GameObject[] NewAsset = new GameObject[rootGO.Length];

		if(rootGO == null)
		{
			Debug.LogWarning("Nothing selected. You need to select an asset!");
			return;
		}
		
		if (rootGO != null )
		{
			for( int i = 0; i < rootGO.Length; i += 1 )
            {
				var go = rootGO[i];
     
				// The selected gameobject is the asset's root gameobject.
				// Get the root component (HEU_HoudiniAssetRoot) which will allow us to get the asset component (HEU_HoudiniAsset) itself.
				HoudiniEngineUnity.HEU_HoudiniAssetRoot root = go.GetComponent<HoudiniEngineUnity.HEU_HoudiniAssetRoot>();
				if (root != null )
				{
					HoudiniEngineUnity.HEU_HoudiniAsset asset = root._houdiniAsset;
					// This will return a duplicated asset as a gameobject
					NewAsset[i] = asset.DuplicateAsset();
				}
			}
		}
		Selection.objects = NewAsset;
	}
}