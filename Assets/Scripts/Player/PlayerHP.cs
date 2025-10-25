using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public void TakeDamage(int dmg) => GameSession.I?.TakeDamage(dmg);
    public void Heal(int val) => GameSession.I?.Heal(val);
}
