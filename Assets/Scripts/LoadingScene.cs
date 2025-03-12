using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public GameObject loadingScreen;
    public GameObject startButton,quitButton,title,volumeSliders;

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        LeanTween.moveLocalX(startButton,-700f,1f).setEase(LeanTweenType.easeOutQuart);
        LeanTween.moveLocalX(quitButton,-700f,1f).setEase(LeanTweenType.easeOutQuart);
        LeanTween.moveLocalX(volumeSliders,-700f,1f).setEase(LeanTweenType.easeOutQuart);
        LeanTween.moveLocalY(title,10.5f,1f).setEase(LeanTweenType.easeInOutQuart);
        loadingScreen.transform.localScale = Vector3.zero;
        loadingScreen.SetActive(true);
        loadingScreen.transform.LeanScale(Vector3.one,1f).setEase(LeanTweenType.easeOutExpo);

        yield return new WaitForSeconds(2f);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        
        while (!operation.isDone)
        {
            yield return null;
        }
    }
}
