using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSFX : MonoBehaviour
{
    [SerializeField] AudioClip[] clips;
    [Range(1, 20)]
    [SerializeField] float delay;
    [SerializeField] float startDelay = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(loop());
    }

    IEnumerator loop()
    {
        yield return new WaitForSeconds(startDelay);
        while (true)
        {
            yield return new WaitForSeconds(delay);
            AudioManager.Instance.PlaySoundEffect(clips[Random.Range(0, clips.Length)]);
        }

    }
}
