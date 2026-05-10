using UnityEngine;

public class AudioManager_UI : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    public void PlayClick()
    {
        if (clickSound != null) audioSource.PlayOneShot(clickSound);
    }
}
