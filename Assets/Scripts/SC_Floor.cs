using System;
using UnityEngine;

public class SC_Floor : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log("OnCollisionEnter2D " + col.gameObject.name);
        if (col.gameObject.CompareTag("Player"))
        {
            Debug.Log("Mario Collision!");

            float playerY = col.gameObject.transform.position.y;
            float tileY = transform.position.y;

            Debug.Log(playerY + " " + tileY);
            if (playerY > tileY + 0.45f)
            {
                OnFloorCollision?.Invoke();
            }
        }
    }
    public static event Action OnFloorCollision;
}
