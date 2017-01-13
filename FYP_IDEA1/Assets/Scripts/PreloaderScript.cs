﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PreloaderScript : MonoBehaviour {

    bool loadScene;

    [SerializeField]
    string scene;

    AsyncOperation async;


    void Awake()
    {
        StartCoroutine(LoadScene());
    }
	
	// Update is called once per frame
	void Update () {

        Debug.Log(async.progress);
    }

    IEnumerator LoadScene()
    {
        async = SceneManager.LoadSceneAsync(scene);
        async.allowSceneActivation = false;

        /*if (async.progress >= 100f)
        {
            async.allowSceneActivation = true;
        }*/

        while(!async.isDone)
        {
            yield return null;
        }
    }
}
