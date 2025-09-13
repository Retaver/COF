using UnityEngine; using UnityEngine.SceneManagement;
[DisallowMultipleComponent] public class GameSceneManager:MonoBehaviour{
[SerializeField] string mainMenuScene="MainMenu", creationScene="Charactercreation", gameScene="Game", combatScene="Combat";
 public void ShowMainMenu()=>LoadSceneSafe(mainMenuScene);
 public void ShowCharacterCreate()=>LoadSceneSafe(creationScene);
 public void ShowGame()=>LoadSceneSafe(gameScene);
 public void ShowCombat()=>LoadSceneSafe(combatScene);
 public void TransitionToScreen(string screen){
  if(string.IsNullOrWhiteSpace(screen)) return; string k=screen.Trim().ToLowerInvariant();
  if(k==mainMenuScene.ToLowerInvariant()||k=="mainmenu"){ ShowMainMenu(); return; }
  if(k==creationScene.ToLowerInvariant()||k=="charactercreation"||k=="charcreate"){ ShowCharacterCreate(); return; }
  if(k==gameScene.ToLowerInvariant()||k=="game"){ ShowGame(); return; }
  if(k==combatScene.ToLowerInvariant()||k=="combat"){ ShowCombat(); return; }
  LoadSceneSafe(screen);
 }
 void LoadSceneSafe(string sceneName){ if(string.IsNullOrEmpty(sceneName)) return; if(Application.CanStreamedLevelBeLoaded(sceneName)) SceneManager.LoadScene(sceneName,LoadSceneMode.Single); else Debug.LogError($"[GameSceneManager] Scene '{sceneName}' not in Build Settings."); }
 public void ToggleCharacterSheet(){
  // Use FindFirstObjectByType in Unity 6.x instead of the deprecated FindObjectOfType
  var sheet = FindFirstObjectByType<CharacterSheetController>();
  if (sheet == null)
  {
    Debug.LogWarning("[GameSceneManager] CharacterSheetController not found");
    return;
  }
  sheet.ToggleCharacterSheet();
 }
 public void ToggleInventory(){ Debug.Log("[GameSceneManager] ToggleInventory (stub)."); }
 public void ToggleInventory(bool show){ Debug.Log("[GameSceneManager] ToggleInventory(bool) (stub)."); }
}
