using Unity.AI.Navigation;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshRuntimeBuilder : MonoBehaviour
{
    [SerializeField] private NavMeshSurface surface;

    private void Awake()
    {
        surface.BuildNavMesh();
    }

    private void Reset()
    {
        surface = GetComponent<NavMeshSurface>();
    }
}
