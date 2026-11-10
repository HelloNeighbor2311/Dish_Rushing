using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Wires the prototype scene after scripts reload, without Inspector setup.</summary>
[InitializeOnLoad]
public static class DishWashingSceneSetup
{
    static DishWashingSceneSetup() { EditorApplication.delayCall += Install; }

    [MenuItem("Tools/Dish Rushing/Set up dish washing")]
    private static void Install()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity") return;
        Player player = Object.FindFirstObjectByType<Player>();
        if (player == null || player.gameObject.scene != scene) return;
        bool changed = false;
        if (player.GetComponent<PlayerInteractor>() == null)
        {
            Undo.AddComponent<PlayerInteractor>(player.gameObject);
            changed = true;
        }
        Dishwasher station = Object.FindFirstObjectByType<Dishwasher>();
        if (station == null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Dishwasher.prefab");
            if (prefab == null) prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dishwasher.prefab");
            GameObject obj = prefab != null ? (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene) : new GameObject("Dishwasher");
            if (obj.GetComponent<Dishwasher>() == null) obj.AddComponent<Dishwasher>();
            if (obj.GetComponent<Collider2D>() == null) obj.AddComponent<BoxCollider2D>();
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) obj.layer = layer;
            obj.transform.position = new Vector3(10, 0, 0);
            Undo.RegisterCreatedObjectUndo(obj, "Add dishwasher");
            changed = true;
        }
        if (changed)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Dish washing ready: approach the blue dishwasher and press E. Escape returns to the restaurant.");
        }
    }
}
