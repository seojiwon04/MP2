using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : MonoBehaviour
{
    private HashSet<string> unlockedAchievements =
        new HashSet<string>();

    [Header("Trophy Display")]
    public GameObject trophyPrefab;
    public Transform trophySpawnPoint;

    public float trophySpacing = 0.4f;

    private int trophyCount = 0;

    public void UnlockAchievement(string achievementName)
    {
        if (unlockedAchievements.Contains(achievementName))
            return;

        unlockedAchievements.Add(achievementName);

        Debug.Log(
            "ACHIEVEMENT UNLOCKED: " + achievementName
        );

        SpawnTrophy(achievementName);
    }

    private void SpawnTrophy(string achievementName)
    {
        if (trophyPrefab == null || trophySpawnPoint == null)
            return;

        Vector3 position =
            trophySpawnPoint.position +
            Vector3.forward * trophySpacing * trophyCount;

        GameObject trophy = Instantiate(
            trophyPrefab,
            position,
            trophySpawnPoint.rotation
        );

        AchievementTrophy trophyScript =
            trophy.GetComponent<AchievementTrophy>();

        if (trophyScript != null)
        {
            trophyScript.SetAchievement(
                achievementName
            );
        }

        trophyCount++;
    }

    public bool HasAchievement(string achievementName)
    {
        return unlockedAchievements.Contains(
            achievementName
        );
    }
}