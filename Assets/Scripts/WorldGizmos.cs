using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WorldManager))]
[ExecuteInEditMode]
public class WorldGizmos : MonoBehaviour
{
    [SerializeField] private bool _drawGizmos;

    private WorldManager _worldManager;
    private Vector3Int _worldCenter;
    private Vector3Int _worldSize;
    private int _centerRadius;
    private List<ZoneData> _zonesData;

    private void Awake()
    {
        _worldManager = GetComponent<WorldManager>();
    }

    private void UpdateInfo()
    {
        _worldSize = _worldManager.GetWorldSize();
        _centerRadius = _worldManager.GetCenterRadius();
        _zonesData = _worldManager.GetZonesData();

        _worldCenter = new Vector3Int(0, _worldSize.y / -2, 0);
    }

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        UpdateInfo();

        DrawCenter();
        DrawWorld();
        DrawZones();
    }

    private void DrawCenter()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(_worldCenter, new Vector3(_centerRadius * 2, _worldSize.y, _centerRadius * 2));
    }

    private void DrawZones()
    {
        Gizmos.color = Color.red;

        for (int i = 0; i < _zonesData.Count; i++)
        {
            int size = _zonesData[i].Radius;
            Gizmos.DrawWireCube(_worldCenter, new Vector3(size * 2, _worldSize.y, size * 2));
        }
    }

    private void DrawWorld()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_worldCenter, _worldSize);
    }
}
