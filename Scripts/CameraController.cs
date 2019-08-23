using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraController : CachedMonoBehaviour
{
    public float TopSpacing = 0f;
    protected Camera fCamera = null;
    protected float fLastTime = 0f;
    
    protected void Refresh()
    {
        Vector2Int map_size = GlobalStorage.Instance.CurrentMap.Size;

        if (map_size == Vector2Int.zero)
            return;

        float ratio = 1f;

        if (Camera.pixelWidth < Camera.pixelHeight)
            ratio = (float)Camera.pixelHeight / Camera.pixelWidth;

        Camera.orthographicSize = (map_size.y + 1 + TopSpacing) * ratio / 2f;
        transform.position = new Vector3(map_size.x / 2f,
                                         (map_size.y + TopSpacing) / 2f,
                                         -10f);
    }

    public Camera Camera
    {
        get
        {
            if (fCamera == null)
                fCamera = GetComponent<Camera>();

            return fCamera;
        }
    }

    private void Update()
    {
        if (Time.time - fLastTime > 1f)
        {
            fLastTime = Time.time;
            Refresh();
        }
    }
}
