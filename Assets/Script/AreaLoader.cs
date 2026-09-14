using UnityEngine;
using UnityEngine.SceneManagement;
public class AreaLoader : MonoBehaviour
{
    [Tooltip("Nama scene yang akan dimuat secara Additive, harus sama persis dengan nama file scene (case-sensitive)")]
    public string sceneToLoad = "Farm";

    [Tooltip("Tag GameObject player, dipakai untuk memastikan yang memicu trigger ini benar-benar player, bukan objek lain")]
    public string playerTag = "Player";

    [Tooltip("Jika true, scene otomatis di-unload lagi saat player keluar dari trigger. Jika false, scene tetap dimuat selamanya setelah pertama kali masuk.")]
    public bool unloadOnExit = true;

    private bool isSceneLoaded = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (isSceneLoaded) return; // hindari load dobel jika trigger kena 2x

        StartCoroutine(LoadFarmScene());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (!unloadOnExit) return;
        if (!isSceneLoaded) return;

        StartCoroutine(UnloadFarmScene());
    }

    private System.Collections.IEnumerator LoadFarmScene()
    {
        isSceneLoaded = true;
        Debug.Log($"[AreaLoader] Memuat scene '{sceneToLoad}' secara Additive...");

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
        yield return loadOp;

        Debug.Log($"[AreaLoader] Scene '{sceneToLoad}' selesai dimuat.");
    }

    private System.Collections.IEnumerator UnloadFarmScene()
    {
        Debug.Log($"[AreaLoader] Melepas scene '{sceneToLoad}'...");

        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneToLoad);
        yield return unloadOp;

        isSceneLoaded = false;
        Debug.Log($"[AreaLoader] Scene '{sceneToLoad}' selesai dilepas.");
    }
}