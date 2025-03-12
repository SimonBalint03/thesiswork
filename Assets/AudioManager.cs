using System;
using System.Collections;
using Hexes;
using Managers;
using MyBox;
using Quests;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource timeOfDayAmbientSource;
    
    
    [Separator("Background music")][SerializeField] private AudioClip[] backgroundMusic;
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    [SerializeField] private AudioClip[] menuMusic;
    [SerializeField] private float maxMusicVolume = 1f;
    private float currentMusicVolume = 1f;
    [Separator("Sound Effects")][SerializeField] private AudioClip buttonClickSound;
    private const string SFX_VOLUME_KEY = "SFXVolume";
    [SerializeField] private float maxSFXVolume = 1f;
    private float currentSFXVolume = 1f;
    [SerializeField] private AudioClip nextStepSound;
    [SerializeField] private AudioClip cityOpenSound;
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.15f;
    [SerializeField] private AudioClip[] decorationDiscoveredSounds;
    [SerializeField] private AudioClip[] cityDiscoveredSounds;
    [SerializeField] private AudioClip[] questCompletedSounds;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip[] sellMapSounds;
    [SerializeField] private AudioClip[] buyItemSounds;
    [Separator("Ambient")][SerializeField] private AudioClip dayAmbientSound;
    private const string AMBIENT_VOLUME_KEY = "AmbientVolume";
    [SerializeField] private float maxAmbientVolume = 1f;
    private float currentAmbientVolume = 1f;
    [SerializeField] private AudioClip nightAmbientSound;
    
    private bool isFirstStepAudioPlayed = false;
    private bool isPlayingRandomMusic = false;
    
    private AudioClip lastTrack;
    
    private Coroutine footstepCoroutine;
    
    private Scene currentScene;
    private bool hasSceneLoaded = false;

    private void Awake()
    {
        //StartCoroutine(MuteSFXForSeconds(2.5f));
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }
        else
        {
            Destroy(gameObject);
        }
        
        LoadVolume();
        
    }
    
    private void Update()
    {
        if (currentScene.name == "Main Menu") { return; }
        if (Player.Instance.isMoving)
        {
            StartFootsteps();
        }
        else
        {
            StopFootsteps();
        }
        
        
    }
    
    private void OnEnable()
    {
        hasSceneLoaded = false;
        SceneManager.sceneLoaded += SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded += SceneManagerOnsceneUnloaded;
        TimeOfDay.OnNextStep += OnNextStep;
        HexTile.OnTileDiscoveredFirstTime += HexTileOnTileDiscoveredFirstTime;
        Quest.OnQuestCompleted += QuestOnQuestCompleted;
        GameManager.OnGameOver += GameManagerOnGameOver;
        Map.OnSellMapEvent += MapOnSellMapEvent;
        Inventory.Inventory.OnBuyItem += InventoryOnBuyItem;
    }


    private void OnDisable()
    {
        hasSceneLoaded = false;
        SceneManager.sceneLoaded -= SceneManagerOnsceneLoaded;
        SceneManager.sceneUnloaded -= SceneManagerOnsceneUnloaded;
        TimeOfDay.OnNextStep -= OnNextStep;
        HexTile.OnTileDiscoveredFirstTime -= HexTileOnTileDiscoveredFirstTime;
        Quest.OnQuestCompleted -= QuestOnQuestCompleted;
        GameManager.OnGameOver -= GameManagerOnGameOver;
        Map.OnSellMapEvent -= MapOnSellMapEvent;
        Inventory.Inventory.OnBuyItem -= InventoryOnBuyItem;
    }
    
    private void InventoryOnBuyItem()
    {
        PlaySFX(buyItemSounds[Random.Range(0, buyItemSounds.Length)]);
    }
    
    private void MapOnSellMapEvent(Map map)
    {
        PlaySFX(sellMapSounds[Random.Range(0, sellMapSounds.Length)]);
    }
    
    private void GameManagerOnGameOver()
    {
        PlaySFX(gameOverSound);
    }
    
    private void QuestOnQuestCompleted(Quest quest)
    {
        PlaySFX(questCompletedSounds[Random.Range(0, questCompletedSounds.Length)]);
    }
    
    private void HexTileOnTileDiscoveredFirstTime(HexTile tile)
    {
        switch (tile.Decoration)
        {
            case HexTile.TileDecoration.None:
            case HexTile.TileDecoration.Vulcan:
            case HexTile.TileDecoration.Waterfall:
            case HexTile.TileDecoration.HotSpring:
            case HexTile.TileDecoration.WildHorses:
            case HexTile.TileDecoration.WildBears:
                break;
            case HexTile.TileDecoration.Cave:
            case HexTile.TileDecoration.GiantTree:
            case HexTile.TileDecoration.Beehive:
            case HexTile.TileDecoration.RitualStones:
                PlaySFX(decorationDiscoveredSounds[Random.Range(0, decorationDiscoveredSounds.Length)]);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (tile.IsCity)
        {
            PlaySFX(cityDiscoveredSounds[Random.Range(0, cityDiscoveredSounds.Length)]);
        }
        
    }
    
    private void SceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        currentScene = arg0;
        if (arg0.name == "Main Menu")
        {
            StopMusic();
            timeOfDayAmbientSource.Stop();
            StartMenuMusic();
        }
        if (arg0.name == "Smaller Scale")
        {
            StopMusic();
            timeOfDayAmbientSource.Play();
            StartInGameRandomMusic();
        }
        
        hasSceneLoaded = true;
    }
    
    private void SceneManagerOnsceneUnloaded(Scene arg0)
    {
        hasSceneLoaded = false;
    }

    public void OnButtonClick()
    {
        Instance.PlaySFX(buttonClickSound);
    }

    public void OnNextStep()
    {
        if (isFirstStepAudioPlayed) { Instance.PlaySFX(nextStepSound); }
        else { isFirstStepAudioPlayed = true; }
        
        if (TimeOfDay.Instance.CurrentStep.TimeType is TimeOfDay.TimeType.Day or TimeOfDay.TimeType.Sunrise)
        {
            if (!timeOfDayAmbientSource.clip || timeOfDayAmbientSource.clip == nightAmbientSound)
            {
                timeOfDayAmbientSource.clip = dayAmbientSound;
                timeOfDayAmbientSource.loop = true;
                timeOfDayAmbientSource.Play();
            }
        } else if (TimeOfDay.Instance.CurrentStep.TimeType is TimeOfDay.TimeType.Night or TimeOfDay.TimeType.Moonrise)
        {
            if (!timeOfDayAmbientSource.clip || timeOfDayAmbientSource.clip == dayAmbientSound)
            {
                timeOfDayAmbientSource.clip = nightAmbientSound;
                timeOfDayAmbientSource.loop = true;
                timeOfDayAmbientSource.Play();
            }
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (!hasSceneLoaded) { return; }
        Debug.Log(clip.name + " : " + hasSceneLoaded);
        
        volume = currentSFXVolume;
        sfxSource.PlayOneShot(clip, volume);
    }
    
    public void SetMusicVolume(float volume)
    {
        currentMusicVolume = Mathf.Clamp(volume, 0f, maxMusicVolume);
        musicSource.volume = currentMusicVolume;

        // Save volume setting
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, currentMusicVolume);
        PlayerPrefs.Save();
    }

    public void SetAmbientVolume(float volume)
    {
        currentAmbientVolume = Mathf.Clamp(volume, 0f, maxAmbientVolume);
        timeOfDayAmbientSource.volume = currentAmbientVolume;
        
        PlayerPrefs.SetFloat(AMBIENT_VOLUME_KEY, currentAmbientVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        currentSFXVolume = Mathf.Clamp(volume, 0f, maxSFXVolume);
        sfxSource.volume = currentSFXVolume;
        
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, currentSFXVolume);
        PlayerPrefs.Save();
    }
    
    
    private void LoadVolume()
    {
        if (PlayerPrefs.HasKey(MUSIC_VOLUME_KEY))
        {
            currentMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY);
        }
        else
        {
            currentMusicVolume = maxMusicVolume;
        }

        if (PlayerPrefs.HasKey(SFX_VOLUME_KEY))
        {
            currentSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY);
        }
        else
        {
            currentSFXVolume = maxSFXVolume;
        }
        
        if (PlayerPrefs.HasKey(AMBIENT_VOLUME_KEY))
        {
            currentAmbientVolume = PlayerPrefs.GetFloat(AMBIENT_VOLUME_KEY);
        }
        else
        {
            currentAmbientVolume = maxAmbientVolume;
        }

        musicSource.volume = currentMusicVolume;
        sfxSource.volume = currentSFXVolume;
        timeOfDayAmbientSource.volume = currentAmbientVolume;
    }


    public float GetMusicVolume()
    {
        return currentMusicVolume;
    }

    public void PlayCityOpenSound()
    {
        PlaySFX(cityOpenSound);
    }
    
    public void StartInGameRandomMusic()
    {
        if (!isPlayingRandomMusic)
        {
            isPlayingRandomMusic = true;
            StartCoroutine(PlayRandomMusic(backgroundMusic));
        }
    }

    public void StartMenuMusic()
    {
        StartCoroutine(PlayRandomMusic(menuMusic));
    }
    
    public void StopMusic()
    {
        isPlayingRandomMusic = false;
        StopAllCoroutines();
        musicSource.Stop();
    }
    
    public void StartFootsteps()
    {
        if (footstepCoroutine == null)
        {
            footstepCoroutine = StartCoroutine(PlayFootsteps());
        }
    }

    public void StopFootsteps()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }

    private IEnumerator MuteSFXForSeconds(float seconds)
    {
        Debug.Log("Mute");
        sfxSource.mute = true;
        yield return new WaitForSeconds(seconds);

        Debug.Log("Unmute");
        sfxSource.mute = false;
        sfxSource.volume = 1f;
        yield break;
    }

    private IEnumerator PlayFootsteps()
    {
        while (true)
        {
            if (footstepSounds.Length > 0)
            {
                sfxSource.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
            }
            yield return new WaitForSeconds(footstepInterval);
        }
    }

    private IEnumerator PlayRandomMusic(AudioClip[] clips)
    {
        while (true)
        {
            if (clips.Length == 0) yield break;

            // Select a random track (avoid repeating the last one)
            AudioClip newTrack;
            do
            {
                newTrack = clips[Random.Range(0, clips.Length)];
            } while (newTrack == lastTrack);
            lastTrack = newTrack;

            // Play the new track
            musicSource.clip = newTrack;
            musicSource.Play();
            yield return StartCoroutine(FadeIn(10f));

            // Wait for the track duration minus the fade time
            float fadeTime = 20f;
            yield return new WaitForSeconds(newTrack.length - fadeTime);

            // Start fade-out before track ends
            yield return StartCoroutine(FadeOut(fadeTime/2));

            // Wait for a random interval before playing the next track
            float randomInterval = Random.Range(5f, 15f);
            yield return new WaitForSeconds(randomInterval);
        }
    }
    
    private IEnumerator FadeIn(float fadeTime)
    {
        musicSource.volume = 0f;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, currentMusicVolume, t / fadeTime);
            yield return null;
        }
        musicSource.volume = currentMusicVolume;
    }

    private IEnumerator FadeOut(float fadeTime)
    {
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.volume = 0f;
        musicSource.Stop();
    }
}