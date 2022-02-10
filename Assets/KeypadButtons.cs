using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using System.Text.RegularExpressions;

public class KeypadButtons : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo BombInfo;
    public KMAudio Audio;

    public KMSelectable[] KeySels;
    public KMSelectable[] ButtonSels;
    public GameObject[] KeyObjs;
    public GameObject[] ButtonCapObjs;
    public GameObject[] KeyParents;
    public Material[] ColorMats;

    private static readonly string[] _colornames = new string[7] { "RED", "GREEN", "BLUE", "YELLOW", "CYAN", "MAGENTA", "BLACK" };
    private static readonly int[][] _talbeValues = new int[7][] {
        new int[7] { 0, 1, 2, 3, 4, 5, 6 },
        new int[7] { 6, 0, 1, 2, 3, 4, 5 },
        new int[7] { 5, 6, 0, 1, 2, 3, 4 },
        new int[7] { 4, 5, 6, 0, 1, 2, 3 },
        new int[7] { 3, 4, 5, 6, 0, 1, 2 },
        new int[7] { 2, 3, 4, 5, 6, 0, 1 },
        new int[7] { 1, 2, 3, 4, 5, 6, 0 }
    };
    private int[] _correctValues = new int[4];
    private int _moduleId;
    private static int _moduleIdCounter = 1;
    private bool _moduleSolved;

    private int[] _keyColors = new int[4];
    private int[] _buttonColors = new int[4];
    private bool[] _isAnimating = new bool[4];
    private bool[] _correctlyPressed = new bool[4];
    private Coroutine[] _keyAnims = new Coroutine[4];

    private void Start()
    {
        _moduleId = _moduleIdCounter++;
        Generate();
        for (int i = 0; i < 4; i++)
        {
            KeySels[i].OnInteract += KeyPress(i);
            ButtonSels[i].OnInteract += ButtonPress(i);
        }
    }

    private KMSelectable.OnInteractHandler KeyPress(int key)
    {
        return delegate ()
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            KeySels[key].AddInteractionPunch(0.5f);
            if (_moduleSolved || _isAnimating[key])
                return false;
            _keyAnims[key] = StartCoroutine(MoveKey(key));
            return false;
        };
    }

    private KMSelectable.OnInteractHandler ButtonPress(int btn)
    {
        return delegate ()
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            ButtonSels[btn].AddInteractionPunch(0.5f);
            StartCoroutine(MoveButton(btn));
            if (_moduleSolved)
                return false;
            var curTime = (int)BombInfo.GetTime() % 10;
            if (_correctValues[btn] == curTime)
            {
                Debug.LogFormat("[Keyed Buttons #{0}] Correctly pressed the {1} button on a {2}.", _moduleId, btn == 0 ? "top left" : btn == 1 ? "top right" : btn == 2 ? "bottom left" : "bottom right", _correctValues[btn]);
                _correctlyPressed[btn] = true;
                if (!_correctlyPressed.Contains(false))
                {
                    _moduleSolved = true;
                    Debug.LogFormat("[Keyed Buttons #{0}] All buttons have been correctly pressed. Module solved.", _moduleId);
                    StartCoroutine(SolveAnimation());
                }
            }
            else
            {
                Debug.LogFormat("[Keyed Buttons #{0}] Incorrectly pressed the {1} button on a {2}, instead of a {3}. Strike.", _moduleId, btn == 0 ? "top left" : btn == 1 ? "top right" : btn == 2 ? "bottom left" : "bottom right", curTime, _correctValues[btn]);
                Module.HandleStrike();
                Generate();
            }
            return false;
        };
    }

    private void Generate()
    {
        _isAnimating = new bool[4];
        _correctlyPressed = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            if (_keyAnims[i] != null)
                StopCoroutine(_keyAnims[i]);
            KeyParents[i].transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            _keyColors[i] = Rnd.Range(0, 7);
            _buttonColors[i] = Rnd.Range(0, 7);
            KeyObjs[i].GetComponent<MeshRenderer>().sharedMaterial = ColorMats[_keyColors[i]];
            ButtonCapObjs[i].GetComponent<MeshRenderer>().sharedMaterial = ColorMats[_buttonColors[i]];
            _correctValues[i] = _talbeValues[_buttonColors[i]][_keyColors[i]];
        }
        Debug.LogFormat("[Keyed Buttons #{0}] The colors of the keys are: {1}.", _moduleId, _keyColors.Select(i => _colornames[i]).Join(", "));
        Debug.LogFormat("[Keyed Buttons #{0}] The colors of the buttons are: {1}.", _moduleId, _buttonColors.Select(i => _colornames[i]).Join(", "));
        Debug.LogFormat("[Keyed Buttons #{0}] Time times to press the buttons are: {1}.", _moduleId, _correctValues.Join(", "));
    }

    private IEnumerator MoveKey(int key)
    {
        _isAnimating[key] = true;
        var duration = 0.7f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            KeyParents[key].transform.localEulerAngles = new Vector3(Easing.InOutQuad(elapsed, 0f, key <= 1 ? 120f : -120f, duration), 0f, 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }
        KeyParents[key].transform.localEulerAngles = new Vector3(key <= 1 ? 120f : -120f, 0f, 0f);
    }

    private IEnumerator MoveButton(int btn)
    {
        var duration = 0.1f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            ButtonCapObjs[btn].transform.localPosition = new Vector3(0f, Easing.InOutQuad(elapsed, 0.01f, 0.005f, duration), 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            ButtonCapObjs[btn].transform.localPosition = new Vector3(0f, Easing.InOutQuad(elapsed, 0.005f, 0.01f, duration), 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }
        ButtonCapObjs[btn].transform.localPosition = new Vector3(0f, 0.01f, 0f);
    }

    private IEnumerator SolveAnimation()
    {
        var duration = 0.7f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            for (int key = 0; key < 4; key++)
                KeyParents[key].transform.localEulerAngles = new Vector3(Easing.InOutQuad(elapsed, key <= 1 ? 120f : -120f, 0f, duration), 0f, 0f);
            yield return null;
            elapsed += Time.deltaTime;
        }
        for (int key = 0; key < 4; key++)
            KeyParents[key].transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        Module.HandlePass();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} key 1 [Presses key 1.] | !{0} button 3 at 9 [Presses button 3 at a 9.] | Keys and buttons are numbered in reading order.";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        var m = Regex.Match(command, @"^\s*key\s*([1-4])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            yield return null;
            int val;
            if (!int.TryParse(m.Groups[1].Value, out val))
                yield break;
            if (_isAnimating[val - 1])
            {
                yield return "sendtochaterror Key " + val + " has already been pressed!";
                yield break;
            }
            KeySels[val - 1].OnInteract();
            yield break;
        }
        m = Regex.Match(command, @"^\s*button\s*([1-4])\s*at\s*([0-9])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (m.Success)
        {
            yield return null;
            int bVal;
            int tVal;
            if (!int.TryParse(m.Groups[1].Value, out bVal))
                yield break;
            if (!int.TryParse(m.Groups[2].Value, out tVal))
                yield break;
            if (!_isAnimating[bVal - 1])
            {
                yield return "sendtochaterror You havent opened Key " + bVal + " yet!";
                yield break;
            }
            yield return "solve";
            yield return "strike";
            while ((int)BombInfo.GetTime() % 10 != tVal)
                yield return null;
            ButtonSels[bVal - 1].OnInteract();
        }
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        for (int i = 0; i < 4; i++)
        {
            if (!_isAnimating[i])
            {
                KeySels[i].OnInteract();
                yield return new WaitForSeconds(0.2f);
            }
        }
        int?[] autoSolveVals = _correctValues.Select(i => (int?)i).ToArray();
        while (_correctlyPressed.Contains(false))
        {
            while (!autoSolveVals.Contains((int)BombInfo.GetTime() % 10))
                yield return true;
            var ix = Array.IndexOf(autoSolveVals, (int)BombInfo.GetTime() % 10);
            if (!_correctlyPressed[ix])
            {
                ButtonSels[ix].OnInteract();
                autoSolveVals[ix] = null;
                yield return new WaitForSeconds(0.1f);
            }
            yield return true;
        }
        while (!_moduleSolved)
            yield return true;
    }
}
