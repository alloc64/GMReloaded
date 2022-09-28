using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

/// <summary>
/// Auto serialized asset.
/// 
/// Trieda podobna MonoSingletonu, az na to ze nededi z MonoBehaviouru. Tzn uvolni overhead.
/// </summary>
public class AutoSerializedAsset<T> : ScriptableObject  where T : ScriptableObject
{
    protected static T _asset;

    public static T Asset
    {
        get
        {
            if (_asset == null)
            {
                _asset = (T)Resources.Load("TO/" + typeof(T).Name, typeof(T));

#if UNITY_EDITOR
                if (_asset == null)
                {
                    CreateAsset();
                }
#endif

            }
            return _asset;
        }
    }

    /// <summary>
    /// Len pre to aby sme nemuseli tolko refaktorovat, nech to funguje rovnako ako MonoSingletonPresisten.
    /// </summary>
    /// <returns>The instance.</returns>
    public static T GetInstance()
    {
        return Asset;
    }

	public static T Instance
	{
		get
		{
			return GetInstance();
		}
	}

#if UNITY_EDITOR
    public static void CreateAsset()
    {

        //If the settings asset doesn't exist, then create it. We require a resources folder
        
        if (!Directory.Exists(Application.dataPath + "/Resources"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Resources");
        }
        if (!Directory.Exists(Application.dataPath + "/Resources/TO"))
        {
			Directory.CreateDirectory(Application.dataPath + "/Resources/TO");
			Debug.LogWarning("/Resources/TO folder is required to store AutoSerializedAsset. It has been created.");
        }

        var asset = ScriptableObject.CreateInstance<T>();
       
		string uniquePath = "Assets/Resources/TO/" + typeof(T).Name + ".asset";
        AssetDatabase.CreateAsset(asset, uniquePath);
        AssetDatabase.SaveAssets();

        //save reference
        _asset = asset;

    }
#endif
}