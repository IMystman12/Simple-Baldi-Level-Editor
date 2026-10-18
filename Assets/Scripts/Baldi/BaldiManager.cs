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
    [SerializeField] private int foundNotebook, foundExits;
    [SerializeField] private List<IconInstance> notebooks = new List<IconInstance>(), exits = new List<IconInstance>();

    [SerializeField] private bool paused;
    IEnumerator entityLife, playGame;
    public void Play()
    {
        EditorPad.Instance.Pause(true);

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
    IEnumerator PlayGame()
    {
        stated.text = string.Empty;
        Stopwatch sw = Stopwatch.StartNew();
        int ce = exits.Count - 1;
        while ((notebooks.Count > foundNotebook) || (ce > foundExits))
        {
            if (!Application.isFocused || Input.GetKeyDown(KeyCode.Escape))
            {
                Pause();
            }
            yield return null;
        }
        stated.color = Color.green;
        stated.text = $"Congratulations!\nYou won the game within {sw.ElapsedMilliseconds} ms!";
        yield return new WaitForSecondsRealtime(5);
        QuitGame();
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
    }
    public void QuitGame()
    {
        if (entityLife != null)
        {
            StopCoroutine(entityLife);
        }
        EditorPad.Instance.Pause(false);
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
