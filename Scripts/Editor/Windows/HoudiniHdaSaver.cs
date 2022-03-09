using HoudiniEngineUnity;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FishingCactus
{
    public class HoudiniHdaSaver : EditorWindow
    {
        // -- UNITY

        [MenuItem( "FishingCactus/Houdini/HdaSaver" )]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow( typeof( HoudiniHdaSaver ) );
        }

        private readonly string HoudiniHdaSaverHelperVersion = "06/06/2019";
        private const string HoudiniDefineTag = "FC_USING_HOUDINI";

        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            {
                DrawToolActions();
                DrawDataListContent();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawToolActions()
        {
            EditorGUILayout.HelpBox(
                $"Buttons will act only for thicked assets{Environment.NewLine}" +
                $"The save functionnality won't do anything if the save folder path is empty !{Environment.NewLine}" +
                $"The \"Refresh List\" button will tick automatically selected Houdini Root Object in the project hierarchy.{Environment.NewLine}" +
                $"The \"Bake\" buttons will save, bake and delete the asset root in scene.", MessageType.Info );

            EditorGUILayout.BeginHorizontal();
            {
                if( !IsWindowInit )
                {
                    ButtonRefreshListsClicked();
                    IsWindowInit = true;
                    StaticFolderPath = PlayerPrefs.GetString( FolderPathPlayerPrefsKey, string.Empty );
                }
                if( GUILayout.Button( "Refresh from opened scenes" ) )
                {
                    ButtonRefreshListsClicked();
                }
                if( GUILayout.Button( "Sort All" ) )
                {
                    ButtonSortAllClicked();
                }
                if( GUILayout.Button( "Rename All" ) )
                {
                    ButtonRenameAllClicked();
                }
                if( GUILayout.Button( "Save All" ) )
                {
                    ButtonSaveAllClicked();
                }
                if( GUILayout.Button( "Bake All" ) )
                {
                    ButtonBakeAllClicked();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = !IsFolderPathValidFlag ? Color.red : Color.black;

                GUILayout.Label( "Save Folder Path :", style, new GUILayoutOption[] { GUILayout.ExpandWidth( false ), GUILayout.Width( 120 ) } );
                GUI.contentColor = Color.black;
                StaticFolderPath = GUILayout.TextField( StaticFolderPath );

                if( GUILayout.Button( "...", new GUILayoutOption[] { GUILayout.ExpandWidth( false ), GUILayout.Width( 30 ) } ) )
                {
                    StaticFolderPath = EditorUtility.OpenFolderPanel( "Select Houdini HDA save folder...", StaticFolderPath, "" );
                }

                GUI.enabled = IsFolderPathValidFlag;
                if( GUILayout.Button( "Open", new GUILayoutOption[] { GUILayout.ExpandWidth( false ), GUILayout.Width( 50 ) } ) )
                {
                    EditorUtility.RevealInFinder( StaticFolderPath );
                }
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();

            if( GUI.changed )
            {
                IsFolderPathValidFlag = Directory.Exists( StaticFolderPath );
                if( IsFolderPathValidFlag )
                {
                    PlayerPrefs.SetString( FolderPathPlayerPrefsKey, StaticFolderPath );
                    PlayerPrefs.Save();
                }
            }
        }

        private void DrawDataListContent()
        {
            for( int index = HoudiniHDASaver_Data.Count - 1; index >= 0; index-- )
            {
                HoudiniHDASaver_Data[index].DrawGui();
            }
        }

        private void ButtonRefreshListsClicked()
        {
            HoudiniHDASaver_Data.Clear();
            HEU_HoudiniAssetRoot[] roots = FindObjectsOfType<HEU_HoudiniAssetRoot>();
            foreach( HEU_HoudiniAssetRoot root in roots )
            {
                GameObject rootGameObject = root.gameObject;
                Scene currentAssetScene = rootGameObject.scene;
                bool assetIsInSelection = Selection.Contains( rootGameObject );

                int index = 0;
                while( index < HoudiniHDASaver_Data.Count && HoudiniHDASaver_Data[index].Scene != currentAssetScene ) index++;
                if( index == HoudiniHDASaver_Data.Count )
                {
                    HoudiniHDASaver_Data.Add( new SceneHoudiniRoot( currentAssetScene, root, assetIsInSelection ) );
                }
                else
                {
                    HoudiniHDASaver_Data[index].AddToSceneRootAssetCollection( root, assetIsInSelection );
                }
            }
        }

        private void ButtonSortAllClicked()
        {
            foreach( SceneHoudiniRoot data in HoudiniHDASaver_Data )
            {
                if( data.ToogleScene )
                {
                    data.SortAll();
                }
            }
            Debug.Log( $"Houdini HDA Save Helper : Sort All assets sorted)" );
        }

        private void ButtonRenameAllClicked()
        {
            string consoleLog = string.Empty;
            int affectedAssetsCount = 0;
            foreach( SceneHoudiniRoot data in HoudiniHDASaver_Data )
            {
                if( data.ToogleScene )
                {
                    string log = string.Empty;
                    affectedAssetsCount += data.RenameAll( out log );
                    consoleLog += log;
                }
            }
            Debug.Log( $"Houdini HDA Save Helper : Rename All ({affectedAssetsCount} assets renamed) {Environment.NewLine}{consoleLog}" );
        }

        private void ButtonSaveAllClicked()
        {
            string consoleLog = string.Empty;
            int savedAssetsCount = 0;
            foreach( SceneHoudiniRoot data in HoudiniHDASaver_Data )
            {
                if( data.ToogleScene )
                {
                    string log = string.Empty;
                    int result = data.SaveAll( out log );
                    if( result == -1 )
                    {
                        Debug.Log( $"Houdini HDA Save Helper : Save Failed !" );
                    }
                    else
                    {
                        savedAssetsCount += result;
                        consoleLog += log;
                    }
                }
            }
            Debug.Log( $"Houdini HDA Save Helper : Save All ({savedAssetsCount} assets saved) {Environment.NewLine}{consoleLog}" );
        }

        private void ButtonBakeAllClicked()
        {
            string consoleLog = string.Empty;
            int bakedAssetsCount = 0;
            foreach( SceneHoudiniRoot data in HoudiniHDASaver_Data )
            {
                if( data.ToogleScene )
                {
                    string log = string.Empty;
                    int result = data.BakeAll( out log );
                    if( result == -1 )
                    {
                        Debug.Log( $"Houdini HDA Save Helper : Bake Failed !" );
                    }
                    else
                    {
                        bakedAssetsCount += result;
                        consoleLog += log;
                    }
                    if( data.IsSceneEmpty() )
                    {
                        HoudiniHDASaver_Data.Remove( data );
                    }
                }
            }
            Debug.Log( $"Houdini HDA Save Helper : Ball All ({bakedAssetsCount} assets baked) {Environment.NewLine}{consoleLog}" );
        }

        private const string FolderPathPlayerPrefsKey = "ArtistTools_HoudiniHdaSaverHelper_SaveFolder";
        private static bool IsFolderPathValidFlag = true;
        private bool IsWindowInit = false;
        private static string StaticFolderPath = string.Empty;
        private static List<SceneHoudiniRoot> HoudiniHDASaver_Data = new List<SceneHoudiniRoot>();

        // -- INNER CLASSES

        private class SceneHoudiniRoot
        {
            // -- PUBLIC

            public bool ToogleScene = false;
            public bool ToogleAllAsset = false;
            public Scene Scene;
            public string FileSaveNameBase = string.Empty;

            public SceneHoudiniRoot( Scene asset_scene, HEU_HoudiniAssetRoot asset, bool asset_toggle )
            {
                Scene = asset_scene;
                AddToSceneRootAssetCollection( asset, asset_toggle );
                FileSaveNameBase = asset.name;
            }

            public bool IsSceneEmpty()
            {
                return AssetToggleCollection.Count == 0;
            }

            public void AddToSceneRootAssetCollection( HEU_HoudiniAssetRoot asset, bool is_toggle )
            {
                AssetToggleCollection.Add( new RootAssetToggle( asset, is_toggle ) );
                ToogleScene = ToogleScene || is_toggle;
            }

            /// <summary>
            /// Rename all toggled Asset Root for the current scene.
            /// </summary>
            /// <param name="log">List of renamed assets name (log purpose).</param>
            /// <returns>-1 if it fail, the number of assets saved otherwise</returns>
            public int RenameAll( out string console_log )
            {
                string log = string.Empty;
                int affectedRow = 0;
                for( int level0Index = 0; level0Index < AssetToggleCollection.Count; level0Index++ )
                {
                    if( AssetToggleCollection[level0Index].IsToggle )
                    {
                        //rename root (level 0)
                        HEU_HoudiniAssetRoot asset = AssetToggleCollection[level0Index].RootAsset;
                        affectedRow++;
                        GameObject level0GameObject = asset.gameObject;
                        log += level0GameObject.name + " => ";
                        level0GameObject.name = FileSaveNameBase + (affectedRow.ToString( "D2" ));
                        log += level0GameObject.name + Environment.NewLine;

                        //rename level 1
                        Transform level0Transform = level0GameObject.transform;
                        for( int level1Index = 0; level1Index < level0Transform.childCount; level1Index++ )
                        {
                            GameObject level1GameObject = level0Transform.GetChild( level1Index ).gameObject;
                            if( level1GameObject.GetComponent<HEU_HoudiniAsset>() == null ) // ignore HDA_Data
                            {
                                log += level1GameObject.name + " => ";
                                level1GameObject.name = $"{level0GameObject.name}_{(level1Index + 1).ToString( "D2" )}";
                                log += level1GameObject.name + Environment.NewLine;

                                //rename level 2
                                Transform level1Transform = level1GameObject.transform;
                                for( int level2Index = 0; level2Index < level1Transform.childCount; level2Index++ )
                                {
                                    GameObject level2GameObject = level1Transform.GetChild( level2Index ).gameObject;
                                    log += level2GameObject.name + " => ";
                                    level2GameObject.name = $"{level1GameObject.name}_instance{(level2Index + 1).ToString( "D2" )}";
                                    log += level2GameObject.name + Environment.NewLine;
                                }
                            }
                        }
                        log += Environment.NewLine;
                    }
                }
                console_log = log;
                return affectedRow;
            }

            /// <summary>
            /// Save all toggled Asset Root for the current scene.
            /// </summary>
            /// <param name="console_log">List of saved assets path (log purpose).</param>
            /// <returns>-1 if it fail, the number of assets saved otherwise</returns>
            public int SaveAll( out string console_log )
            {
                string log = string.Empty;
                int affectedRow = -1;
                if( ToogleScene )
                {
                    string folderPath = CheckFolderPathValidity( StaticFolderPath, Scene.name );
                    if( !string.IsNullOrEmpty( folderPath ) )
                    {
                        affectedRow = 0;
                        for( int index = 0; index < AssetToggleCollection.Count; index++ )
                        {
                            if( AssetToggleCollection[index].IsToggle )
                            {
                                HEU_HoudiniAssetRoot assetRoot = AssetToggleCollection[index].RootAsset;
                                string fullpath = CreateFullPath( folderPath, assetRoot.gameObject.name );
                                HEU_AssetPresetUtility.SaveAssetPresetToFile( assetRoot.HoudiniAsset, fullpath );
                                log += fullpath + Environment.NewLine;
                                affectedRow++;
                            }
                        }
                    }
                    else
                    {
                        log += "Folder path not valid.";
                    }
                }
                console_log = log;
                return affectedRow;
            }

            /// <summary>
            /// Save, bake and delete all toggled Asset Root for the current scene.
            /// The Houdini Asset Root objects will be replaced by their baked version.
            /// </summary>
            /// <param name="console_log">List of baked assets path (log purpose).</param>
            /// <returns>-1 if it fail, the number of assets baked otherwise</returns>
            public int BakeAll( out string console_log )
            {
                string log = string.Empty;
                int bakeCount = -1;
                string folderPath = CheckFolderPathValidity( StaticFolderPath, Scene.name );
                if( !string.IsNullOrEmpty( folderPath ) )
                {
                    bakeCount = 0;
                    for( int index = 0; index < AssetToggleCollection.Count; index++ )
                    {
                        string bakeLog;
                        if( BakeAsset( index, folderPath, out bakeLog ) )
                        {
                            log += bakeLog + Environment.NewLine;
                            bakeCount++;
                        }
                    }
                    DeleteTaggedAssetsGamesObject();
                    if( IsSceneEmpty() )
                    {
                        HoudiniHDASaver_Data.Remove( this );
                    }
                }
                else
                {
                    log += "Folder path not valid.";
                }
                console_log = log;
                return bakeCount;
            }

            public void SortAll( )
            {
                AssetToggleCollection.Sort();
                for( int i = 0; i < AssetToggleCollection.Count; i++ )
                {
                    AssetToggleCollection[i].RootAsset.transform.SetSiblingIndex( i );
                }
            }

            public void DrawGui()
            {
                EditorGUILayout.BeginVertical();
                {
                    ToogleScene = EditorGUILayout.BeginToggleGroup( Scene.name, ToogleScene );
                    if( ToogleScene )
                    {
                        using( new GUILayout.HorizontalScope() )
                        {
                            GUILayout.Space( 30f );
                            bool toggleJustChange = false;
                            using( new GUILayout.VerticalScope() )
                            {
                                EditorGUILayout.BeginHorizontal( GUILayout.Height( 20f ) );
                                {
                                    EditorGUI.BeginChangeCheck();
                                    ToogleAllAsset = EditorGUILayout.ToggleLeft( "All", ToogleAllAsset );
                                    toggleJustChange = EditorGUI.EndChangeCheck();

                                    GUILayout.Space( 50f );
                                    if( GUILayout.Button( "Save", new GUILayoutOption[] { GUILayout.ExpandWidth( false ), GUILayout.Width( 50 ) } ) )
                                    {
                                        ButtonSaveClicked();
                                    }
                                    if( GUILayout.Button( "Bake", new GUILayoutOption[] { GUILayout.ExpandWidth( false ), GUILayout.Width( 50 ) } ) )
                                    {
                                        ButtonBakeClicked();
                                    }
                                    if( GUILayout.Button( "Sort", new GUILayoutOption[] { GUILayout.ExpandWidth( false ), GUILayout.Width( 50 ) } ) )
                                    {
                                        ButtonSortClicked();
                                    }
                                    if( GUILayout.Button( "Rename", new GUILayoutOption[] { GUILayout.ExpandWidth( false ), GUILayout.Width( 70 ) } ) )
                                    {
                                        ButtonRenameClicked();
                                    }
                                    FileSaveNameBase = GUILayout.TextField( FileSaveNameBase, new GUILayoutOption[] { GUILayout.ExpandWidth( true ), GUILayout.MinWidth( 140 ) } );
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginVertical();
                                for( int i = 0; i < AssetToggleCollection.Count; i++ )
                                {
                                    HEU_HoudiniAssetRoot assetRoot = AssetToggleCollection[i].RootAsset;

                                    EditorGUILayout.BeginHorizontal();
                                    AssetToggleCollection[i].IsToggle = EditorGUILayout.ToggleLeft( assetRoot.name, toggleJustChange ? ToogleAllAsset : AssetToggleCollection[i].IsToggle );
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.EndHorizontal();
                                }

                                EditorGUILayout.EndVertical();
                            }
                        }
                    }
                    EditorGUILayout.EndToggleGroup();
                }
                EditorGUILayout.EndVertical();
            }

            // -- PRIVATE

            private List<RootAssetToggle> AssetToggleCollection = new List<RootAssetToggle>();

            private bool BakeAsset( int index, string folder_path, out string console_log )
            {
                string log = string.Empty;
                bool result = false;
                if( AssetToggleCollection[index].IsToggle )
                {
                    HEU_HoudiniAssetRoot assetRoot = AssetToggleCollection[index].RootAsset;
                    string assetName = assetRoot.name;

                    //Save
                    string fullpath = CreateFullPath( folder_path, assetName );
                    HEU_AssetPresetUtility.SaveAssetPresetToFile( assetRoot.HoudiniAsset, fullpath );
                    log += fullpath;

                    //Bake
                    BakeAsset( assetRoot, index );

                    result = true;
                }
                console_log = log;
                return result;
            }

            private void BakeAsset( HEU_HoudiniAssetRoot asset_root, int index )
            {
                GameObject assetGameObject = asset_root.gameObject;
                HEU_HoudiniAsset asset = asset_root.HoudiniAsset;
                int siblingIndex = asset_root.transform.GetSiblingIndex();
                asset.BakedDataEvent.AddListener( ( event_data ) =>
                {
                    if( event_data.CookSuccess )
                    {
                        event_data.OutputObjects[0].name = $"{assetGameObject.name}_baked";
                        event_data.OutputObjects[0].transform.SetSiblingIndex( siblingIndex );
                        AssetToggleCollection[index].ToDeleteFlag = true;
                    }
                } );
                SceneManager.SetActiveScene( assetGameObject.scene );
                asset.BakeToNewStandalone();
            }

            private void DeleteTaggedAssetsGamesObject()
            {
                for( int index = AssetToggleCollection.Count - 1; index >= 0; index-- )
                {
                    if( AssetToggleCollection[index].ToDeleteFlag )
                    {
                        DestroyImmediate( AssetToggleCollection[index].RootAsset.gameObject );
                        AssetToggleCollection.RemoveAt( index );
                    }
                }
            }

            private void ButtonSaveClicked()
            {
                string consoleLog = string.Empty;
                int methodReturn = SaveAll( out consoleLog );
                if( methodReturn == -1 )
                {
                    Debug.Log( $"Houdini HDA Save Helper : Save Failed !{Environment.NewLine}{consoleLog}" );
                }
                else
                {
                    Debug.Log( $"Houdini HDA Save Helper : Scene [{Scene.name}]: ({methodReturn} assets saved) {Environment.NewLine}{consoleLog}" );
                }
            }

            private void ButtonRenameClicked()
            {
                string consoleLog = string.Empty;
                int methodReturn = RenameAll( out consoleLog );
                if( methodReturn == -1 )
                {
                    Debug.Log( $"Houdini HDA Save Helper : Rename Failed !{Environment.NewLine}{consoleLog}" );
                }
                else
                {
                    Debug.Log( $"Houdini HDA Save Helper : Scene [{Scene.name}]: ({methodReturn} assets renamed) {Environment.NewLine}{consoleLog}" );
                }
            }

            private void ButtonBakeClicked()
            {
                string consoleLog = string.Empty;
                int methodReturn = BakeAll( out consoleLog );
                if( methodReturn == -1 )
                {
                    Debug.Log( $"Houdini HDA Save Helper : Bake Failed !{Environment.NewLine}{consoleLog}" );
                }
                else
                {
                    Debug.Log( $"Houdini HDA Save Helper : Scene [{Scene.name}]: ({methodReturn} assets baked) {Environment.NewLine}{consoleLog}" );
                    IsSceneEmpty();
                }
            }

            private void ButtonSortClicked()
            {
                SortAll();
                Debug.Log( $"Houdini HDA Save Helper : Scene [{Scene.name}]: assets sorted" );
            }

            private string CreateFullPath( string folder_path, string file_name, string file_extension = "preset" )
            {
                return $"{folder_path}{Path.DirectorySeparatorChar}{file_name}.{file_extension}"; ;
            }

            private string CheckFolderPathValidity( string folder_path, string scene_path, bool allow_create = true )
            {
                string path = null;
                if( !string.IsNullOrEmpty( folder_path ) && !string.IsNullOrEmpty( scene_path ) && Directory.Exists( folder_path ) )
                {
                    char separator = Path.DirectorySeparatorChar;
                    path = $"{ folder_path.TrimEnd( separator )}{separator}{scene_path.TrimEnd( separator )}";
                    if( allow_create && !Directory.Exists( path ) )
                    {
                        Directory.CreateDirectory( path );
                    }
                    if( Directory.Exists( path ) )
                    {
                        IsFolderPathValidFlag = true;
                    }
                    else
                    {
                        IsFolderPathValidFlag = false;
                        path = null;
                    }
                }
                return path;
            }

            // -- INNER CLASSES

            private class RootAssetToggle : IComparable<RootAssetToggle>
            {
                public HEU_HoudiniAssetRoot RootAsset;
                public bool IsToggle;
                public bool ToDeleteFlag;

                public RootAssetToggle( HEU_HoudiniAssetRoot asset, bool is_toggle )
                {
                    RootAsset = asset;
                    IsToggle = is_toggle;
                    ToDeleteFlag = false;
                }

                public int CompareTo( RootAssetToggle other )
                {
                    return string.Compare( RootAsset.gameObject.name, other.RootAsset.gameObject.name, StringComparison.InvariantCulture );
                }
            }
        }
    }
}
