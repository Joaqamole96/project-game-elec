// FloorController.cs
using UnityEngine;
using FGA.Core;
using FGA.Presentation;

[RequireComponent(typeof(FloorGenerator_Orchestrator))]
public class FloorController : MonoBehaviour
{
    public FloorGenerator_Orchestrator orchestrator;
    public FloorView view;

    void Reset() { orchestrator = GetComponent<FloorGenerator_Orchestrator>(); }

    [ContextMenu("Generate & Render")]
    public void GenerateAndRender()
    {
        if (orchestrator == null || view == null) { Debug.LogWarning("Missing references"); return; }
        orchestrator.Generate();
        view.Render(orchestrator.model);
    }
}