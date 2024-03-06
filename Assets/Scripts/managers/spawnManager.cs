using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class spawnManager : MonoBehaviour
{
    [Header("INITIAL")]
    [SerializeField] GameObject[] enemies;
    [SerializeField] Vector3[] spawnPos;
    [SerializeField] Vector2 spawnSize;
    [SerializeField] GameObject enemyDoor;
    [SerializeField] GameObject exitDoor;
    [SerializeField] int lastWave;

    [Header("PER WAVE")]
    [SerializeField] int waveCount;
    [SerializeField] string[] enemyPosWave;
    public List<GameObject> enemyList;

    [Header("WAVE SETTINGS")]
    [SerializeField] float waitTime;
    [SerializeField] bool stopSpawning;
    [SerializeField] bool isSpawning;

    private void Start()
    {
        StartCoroutine(PhaseManagement());
    }

    private IEnumerator PhaseManagement()
    {
        yield return new WaitForSeconds(waitTime);
        while (waveCount <= lastWave)
        {
            SpawnWave();
            yield return new WaitWhile(EnemyIsAlive);
            yield return new WaitForSeconds(waitTime);
        }
        EndLevel();
    }

    private void SpawnWave()
    {
        waveCount++;
        StartCoroutine(CheckEnemy());
    }

    private IEnumerator CheckEnemy()
    {
        for (int i = 0; i < enemyPosWave.Length; i++)
        {
            string[] key = enemyPosWave[i].Split(",");
            int enemyType = int.Parse(key[0]);
            int enemyPos = int.Parse(key[1]);
            int enemyWave = int.Parse(key[2]);
            if (enemyWave == waveCount)
            {
                StartCoroutine(SpawnEnemy(enemyType, enemyPos));
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator SpawnEnemy(int enemyType, int enemyPos)
    {
        isSpawning = true;

        //OPEN LIFT
        GameObject eDoor = objectPoolManager.SpawnObject(enemyDoor, new Vector3(spawnPos[enemyPos].x, spawnPos[enemyPos].y, 1f), 
            Quaternion.identity, objectPoolManager.PoolType.GameObject);

        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float scaleChange = Mathf.Lerp(0f, 2f, (elapsedTime / 0.1f));

            eDoor.transform.localScale = new Vector2(scaleChange, 3f);
            yield return null;
        }

        //SPAWN ENEMY
        GameObject obj = Instantiate(enemies[enemyType], spawnPos[enemyPos], Quaternion.identity);
        enemyList.Add(obj);
        yield return new WaitForSeconds(1f);

        //CLOSE LIFT
        elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float scaleChange = Mathf.Lerp(2f, 0f, (elapsedTime / 0.1f));

            eDoor.transform.localScale = new Vector2(scaleChange, 3f);
            yield return null;
        }
        objectPoolManager.ReturnObjectToPool(eDoor);

        isSpawning = false;
    }

    private void EndLevel()
    {
        exitDoor.SetActive(true);
    }

    private bool EnemyIsAlive()
    {
        enemyList = enemyList.Where(e => e != null).ToList();
        return enemyList.Count > 0 || isSpawning;
    }

    #region EDITOR METHODS
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i < spawnPos.Length; i++)
        {
            Gizmos.DrawWireCube(spawnPos[i], spawnSize);
        }
    }
    #endregion
}
