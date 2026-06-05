using System.Collections;
using System.Collections.Generic;
using LunarConsolePlugin;
using UnityEngine;

public class ShowConsole : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(WaitAndShow());
    }

    private IEnumerator WaitAndShow()
    {
        yield return new WaitForSeconds(1);
        LunarConsole.Show();
        Debug.Log("Hello World!");
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
