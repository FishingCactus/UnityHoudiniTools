using UnityEditor;
using UnityEngine;
using FishingCactus;

public class HoudiniHdaFunctionality 
{
    [MenuItem( "FishingCactus/HoudiniHdaDuplicate" )]
    public static void onHoudiniHDADuplicate()
    {
        GameObject[] root_gameobject = Selection.gameObjects;

        if( root_gameobject == null )
        {
            Debug.LogWarning("Nothing selected. You need to select an asset!");
            return;
        }

        GameObject[] new_game_object = new GameObject[ root_gameobject.Length ];        
        for( int i = 0; i < root_gameobject.Length; i += 1 )
        {
            var go = root_gameobject[i];
    
            // The selected gameobject is the asset's root gameobject.
            // Get the root component (HEU_HoudiniAssetRoot) which will allow us to get the asset component (HEU_HoudiniAsset) itself.
            HoudiniEngineUnity.HEU_HoudiniAssetRoot root = go.GetComponent<HoudiniEngineUnity.HEU_HoudiniAssetRoot>();
            
            if (root != null )
            {
                HoudiniEngineUnity.HEU_HoudiniAsset asset = root._houdiniAsset;
                // This will return a duplicated asset as a gameobject
                new_game_object[i] = asset.DuplicateAsset();
            }
        }

        Selection.objects = new_game_object;
    }
}