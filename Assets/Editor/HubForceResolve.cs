
      using System.IO;
      using UnityEditor;
      using UnityEditor.PackageManager;

      [InitializeOnLoad]
      public static class HubForceResolve
      {
          private const string k_ScriptPath = "Assets\\Editor\\HubForceResolve.cs";
          private const string k_EditorFolderPath = "Assets\\Editor";
          private const string k_StateKey = "Hub.ForceResolve.State";
          private const string k_ManifestPath = "Packages/manifest.json";
          private const string k_ManifestContent = "{\n  \"dependencies\": {\n    \"com.cysharp.unitask\": \"https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask\",\n    \"com.hellodev.bootstrap\": \"https://github.com/joaovictoralencar/hellodev-bootstrap.git\",\n    \"com.hellodev.conditions\": \"https://github.com/joaovictoralencar/hellodev-conditions.git\",\n    \"com.hellodev.events\": \"https://github.com/joaovictoralencar/hellodev-events.git\",\n    \"com.hellodev.ids\": \"https://github.com/joaovictoralencar/hellodev-ids.git\",\n    \"com.hellodev.loaders\": \"https://github.com/joaovictoralencar/hellodev-loaders.git\",\n    \"com.hellodev.saving\": \"https://github.com/joaovictoralencar/com.hellodev.saving.git\",\n    \"com.hellodev.ui\": \"https://github.com/joaovictoralencar/hellodev-ui.git\",\n    \"com.hellodev.utils\": \"https://github.com/joaovictoralencar/hellodev-utils.git#v1.1.0\",\n    \"com.kyrylokuzyk.primetween\": \"file:../Assets/Plugins/PrimeTween/internal/com.kyrylokuzyk.primetween.tgz\",\n    \"com.unity.2d.sprite\": \"1.0.0\",\n    \"com.unity.addressables\": \"2.9.1\",\n    \"com.unity.ai.navigation\": \"2.0.14\",\n    \"com.unity.cinemachine\": \"3.1.7\",\n    \"com.unity.collab-proxy\": \"2.12.4\",\n    \"com.unity.editorcoroutines\": \"1.1.0\",\n    \"com.unity.ide.rider\": \"3.0.40\",\n    \"com.unity.ide.visualstudio\": \"2.0.26\",\n    \"com.unity.inputsystem\": \"1.19.0\",\n    \"com.unity.localization\": \"1.5.12\",\n    \"com.unity.multiplayer.center\": \"1.0.1\",\n    \"com.unity.nuget.newtonsoft-json\": \"3.2.2\",\n    \"com.unity.render-pipelines.universal\": \"17.5.0\",\n    \"com.unity.test-framework\": \"1.7.0\",\n    \"com.unity.timeline\": \"1.8.12\",\n    \"com.unity.ugui\": \"2.5.0\",\n    \"com.unity.visualscripting\": \"1.9.11\",\n    \"com.unity.modules.accessibility\": \"1.0.0\",\n    \"com.unity.modules.adaptiveperformance\": \"1.0.0\",\n    \"com.unity.modules.ai\": \"1.0.0\",\n    \"com.unity.modules.androidjni\": \"1.0.0\",\n    \"com.unity.modules.animation\": \"1.0.0\",\n    \"com.unity.modules.assetbundle\": \"1.0.0\",\n    \"com.unity.modules.audio\": \"1.0.0\",\n    \"com.unity.modules.cloth\": \"1.0.0\",\n    \"com.unity.modules.director\": \"1.0.0\",\n    \"com.unity.modules.imageconversion\": \"1.0.0\",\n    \"com.unity.modules.imgui\": \"1.0.0\",\n    \"com.unity.modules.jsonserialize\": \"1.0.0\",\n    \"com.unity.modules.particlesystem\": \"1.0.0\",\n    \"com.unity.modules.physics\": \"1.0.0\",\n    \"com.unity.modules.physics2d\": \"1.0.0\",\n    \"com.unity.modules.physicscore2d\": \"1.0.0\",\n    \"com.unity.modules.screencapture\": \"1.0.0\",\n    \"com.unity.modules.terrain\": \"1.0.0\",\n    \"com.unity.modules.terrainphysics\": \"1.0.0\",\n    \"com.unity.modules.tilemap\": \"1.0.0\",\n    \"com.unity.modules.ui\": \"1.0.0\",\n    \"com.unity.modules.uielements\": \"1.0.0\",\n    \"com.unity.modules.umbra\": \"1.0.0\",\n    \"com.unity.modules.unityanalytics\": \"1.0.0\",\n    \"com.unity.modules.unitywebrequest\": \"1.0.0\",\n    \"com.unity.modules.unitywebrequestassetbundle\": \"1.0.0\",\n    \"com.unity.modules.unitywebrequestaudio\": \"1.0.0\",\n    \"com.unity.modules.unitywebrequesttexture\": \"1.0.0\",\n    \"com.unity.modules.unitywebrequestwww\": \"1.0.0\",\n    \"com.unity.modules.vectorgraphics\": \"1.0.0\",\n    \"com.unity.modules.vehicles\": \"1.0.0\",\n    \"com.unity.modules.video\": \"1.0.0\",\n    \"com.unity.modules.wind\": \"1.0.0\",\n    \"com.unity.modules.xr\": \"1.0.0\"\n  }\n}\n";

          static HubForceResolve()
          {
              EditorApplication.delayCall += Tick;
          }

          static void Tick()
          {
              // Wait until the Editor is idle so the resolve/self-delete don't fight the import.
              if (EditorApplication.isCompiling || EditorApplication.isUpdating)
              {
                  EditorApplication.delayCall += Tick;
                  return;
              }

              if (SessionState.GetString(k_StateKey, "") == "")
              {
                  // Write the intended manifest inside the Editor process — this is *after*
                  // the Editor's own template-generation and UPM update-dependencies writes,
                  // so nothing can race with (and clobber) the value we put on disk. See
                  // HUB-7048. Guarded so a write failure (locked file, read-only perms, AV on
                  // Windows) still lets the resolve + self-delete run — the project degrades to
                  // the Editor-generated manifest instead of leaving a stray script behind.
                  try
                  {
                      File.WriteAllText(k_ManifestPath, k_ManifestContent);
                  }
                  catch (System.Exception e)
                  {
                      UnityEngine.Debug.LogWarning("[Unity Hub] Failed to restore captured Packages/manifest.json: " + e.Message);
                  }
                  // First idle pass: resolve. May reload, after which Tick runs again.
                  SessionState.SetString(k_StateKey, "resolved");
                  Client.Resolve();
                  EditorApplication.delayCall += Tick;
                  return;
              }

              // Resolved and idle again → remove the script.
              Cleanup();
          }

          static void Cleanup()
          {
              AssetDatabase.DeleteAsset(k_ScriptPath);
              CleanupEmptyEditorFolder();
          }

          static void CleanupEmptyEditorFolder()
          {
              string absolutePath = Path.GetFullPath(k_EditorFolderPath);

              if (!Directory.Exists(absolutePath)) return;

              string[] files = Directory.GetFiles(absolutePath);
              string[] dirs = Directory.GetDirectories(absolutePath);

              bool isEmpty = true;

              if (dirs.Length > 0)
              {
                  isEmpty = false;
              }

              foreach (string file in files)
              {
                  if (!file.EndsWith(".DS_Store"))
                  {
                      isEmpty = false;
                      break;
                  }
              }

              if (isEmpty)
              {
                  AssetDatabase.DeleteAsset(k_EditorFolderPath);
              }
          }
      }