using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private WorldGenerator _worldGenerator;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _rayDistance = 15;
    [SerializeField] private int _explosionRadius = 5;

    private void Update()
    {
        bool mouseDown0 = Input.GetMouseButton(0);
        bool mouseDown1 = Input.GetMouseButton(1);
        _lineRenderer.gameObject.SetActive(mouseDown0 || mouseDown1);

        if (mouseDown0) ModifyTerrain(false);
        if (mouseDown1) ModifyTerrain(true);
    }

    private void ModifyTerrain(bool add)
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, _rayDistance, 1 << LayerMask.NameToLayer("Terrain")) &&
            hit.distance > _explosionRadius)
        {
            _worldGenerator.ModifyTerrain(hit.point, _explosionRadius, add);
        }

        float distance = hit.distance == 0 ? _rayDistance : hit.distance;
        _lineRenderer.SetPosition(0, transform.position);
        _lineRenderer.SetPosition(1, transform.position + (transform.forward * distance));
    }
}
