
using UnityEngine;
using UnityEngine.UIElements;
using MyGameNamespace;

[DisallowMultipleComponent]
public class MainGameController : MonoBehaviour
{
    [SerializeField] private UIDocument gameUIDocument;

    private VisualElement root;
    private Label storyText;
    private VisualElement choicesPanel;

    private void Awake()
    {
        if (gameUIDocument == null)
            gameUIDocument = GetComponent<UIDocument>();

        root = gameUIDocument ? gameUIDocument.rootVisualElement : null;
        if (root == null)
        {
            Debug.LogWarning("[MainGameController] No UIDocument / rootVisualElement found.");
            return;
        }

        storyText = root.Q<Label>("StoryText");
        choicesPanel = root.Q<VisualElement>("ChoicesPanel");

        // Try to bind to GameManager player/story
        var gm = GameManager.Instance ?? FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            var player = gm.GetPlayer(); // <-- use method; not a property
            // Fallback to PlayerState.Current if GameManager has no player yet
            if (player == null)
            {
                player = MyGameNamespace.PlayerState.Current;
            }
            if (gm.StoryManager != null && player != null)
            {
                gm.StoryManager.SetPlayer(player);
            }
        }
    }
}
