using UnityEngine;

public class HexGrid : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public float hexSize = 1f;

    void Start()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 hexPos = HexToWorld(x, y);
                // Visual marker for the hex grid
                Debug.DrawLine(hexPos, hexPos + Vector3.up * 2, Color.red, 100f);
            }
        }
    }

    Vector3 HexToWorld(int x, int y)
    {
        float xPos = x * hexSize * 1.5f;
        float yPos = y * hexSize * Mathf.Sqrt(3) * (x % 2 == 0 ? 1f : 0.5f);
        return new Vector3(xPos, 0, yPos);
    }
}