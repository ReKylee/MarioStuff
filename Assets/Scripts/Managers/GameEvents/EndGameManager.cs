using System.Collections;
using Managers;
using UnityEngine;

public class EndGameManager : MonoBehaviour
{
    public void EndGame()
    {
        StartCoroutine(EndGameRoutine());
    }
    private IEnumerator EndGameRoutine()
    {
        Debug.Log("Game Won! Closing in 5 seconds.");
        yield return new WaitForSeconds(5f);
        ResetManager.Instance?.ResetAll();
    }
}
