using UnityEngine;
using TMPro;

public class AchievementTrophy : MonoBehaviour
{
    public TMP_Text achievementText;

    public void SetAchievement(string achievementName)
    {
        achievementText.text = achievementName;
    }
}