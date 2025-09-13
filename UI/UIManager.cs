using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent] // optional: prevents duplicates, but DOES NOT force UIDocument
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Optional")]
    [Tooltip("If assigned, UIManager will use this document as its Root. Otherwise it will try to GetComponent<UIDocument>().")]
    [SerializeField] private UIDocument mainDocument;

    /// <summary>Root visual of the active document (may be null if you keep UIDocuments on other objects).</summary>
    public VisualElement Root { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // If no document assigned, try to read one on this GO (but it's optional)
        if (mainDocument == null)
            TryGetComponent(out mainDocument);

        Root = mainDocument ? mainDocument.rootVisualElement : null;

        // You can put any global UI setup in here if you keep a document on this GO.
        // Otherwise, leave it empty and manage per-view controllers (UIController, CharacterSheetController, etc.).
    }

    /// <summary>True if Root is available (only when a UIDocument is set).</summary>
    public bool IsReady() => Root != null;

    /// <summary>Assign/replace the document at runtime (e.g., from CompositionRoot when a scene loads).</summary>
    public void SetDocument(UIDocument doc)
    {
        mainDocument = doc;
        Root = doc ? doc.rootVisualElement : null;
    }
}
