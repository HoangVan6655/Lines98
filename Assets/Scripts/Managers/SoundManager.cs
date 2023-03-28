using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    private void Awake()
    {
        if(instance== null)
        {
            instance = this;
        }
        PlaySound("BackgroundMusic");
    }
    public List<AudioSource> audioList;

    private void PlaySound(string audioName)
    {
        AudioSource audio = audioList.FirstOrDefault(audio => audio.name == audioName);
        if(audio==null)
        {
            Debug.Log("Please check the name again");
        }
        else
        {
            audio.Play();
        }
    }
}