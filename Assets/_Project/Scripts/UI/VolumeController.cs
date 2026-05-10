using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
   public Slider volumeSlider;

    void Start()
    {
        if (volumeSlider == null) {
            volumeSlider = GetComponent<Slider>();
        }

        volumeSlider.value = 0.5f;
 
        AudioListener.volume = volumeSlider.value;

        volumeSlider.onValueChanged.AddListener(ChangeVolume);
    }

    public void ChangeVolume(float value){
        AudioListener.volume = value;
        if (value <= 0.01f) {
            AudioListener.volume = 0;
        }
    }
}
