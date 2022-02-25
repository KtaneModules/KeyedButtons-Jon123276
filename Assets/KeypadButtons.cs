using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
public enum ColorNames{
    Red,
    Green,
    Blue,
    Yellow,
    Magenta,
    Cyan,
    Black
};
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
    public static Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta, Color.cyan, Color.black };
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
    private IEnumerator bringKeysBack(GameObject keyObject, MeshRenderer renderer){
        keyObject.SetActive(true);
        float duration = 2.0f;
        float elapsed = 0.0f;
        while (elapsed < duration){
            keyObject.transform.localPosition = new Vector3(keyObject.transform.localPosition.x, Mathf.Lerp(0.04837f, 0.0247f, elapsed/duration), keyObject.transform.localPosition.z);
            if (renderer.material.color.a < 0)
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, renderer.material.color.a + 0.005f);
            elapsed += Time.deltaTime;
            yield return null;
        }
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
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    private void Beginning(){
        Debug.LogFormat("[Keyed Buttons #{0}] Starting\n--------------------------------", _moduleId);
        Debug.LogFormat("[Keyed Buttons #{0}] Key Colors in order (TL, TR, BL, BR): ", _moduleId);
        int order = 1;
        foreach(Material keyMat in keyMats){
            int randomValue = (int)Rnd.Range(0, 7); 
            correctValues[order-1] += randomValue; // Row
            keyMat.color = colors[randomValue];
            keyObjects[order-1].GetComponent<MeshRenderer>().material = keyMat;
            Debug.LogFormat("[Keyed Buttons #{0}]Key {1}: {2}", _moduleId, order, (ColorNames)randomValue);
            order++;
        }
        order = 1;
        Debug.LogFormat("[Keyed Buttons #{0}]----------------------------------", _moduleId);
        Debug.LogFormat("[Keyed Buttons #{0}]Button Colors in order (TL, TR, BL, BR): ", _moduleId);
        Debug.LogFormat( correctValues[0] + " " + correctValues[1] + " " + correctValues[2] + " " + correctValues[3]);
        foreach(Material buttonMat in buttonMats){
            int randomValue = (int)Rnd.Range(0, 7); 
            correctValues[order-1] = valuesForTable[correctValues[order-1]][randomValue];
            buttonMat.color = colors[randomValue];
            buttonObjects[order-1].GetComponent<MeshRenderer>().material = buttonMat;
            Debug.LogFormat("[Keyed Buttons #{0}]Button {1}: {2}", _moduleId, order, (ColorNames)randomValue);
            order++;
        }
        order = 1;
        Debug.LogFormat("----------------------------------", _moduleId);
        Debug.LogFormat("Correct Values in order (TL, TR, BL, BR): ", _moduleId);
        foreach(int val in correctValues) {
            Debug.LogFormat("[Keyed Buttons #{0}]Correct value {1}: {2}", _moduleId, order, val);
            order++;
        }
        order = 1;
    }
    private void Awake()
    {
        _moduleId = _moduleIdCounter++;
        keys[0].OnInteract += delegate{
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, keys[0].transform);
            Debug.Log("TL being clicked!");
            StartCoroutine(liftKeys(keyObjects[0], meshRenderers[0]));
            return false;
        };
        keys[1].OnInteract += delegate{
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, keys[1].transform);
            Debug.Log("TR being clicked!");
            StartCoroutine(liftKeys(keyObjects[1], meshRenderers[1]));
            return false;
        };
        keys[2].OnInteract += delegate{
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, keys[2].transform);
            Debug.Log("BL being clicked!");
            StartCoroutine(liftKeys(keyObjects[2], meshRenderers[2]));
            return false;
        };
        keys[3].OnInteract += delegate{
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, keys[3].transform);
            Debug.Log("BR being clicked!");
            StartCoroutine(liftKeys(keyObjects[3], meshRenderers[3]));
            return false;
        };
        buttons[0].OnInteract += delegate{
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[0].transform);
            StartCoroutine(pressButton(buttonObjects[0], meshRenderers[4]));
            pressAtTime(buttons[0], 0);
            return false;
        };
        buttons[1].OnInteract += delegate{
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[1].transform);
            StartCoroutine(pressButton(buttonObjects[1], meshRenderers[5]));
            pressAtTime(buttons[1], 1);
            return false;
        };
        buttons[2].OnInteract += delegate{
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[2].transform);
            StartCoroutine(pressButton(buttonObjects[2], meshRenderers[6]));
            pressAtTime(buttons[2], 2);
            return false;
        };
        buttons[3].OnInteract += delegate{
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[3].transform);
            StartCoroutine(pressButton(buttonObjects[3], meshRenderers[7]));
            pressAtTime(buttons[3], 3);
            return false;
        };
    }
    private void Start(){
        Beginning();
    }
    private void pressAtTime(KMSelectable button, int pos){
        
        int time = (int)BombInfo.GetTime() % 10;
        if (time == correctValues[pos]){
            Debug.LogFormat("[Keyed Buttons #{0}]Pressed at: {1}", _moduleId, time);
            numberOfCorrectButtons++;
        }
        numberOfPresses++;
        Debug.Log("Presses: " + numberOfPresses);
        if (numberOfPresses == 4){
            if (numberOfCorrectButtons != 4){
                Module.OnStrike();
                Debug.LogFormat("[Keyed Buttons #{0}] Strike! Resetting buttons and keys.");
                for (int i = 0; i < keyObjects.Length; i++){
                    StartCoroutine(bringKeysBack(keyObjects[i], meshRenderers[i]));
                }
                correctValues = new int[]{0, 0, 0, 0};
                numberOfCorrectButtons = 0;
                numberOfPresses = 0;
                Beginning();
            }
            else{
                Module.OnPass();
            }
        } 
    }
#pragma warning disable 414
    private readonly string TwitchHelpMessage = "!{0} key {1-4} [Presses 1 of the keys] | !{0} button {1-4} [Presses 1 of the buttons (Should only be used when the key on top of the button is pressed)";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command){
        var parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*key\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)){
            if (parameters.Length < 2) { yield return "sendtochaterror Please specify what key you want to press (1-4)."; yield break;}
            int j;
            if (int.TryParse(parameters[1], out j))
            {
                yield return null;
                j = (j % 4) + 1;
                keys[j-1].OnInteract();
            }
            else
            {
                yield return "sendtochaterror Please specify what key you want to press (1-4).";
                yield break;
            }
        }
        if (Regex.IsMatch(parameters[0], @"^\s*button\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)){
            if (parameters.Length < 2) { yield return "sendtochaterror Please specify what button you want to press (1-4)."; yield break;}
            int j;
            if (int.TryParse(parameters[1], out j))
            {
                yield return null;
                j = (j % 4) + 1;
                buttons[j-1].OnInteract();
            }
            else
            {
                yield return "sendtochaterror Please specify what button you want to press (1-4).";
                yield break;
            }
        }
    }
}
