using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : Singleton<MusicManager>
{

    [SerializeField] AudioSource musicSource;

    public void ToggleMusic(bool b = false)
    {
        if (!b) musicSource.Stop();
        else musicSource.Play();
    }
}
