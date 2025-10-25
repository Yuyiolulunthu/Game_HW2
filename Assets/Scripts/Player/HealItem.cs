using UnityEngine;

public class HealItem : MonoBehaviour
{
    [SerializeField] private int healAmount = 30;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;

    private bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (used) return; 
        if (!other.CompareTag("Player")) return;

        var php = other.GetComponent<PlayerHP>();
        if (php != null)
        {
            php.Heal(healAmount);
            used = true;

            if (pickupEffect)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            if (pickupSound)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}
