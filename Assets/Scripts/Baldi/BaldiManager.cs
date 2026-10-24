using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class BaldiManager : Singleton<BaldiManager>
{
    [SerializeField] private List<EntityInstance> entityPref = new List<EntityInstance>(), entities = new List<EntityInstance>();
    [SerializeField] private List<EntitySpawn> entitySpawns = new List<EntitySpawn>();

    [SerializeField] private TMP_Text hud, stated;
    [SerializeField] private float playerTimeScale = 1, npcTimeScale = 1;

    public int foundNotebooks, foundExits;
    [SerializeField] private Notebook notebookPref;
    [SerializeField] private List<Notebook> notebooks = new List<Notebook>();
    [SerializeField] private Entrance entrancePref;
    [SerializeField] private List<Entrance> exits = new List<Entrance>();

    [SerializeField] private bool paused;
    IEnumerator entityLife, playGame;

    [SerializeField] private GameObject editorProperties;
    [SerializeField] private Sprite exitActivated, exitInactivated;
    public void Play()
    {
        EditorPad.Instance.Pause(true);
        editorProperties.SetActive(false);

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
        playerTimeScale = 1;
        npcTimeScale = 1;

        foundNotebooks = 0;
        foundExits = 0;
        foreach (var a in notebooks)
        {
            a.ResetState();
        }
        foreach (var e in exits)
        {
            e.spriteRenderer.color = Color.white;
            e.spriteRenderer.sprite = exitActivated;
        }
    }

    IEnumerator PlayGame()
    {
        ResetAll();
        stated.text = string.Empty;
        Stopwatch sw = Stopwatch.StartNew();

        int ce = exits.Count - 1, cn = notebooks.Count - 1;
        while ((cn > foundNotebooks) || (ce > foundExits))
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
            yield return null;
        }

        stated.color = Color.green;
        stated.text = $"Congratulations!\nYou won the game within {sw.ElapsedMilliseconds} ms!";
        yield return new WaitForSecondsRealtime(5);
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
        stated.color = Color.red;
        stated.text = $"G@M3 0V3R111";
        yield return new WaitForSecondsRealtime(5);
        ResetAll();
        QuitGame();
    }
    public void GetNotebook()
    {
        foundNotebooks++;
        int ce = exits.Count - 1, cn = notebooks.Count - 1;
        if (cn > foundNotebooks)
        {
            hud.text = $"Notebooks: {foundNotebooks}/{cn}";
        }
        else
        {
            hud.text = $"Exits: {foundExits}/{ce}";
            foreach (var e in exits)
            {
                e.spriteRenderer.color = Color.green;
            }
        }
    }
    public void GetEntrance(Entrance entrance)
    {
        entrance.spriteRenderer.color = Color.red;
        entrance.spriteRenderer.sprite = exitInactivated;
        hud.text = $"Exits: {foundExits}/{exits.Count - 1}";
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

    public void Pause()
    {
        paused = !paused;
        stated.color = Color.red;
        stated.text = paused ? $"Paused!" : string.Empty;
        if (paused)
        {
            _pts = playerTimeScale;
            _nts = npcTimeScale;
        }
        playerTimeScale = paused ? 0 : _pts;
        npcTimeScale = paused ? 0 : _nts;
    }
    float _pts, _nts;

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


    public void AddEntityToSpawn(int index, Coordinate coordinate) => entitySpawns.Add(new EntitySpawn()
    {
        index = index,
        position = coordinate,
        prev = Instantiate(entityPref[index], Coordinate.ConvertToMapCoordinate(coordinate), Quaternion.identity, transform)
    });
    public void RemoveEntityFromSpawn(int index, Coordinate coordinate)
    {
        EntitySpawn e;
        for (int i = 0; i < entitySpawns.Count; i++)
        {
            if (entitySpawns[i].index == index && entitySpawns[i].position == coordinate)
            {
                e = entitySpawns[i];
                Destroy(e.prev.gameObject);
                entitySpawns.RemoveAt(i);
            }
        }
    }
    [Serializable]
    public struct EntitySpawn
    {
        public int index;
        public Coordinate position;
        public EntityInstance prev;
    }
}
