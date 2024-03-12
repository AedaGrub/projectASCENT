using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class spawnManager : MonoBehaviour
{
    public static spawnManager instance;

    [Header("DOORS")]
    [SerializeField] GameObject decorDoor;
    [SerializeField] GameObject exitDoor;
    [SerializeField] Sprite normalIcon;
    [SerializeField] Sprite bossIcon;
    private SpriteRenderer doorIcon;

    [Header("PLAYER DOOR")]
    [SerializeField] Vector3 playerPos;
    [SerializeField] Vector2 playerSize;

    [Header("ENEMIES")]
    [SerializeField] GameObject[] enemies;
    [SerializeField] Vector3[] spawnPos;
    [SerializeField] Vector2 spawnSize;
    [SerializeField] int lastWave;

    [Header("PER WAVE")]
    [SerializeField] int waveCount;
    [SerializeField] string[] enemyPosWave;
    public List<GameObject> enemyList;

    [Header("WAVE SETTINGS")]
    [SerializeField] float waitTime;
    [SerializeField] bool stopSpawning;
    [SerializeField] bool isSpawning;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnPlayerDoor());
        StartCoroutine(PhaseManagement());
        audioManager.instance.Play("Combat");
    }

    private IEnumerator SpawnPlayerDoor()
    {
        //SPAWN LIFT
        GameObject pDoor = objectPoolManager.SpawnObject(decorDoor, new Vector3(playerPos.x, playerPos.y, 1f),
            Quaternion.identity, objectPoolManager.PoolType.GameObject);

        doorIcon = pDoor.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        if (gameManager.instance.currentRoom < 3)
        {
            doorIcon.sprite = normalIcon;
        }
        else
        {
            doorIcon.sprite = bossIcon;
        }

        yield return new WaitForSeconds(1f);

        //CLOSE LIFT
        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float scaleChange = Mathf.Lerp(1f, 0f, (elapsedTime / 0.1f));

            pDoor.transform.localScale = new Vector2(scaleChange, 1f);
            yield return null;
        }
        doorIcon = null;
        objectPoolManager.ReturnObjectToPool(pDoor);

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
        EndCombat();
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
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private IEnumerator SpawnEnemy(int enemyType, int enemyPos)
    {
        isSpawning = true;

        //OPEN LIFT
        GameObject eDoor = objectPoolManager.SpawnObject(decorDoor, new Vector3(spawnPos[enemyPos].x, spawnPos[enemyPos].y, 1f), 
            Quaternion.identity, objectPoolManager.PoolType.GameObject);

        doorIcon = eDoor.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        doorIcon.sprite = null;

        float elapsedTime = 0f;
        while (elapsedTime < 0.1f)
        {
            elapsedTime += Time.deltaTime;
            float scaleChange = Mathf.Lerp(0f, 1f, (elapsedTime / 0.1f));

            eDoor.transform.localScale = new Vector2(scaleChange, 1f);
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
            float scaleChange = Mathf.Lerp(1f, 0f, (elapsedTime / 0.1f));

            eDoor.transform.localScale = new Vector2(scaleChange, 1f);
            yield return null;
        }
        objectPoolManager.ReturnObjectToPool(eDoor);

        isSpawning = false;
    }

    private void EndCombat()
    {
        boonsSelectManager.instance.StartOptionSelection();
    }

    public  void EndLevel()
    {
        exitDoor.SetActive(true);
        doorIcon = exitDoor.transform.GetChild(0).GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        if (gameManager.instance.currentRoom < 2)
        {
            doorIcon.sprite = normalIcon;
        }
        else
        {
            doorIcon.sprite = bossIcon;
        }
    }

    private bool EnemyIsAlive()
    {
        enemyList = enemyList.Where(e => e != null).ToList();
        return enemyList.Count > 0 || isSpawning;
    }

    #region EDITOR METHODS
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(playerPos, playerSize);

        Gizmos.color = Color.red;
        for (int i = 0; i < spawnPos.Length; i++)
        {
            Gizmos.DrawWireCube(spawnPos[i], spawnSize);
        }
    }
    #endregion
}
