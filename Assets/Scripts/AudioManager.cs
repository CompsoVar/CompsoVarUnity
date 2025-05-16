using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	[Header("Audio Sources")]
	public AudioSource musicSource;
	public AudioSource ambianceSource;
	public AudioSource interfaceSource;
	public AudioSource stepSource;
	public AudioSource soundEffectSource;

	[Header("Audio Settings")]
	[Range(0f, 1f)] public float musicVolume = 0.5f;
	[Range(0f, 1f)] public float ambianceVolume = 0.5f;
	[Range(0f, 1f)] public float interfaceVolume = 0.5f;
	[Range(0f, 1f)] public float stepVolume = 0.5f;
	[Range(0f, 1f)] public float soundEffectVolume = 0.5f;


	private void Awake()
	{
		// Singleton Pattern
		if (Instance == null)
		{
			Instance = this;
			//DontDestroyOnLoad(gameObject); // Garde l'AudioManager entre les scènes
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		// Initialise les volumes
		InitializeVolumes();
	}

	private void InitializeVolumes()
	{
		if (musicSource) musicSource.volume = musicVolume;
		if (ambianceSource) ambianceSource.volume = ambianceVolume;
		if (interfaceSource) interfaceSource.volume = interfaceVolume;
		if (stepSource) stepSource.volume = stepVolume;
		if (soundEffectSource) soundEffectSource.volume = soundEffectVolume;
	}

	#region Music

	public void PlayMusicLooped(AudioClip clip)
	{
		PlayMusic(clip, true);
	}
	public void PlayMusic(AudioClip clip, bool loop = true)
	{
		if (musicSource && clip)
		{
			musicSource.clip = clip;
			musicSource.loop = loop;
			musicSource.Play();
		}
	}

	public void StopMusic()
	{
		if (musicSource) musicSource.Stop();
	}


	#endregion

	#region Ambiance
	public void PlayAmbiance(AudioClip clip, bool loop = true)
	{
		if (ambianceSource && clip)
		{
			ambianceSource.clip = clip;
			ambianceSource.loop = loop;
			ambianceSource.Play();
		}
	}

	public void StopAmbiance()
	{
		if (ambianceSource) ambianceSource.Stop();
	}
	#endregion

	#region Interface
	public void PlayInterfaceSound(AudioClip clip)
	{
		if (interfaceSource && clip)
		{
			interfaceSource.pitch = 1;
			interfaceSource.PlayOneShot(clip, interfaceVolume);
		}
	}
	#endregion

	#region Dialogue
	public void PlayDialogue(AudioClip clip)
	{
		if (stepSource && clip)
		{
			stepSource.PlayOneShot(clip, stepVolume);
		}
	}
	#endregion


	#region Sound Effects

	public void PlaySoundEffect(AudioClip clip)
	{
		if (soundEffectSource && clip)
		{
			soundEffectSource.PlayOneShot(clip, soundEffectVolume);
		}
	}

	public void PlayOnlyOneSoundStepEffect(AudioClip clip, float pitch = 1)
    {
        if (!stepSource.isPlaying)
        {
			stepSource.pitch = pitch;
			stepSource.clip = clip;
			stepSource.Play();
        }
    }


	public void PlayStepsSoundEffect(AudioClip clip)
	{
		soundEffectSource.clip = clip;
		soundEffectSource.PlayOneShot(clip, soundEffectVolume / Random.Range(5, 10));
	}

	#endregion
}
