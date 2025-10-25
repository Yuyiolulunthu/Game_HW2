using UnityEngine;

public class DebugHPKeys : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Minus)) GameSession.I.TakeDamage(10);
        if (Input.GetKeyDown(KeyCode.Equals)) GameSession.I.Heal(10); // µ¥¦P©ó Shift+ =
    }
}
