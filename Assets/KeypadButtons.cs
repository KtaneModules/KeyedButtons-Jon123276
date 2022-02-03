using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class KeypadButtons : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;
    public KMSelectable[] keys;
    public GameObject[] keyObjects;
    public GameObject[] buttonObjects;
    public MeshRenderer[] meshRenderers;
    public Material[] buttonMats;
    public Material[] keyMats;
    public KMSelectable[] buttons;
    public static Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan, Color.white };
    public static int[][] valuesForTable = new int[][]{
        new int[]{0,1,2,3,4,5,6},
        new int[]{6,0,1,2,3,4,5},
        new int[]{5,6,0,1,2,3,4},
        new int[]{4,5,6,0,1,2,3},
        new int[]{3,4,5,6,0,1,2},
        new int[]{2,3,4,5,6,0,1},
        new int[]{1,2,3,4,5,6,0}
    };
    private int[] correctValues = {0, 0, 0, 0};
    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;
    int numberOfCorrectButtons = 0;
    int numberOfPresses = 0;
    private IEnumerator liftKeys(GameObject keyObject, MeshRenderer renderer){
        float duration = 2.0f;
        float elapsed = 0.0f;
        while (elapsed < duration){
            keyObject.transform.localPosition = new Vector3(keyObject.transform.localPosition.x, Mathf.Lerp(0.0247f, 0.04837f, elapsed/duration), keyObject.transform.localPosition.z);
            if (renderer.material.color.a > 0)
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, renderer.material.color.a - 0.005f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        keyObject.SetActive(false);
    }
	private IEnumerator pressButton(GameObject buttonObject, MeshRenderer renderer){
        float duration = 1.0f;
        float elapsed = 0.0f;
        while (elapsed < duration){
            buttonObject.transform.localPosition = new Vector3(buttonObject.transform.localPosition.x, Mathf.Lerp(0.01f, 0.005f, elapsed/duration), buttonObject.transform.localPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        duration = 1.0f;
        elapsed = 0.0f;
        while (elapsed < duration){
            buttonObject.transform.localPosition = new Vector3(buttonObject.transform.localPosition.x, Mathf.Lerp(0.005f, 0.01f, elapsed/duration), buttonObject.transform.localPosition.z);
            if (renderer.material.color.a > 0)
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, renderer.material.color.a - 0.005f);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        Debug.Log("Starting\n--------------------------------");
        Debug.Log("Key Colors in order (TL, TR, BL, BR): ");
        int order = 1;
        foreach(Material keyMat in keyMats){
            int randomValue = (int)Rnd.Range(0, 6f); 
            correctValues[order-1] += randomValue;
            keyMat.color = colors[randomValue];
            Debug.Log("Key " + order + ": " + keyMat.color);
            order++;
        }
        order = 1;
        Debug.Log("----------------------------------");
        Debug.Log("Button Colors in order (TL, TR, BL, BR): ");
        foreach(Material buttonMat in buttonMats){
            int randomValue = (int)Rnd.Range(0, 6f); 
            correctValues[order-1] += randomValue;
            buttonMat.color = colors[randomValue];
            Debug.Log("Button " + order + ": " + buttonMat.color);
            order++;
        }
        order = 1;
        Debug.Log("----------------------------------");
        Debug.Log("Correct Values in order (TL, TR, BL, BR): ");
        foreach(int val in correctValues) {
            Debug.Log("Correct value " + order + ": " + val);
            order++;
        }
        order = 1;
        keys[0].OnInteract += delegate{
            Debug.Log("TL being clicked!");
            StartCoroutine(liftKeys(keyObjects[0], meshRenderers[0]));
            return false;
        };
        keys[1].OnInteract += delegate{
            Debug.Log("TR being clicked!");
            StartCoroutine(liftKeys(keyObjects[1], meshRenderers[1]));
            return false;
        };
        keys[2].OnInteract += delegate{
            Debug.Log("BL being clicked!");
            StartCoroutine(liftKeys(keyObjects[2], meshRenderers[2]));
            return false;
        };
        keys[3].OnInteract += delegate{
            Debug.Log("BR being clicked!");
            StartCoroutine(liftKeys(keyObjects[3], meshRenderers[3]));
            return false;
        };
        buttons[0].OnInteract += delegate{
            StartCoroutine(pressButton(buttonObjects[0], meshRenderers[4]));
            pressAtTime(buttons[0], 0);
            return false;
        };
        buttons[1].OnInteract += delegate{
            StartCoroutine(pressButton(buttonObjects[1], meshRenderers[5]));
            pressAtTime(buttons[1], 1);
            return false;
        };
        buttons[2].OnInteract += delegate{
            StartCoroutine(pressButton(buttonObjects[2], meshRenderers[6]));
            pressAtTime(buttons[2], 2);
            return false;
        };
        buttons[3].OnInteract += delegate{
            StartCoroutine(pressButton(buttonObjects[3], meshRenderers[7]));
            pressAtTime(buttons[3], 3);
            return false;
        };
    }

    private void pressAtTime(KMSelectable button, int pos){
        
        int time = (int)BombInfo.GetTime() % 10;
        if (time == correctValues[pos]){
            Debug.Log("Pressed at: " + time);
            numberOfCorrectButtons++;
        }
        numberOfPresses++;
        Debug.Log("Presses: " + numberOfPresses);
        if (numberOfPresses == 4){
            if (numberOfCorrectButtons != 4){
                Module.OnStrike();
            }
            else{
                Module.OnPass();
            }
        } 
    }
}
