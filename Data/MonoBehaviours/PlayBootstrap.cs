using System.Collections;
using TeamCherry.SharedUtils;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using WorldWeaver.Editor;
using static GlobalEnums.GatePosition;

namespace WorldWeaver.Data.MonoBehaviours
{
    public class PlayBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Create()
        {
            if (!WorldWeaverSettings.Instance.PlayBootstrapEnabled)
                return;

            DontDestroyOnLoad(new GameObject(nameof(PlayBootstrap), typeof(PlayBootstrap), typeof(SceneDataFixer), typeof(CheatManagerUI)));
        }

        void Start()
        {
            if (GameObject.FindGameObjectWithTag("SceneManager") == null)
            {
                Debug.LogError($"[PlayBootstrap] Scene '{EditorSceneManager.GetActiveScene().name}' has no object tagged \"SceneManager \" (CustomSceneManager). The save-load flow requires one, so boot was aborted. " +
                    "Pick a scene that contains a SceneManager (e.g. Under_01).");
                return;
            }

            if (GameManager.SilentInstance != null)
                Destroy(gameObject);

            PlayerData pd = PlayerData.instance;

            pd.ResetTempRespawn();
            pd.respawnScene = SceneManager.GetActiveScene().name;
            pd.respawnMarkerName = GetRespawn();

            if (string.IsNullOrEmpty(pd.respawnMarkerName))
            {
                Debug.LogError($"[PlayBootstrap] Boot aborted: no respawn marker was found. Ensure {EditorSceneManager.GetActiveScene().name} contains a valid respawn marker.");
                Destroy(gameObject);
                return;
            }

            if (!pd.respawnScene.StartsWith("Tut_01"))
                pd.bindCutscenePlayed = true; // Fix hp not showing when using empty save

            pd.ActivateTestingCheats(); // 5000 rosaries

            //LoadBootPrefabs();
            StartCoroutine(LoadBootScene());
        }

        private static IEnumerator LoadBootScene()
        {
            string bootScene = WorldWeaverSettings.Instance.PlayBootstrapBootScene;
            var handle = Addressables.LoadSceneAsync($"Scenes/{bootScene}", LoadSceneMode.Single);
            
            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[PlayBootstrap] Failed to load boot scene with key: 'Scenes/{bootScene}'.");
                Debug.LogException(handle.OperationException);
                yield break;
            }

            int frames = 0;
            var timeoutFrames = WorldWeaverSettings.Instance.PlayBootstrapTimeoutFrames;

            while (GameManager.SilentInstance == null)
            {
                if (frames >= timeoutFrames)
                {
                    Debug.LogError($"[PlayBootstrap] Timed out waiting for GameManager to become ready in 'Scenes/{bootScene}'.");
                    yield break;
                }
                    
                frames++;
                yield return null;
            }

            FindFirstObjectByType<UIManager>()?.mainMenuButtons.gameObject.SetActive(false);
            GameManager.instance.ContinueGame();
        }

        private static void LoadBootPrefabs()
        {
            if (WorldWeaverSettings.Instance.PlayBootstrapBootGameManagerPrefab == null)
            {
                Debug.LogError("[PlayBootstrap] GameManager prefab is null on WorldWeaverSettings");
                return;
            }

            EditorSceneManager.CreateScene("PlayBootstrap_Loader");
            EditorSceneManager.UnloadSceneAsync(EditorSceneManager.GetActiveScene());

            var gm = Instantiate(WorldWeaverSettings.Instance.PlayBootstrapBootGameManagerPrefab)!;
            var ui = Instantiate(WorldWeaverSettings.Instance.PlayBootstrapBootUIManagerPrefab)!;
            Instantiate(WorldWeaverSettings.Instance.PlayBootstrapBootGameCamerasPrefab);

            ui.GetComponent<UIManager>().mainMenuScreen.gameObject.SetActive(false);
            gm.GetComponent<GameManager>().ContinueGame();
        }

        string GetRespawn()
        {
            GameObject tagged = GameObject.FindGameObjectWithTag("RespawnPoint");

            if (tagged == null)
                tagged = GameObject.FindGameObjectWithTag("Respawn");

            if (tagged != null && tagged.TryGetComponent<RespawnMarker>(out var marker))
            {
                EnsureMarkerDefaults(marker);
                return marker.name;
            }

            RespawnMarker existing = FindFirstObjectByType<RespawnMarker>(FindObjectsInactive.Include);
            if (existing != null)
            {
                EnsureMarkerDefaults(existing);
                return existing.name;
            }

            var gate = FindFirstObjectByType<TransitionPoint>(FindObjectsInactive.Include);
            if (gate == null)
            {
                HazardRespawnMarker hazardRespawn = FindFirstObjectByType<HazardRespawnMarker>(FindObjectsInactive.Include);
                if (hazardRespawn != null)
                {
                    EnsureMarkerDefaults(gameObject.AddComponent<RespawnMarker>());
                    transform.position = hazardRespawn.transform.position;
                    return name;
                }

                Debug.LogError($"[PlayBootstrap] Scene '{EditorSceneManager.GetActiveScene().name}' has no respawn marker or transition point.");
                return string.Empty;
            }

            EnsureMarkerDefaults(gameObject.AddComponent<RespawnMarker>());
            bool faceRight = false;

            if (gate.alwaysEnterRight)
                faceRight = true;

            if (gate.alwaysEnterLeft)
                faceRight = false;
            
            var gatePos = gate.GetGatePosition();
            Vector3 offset;

            if (gatePos == bottom)
                offset = new(faceRight ? 3f : -3f, y: 3.94f);

            else if (gatePos is right or left)
                offset = new(faceRight ? 3f : -3f, 0);
            else 
                offset = Vector3.zero;

            var position = offset + gate.transform.position;
            
            RaycastHit2D closestHit = new()
            {
                point = new()
            };

            if (gatePos is top or door || Helper.IsRayHittingNoTriggers(position, Vector2.down, 10, 8448, out closestHit) || Helper.IsRayHittingNoTriggers(position = offset * -1 + gate.transform.position, Vector2.down, 10, 8448, out closestHit))
            {
                transform.position = new Vector2(closestHit.point.x, closestHit.point.y + (float)(1.0394495f - (-0.5132368f) + 0.01f));
                return name;
            }

            Debug.LogError($"[PlayBootstrap] Could not find ground point below {position}.");
            return string.Empty;
        }

        static void EnsureMarkerDefaults(RespawnMarker marker)
        {
            if (marker.customFadeDuration == null)
                marker.customFadeDuration = new OverrideFloat();

            if (marker.overrideMapZone == null)
                marker.overrideMapZone = new OverrideMapZone();
        }
    }   
}