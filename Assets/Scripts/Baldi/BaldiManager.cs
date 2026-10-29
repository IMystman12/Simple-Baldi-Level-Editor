using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class BaldiManager : Singleton<BaldiManager>
{
    [Header("Pref")]
    [SerializeField] private Notebook notebookPref;
    [SerializeField] private Entrance entrancePref;
    [SerializeField] private Player playerPref;
    [SerializeField] private NPC nPCPref;

    [Header("Spawn")]
    [SerializeField] private GameObject editorProperties;
    public List<EntityInstance> entities = new List<EntityInstance>();
    [SerializeField] private List<Notebook> notebooks = new List<Notebook>();
    [SerializeField] private List<Entrance> entrances = new List<Entrance>();

    [Header("Game")]
    IEnumerator entityLife, playGame;
    [SerializeField] private TMP_Text hud, stated;
    [SerializeField] private bool paused;
    [SerializeField] public float playerTimeScale = 1, npcTimeScale = 1;
    public int foundNotebooks, foundExits;
    [SerializeField] private Sprite exitActivated, exitInactivated;
    public Player mainPlayer;

    public void Play()
    {
        EditorPad.Instance.Pause(true);
        editorProperties.SetActive(false);

        EditorPad.Instance.ec.UpdateColliders();

        if (playGame != null)
        {
            StopCoroutine(playGame);
        }
        playGame = PlayGame();
        StartCoroutine(playGame);

        if (entityLife != null)
        {
            StopCoroutine(entityLife);
        }
        entityLife = EntityLife();
        StartCoroutine(entityLife);
    }

    void ResetAll()
    {
        stated.text = string.Empty;
        Pause(false, false);

        foundNotebooks = 0;
        foundExits = 0;
        foreach (var a in notebooks)
        {
            a.ResetState();
        }
        foreach (var a in entities)
        {
            a.UpdateFromData();
        }
        foreach (var e in entrances)
        {
            e.SetColor(Color.white);
            e.spriteRenderer.sprite = exitActivated;
            e.ResetState();
        }
    }

    IEnumerator PlayGame()
    {
        ResetAll();
        stated.text = string.Empty;
        for (int i = 0; i < entities.Count; i++)
        {
            mainPlayer = entities[i].GetComponent<Player>();
            if (mainPlayer) break;
        }
        if (mainPlayer)
        {
            Stopwatch sw = Stopwatch.StartNew();

            UpdateHudText();
            while ((notebooks.Count > foundNotebooks) || (entrances.Count > foundExits))
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Pause(!paused, true);
                }
                yield return null;
            }

            Pause(true, false);
            stated.color = Color.green;
            stated.text = $"Congratulations!\nYou won the game within {sw.ElapsedMilliseconds} ms!";
            yield return new WaitForSecondsRealtime(5);
        }
        ResetAll();
        QuitGame();
    }
    public IEnumerator GameOver()
    {
        if (playGame != null)
        {
            StopCoroutine(playGame);
        }
        if (entityLife != null)
        {
            StopCoroutine(entityLife);
        }
        Pause(true, false);
        stated.color = Color.red;
        stated.text = $"G@M3 0V3R111";
        yield return new WaitForSecondsRealtime(5);
        ResetAll();
        QuitGame();
    }
    public void GetNotebook()
    {
        foundNotebooks++;
        if (notebooks.Count >= foundNotebooks)
        {
            foreach (var e in entrances)
            {
                e.SetColor(Color.green);
            }
        }
        UpdateHudText();
    }
    public bool GetEntrance(Entrance entrance)
    {
        if (notebooks.Count <= foundNotebooks)
        {
            if ((entrances.Count - 1) > foundExits)
            {
                entrance.SetColor(Color.red);
                entrance.spriteRenderer.sprite = exitInactivated;
            }
            foundExits++;
            UpdateHudText();
            return true;
        }
        return false;
    }
    void UpdateHudText()
    {
        if (notebooks.Count > foundNotebooks)
        {
            hud.text = $"Notebooks: {foundNotebooks}/{notebooks.Count}";
        }
        else
        {
            hud.text = $"Exits: {foundExits}/{entrances.Count}";
        }
    }


    IEnumerator EntityLife()
    {
        entities.ForEach(a => a.GameStart());
        while (true)
        {
            entities.ForEach(a => a.GameUpdate());
            yield return null;
        }
    }

    public void Pause(bool val, bool text)
    {
        paused = val;
        if (text)
        {
            stated.color = Color.red;
            stated.text = paused ? $"Paused!" : string.Empty;
        }
        if (paused)
        {
            _pts = playerTimeScale;
            _nts = npcTimeScale;
            _hs = hud.text;
        }
        playerTimeScale = paused ? 0 : _pts;
        npcTimeScale = paused ? 0 : _nts;
        hud.text = paused ? string.Empty : _hs;
    }
    string _hs;
    float _pts = 1, _nts = 1;

    public void QuitGame()
    {
        if (entityLife != null)
        {
            StopCoroutine(entityLife);
        }
        EditorPad.Instance.Pause(false);
        editorProperties.SetActive(true);
        stated.text = string.Empty;
    }

    public void AddSpawn(int index, Coordinate coordinate) => LoadObject(new BalObject() { type = index, position = Coordinate.ConvertToMapCoordinate(coordinate) });

    public BalPosable LoadObject(BalObject balObject)
    {
        EditorPad.Instance.ec.balObjects.Add(balObject);
        BalPosable result;
        switch (balObject.type)
        {
            case 0:
                notebooks.Add(Instantiate(notebookPref, EditorPad.Instance.ec.transform));
                result = notebooks[notebooks.Count - 1];
                break;
            case 1:
                entrances.Add(Instantiate(entrancePref, EditorPad.Instance.ec.transform));
                result = entrances[entrances.Count - 1];
                break;
            case 2:
                entities.Add(Instantiate(playerPref, EditorPad.Instance.ec.transform));
                result = entities[entities.Count - 1];
                break;
            default:
                NPC n = Instantiate(nPCPref, EditorPad.Instance.ec.transform);
                n.Initialize((NPCType)(balObject.type - 3));
                entities.Add(n);
                result = entities[entities.Count - 1];
                break;
        }
        result.balObject = balObject;
        result.UpdateFromData();
        return result;
    }

    public void RemoveSpawn(int index, Coordinate coordinate)
    {
        Transform tf;
        switch (index)
        {
            case 0:
                for (int i = 0; i < notebooks.Count; i++)
                {
                    tf = notebooks[i].transform;
                    if (Coordinate.ConvertToGridCoordinate(tf.position) == coordinate)
                    {
                        EditorPad.Instance.ec.balObjects.Remove(notebooks[i].balObject);
                        notebooks.RemoveAt(i);
                        i--;
                        Destroy(tf.gameObject);
                    }
                }
                break;
            case 1:
                for (int i = 0; i < entrances.Count; i++)
                {
                    tf = entrances[i].transform;
                    if (Coordinate.ConvertToGridCoordinate(tf.position) == coordinate)
                    {
                        EditorPad.Instance.ec.balObjects.Remove(entrances[i].balObject);
                        entrances.RemoveAt(i);
                        i--;
                        Destroy(tf.gameObject);
                    }
                }
                break;
            case 2:
                for (int i = 0; i < entities.Count; i++)
                {
                    tf = entities[i].transform;
                    if (Coordinate.ConvertToGridCoordinate(tf.position) == coordinate && tf.GetComponent<Player>())
                    {
                        EditorPad.Instance.ec.balObjects.Remove(entities[i].balObject);
                        entities.RemoveAt(i);
                        i--;
                        Destroy(tf.gameObject);
                    }
                }
                break;
            case 3:
                for (int i = 0; i < entities.Count; i++)
                {
                    tf = entities[i].transform;
                    if (Coordinate.ConvertToGridCoordinate(tf.position) == coordinate && tf.GetComponent<NPC>())
                    {
                        EditorPad.Instance.ec.balObjects.Remove(entities[i].balObject);
                        entities.RemoveAt(i);
                        i--;
                        Destroy(tf.gameObject);
                    }
                }
                break;
        }
    }
}
