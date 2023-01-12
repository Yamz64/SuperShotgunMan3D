using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private bool has_shotgun, already_dead;
    private bool explosive_shells, invincible, fade_direction, finished_fade, timer_go;
    private int hp, ap, shells;
    private float announce_text_fade, powerup_time, powerup_timer_max, time;
    [SerializeField]
    private float announce_text_fade_increment, knockback_resistance, damage_vignette_increment, fade_vignette_increment;
    private string announce_text;
    private Text hp_text, ap_text, announce_ui, timer_text;
    private Image shell_ui, powerup_ui, damage_vignette, fade_vignette;
    [SerializeField]
    private Sprite[] shell_ui_images;
    private Sprite powerup_sprite;
    private GameObject last_damage_dealer;
    private PlayerDamagingFieldBehavior field;
    public FaceBehavior face;

    public bool HasShotgun
    {
        get { return has_shotgun; }
        set
        {
            has_shotgun = value;
        }
    }

    public bool AlreadyDead
    {
        get { return already_dead; }
        set
        {
            already_dead = value;
        }
    }

    public bool EShells
    {
        get { return explosive_shells; }
        set
        {
            explosive_shells = value;
        }
    }

    public bool Invincible
    {
        get { return invincible; }
        set
        {
            invincible = value;
            field.active = value;
        }
    }

    public bool FadeFinished
    {
        get { return finished_fade; }
    }

    public int HP
    {
        get { return hp; }
        set
        {
            hp = value;
            if (hp > 200)
                hp = 200;
            else if (hp < -100)
                hp = -100;

            int text_value = Mathf.Clamp(hp, 0, 200);
            hp_text.text = $"HP:\n{text_value}%";
        }
    }

    public int AP
    {
        get { return ap; }
        set
        {
            ap = value;
            if (ap > 100)
                ap = 100;
            else if (ap < 0)
                ap = 0;

            ap_text.text = $"RS:\n{ap}%";
        }
    }

    public int Shells
    {
        get { return shells; }
        set {
            shells = value;
            if (shells > 2)
                shells = 2;
            else if (shells < 0)
                shells = 0;

            shell_ui.sprite = shell_ui_images[2 - shells];
        }
    }

    public float PowerupTime
    {
        get { return powerup_time; }
        set
        {
            powerup_time = value;
            powerup_timer_max = powerup_time;
        }
    }

    public string AnnounceText
    {
        get { return announce_text; }
        set
        {
            announce_text = value;
            announce_ui.text = announce_text;
            announce_text_fade = 2.0f;
        }
    }

    public Sprite PowerUpSprite
    {
        get { return powerup_sprite; }
        set
        {
            powerup_sprite = value;
            powerup_ui.sprite = powerup_sprite;
            powerup_ui.transform.GetChild(0).GetComponent<Image>().sprite = powerup_sprite;
        }
    }

    public GameObject LastDamageDealer
    {
        get { return last_damage_dealer; }
        set
        {
            last_damage_dealer = value;
        }
    }

    //function plays a generic hitsound (index 16 of the sfx table)
    void PlayHitSound()
    {
        AudioUtils.InstanceSound(16, transform.position, this, null, false, 1.0f, Random.Range(0.8f, 1.2f));
    }

    //function handles logic behind the painsound (normal painsounds are indicies 11-14 of the voice table, extreme painsounds are indicies 15-18)
    void PlayPainSound(int incoming_damage)
    {
        int index = 11;
        //extreme pain
        if(incoming_damage > 40)
        {
            index = Random.Range(15, 19);
            AudioUtils.InstanceVoice(index, transform.position, this);
        }

        //if the pain isn't extreme, roll a random chance to not make a noise
        if (Random.Range(0, 5) != 0)
            return;

        index = Random.Range(11, 15);
        AudioUtils.InstanceVoice(index, transform.position, this);
    }

    //function handles logic behind the deathsound (index 19 and 20 are normal, 21 is an extreme death)
    void PlayDeathSound()
    {
        if (already_dead) return;
        if (HP <= -50)
        {
            AudioUtils.InstanceVoice(21, transform.position, this);
            return;
        }
        int index = Random.Range(19, 21);
        AudioUtils.InstanceVoice(index, transform.position, this);
        already_dead = true;
    }

    public void TakeDamage(int amount, GameObject dealer = null, bool no_painsound = false)
    {
        PlayHitSound();
        if (invincible) return;
        int armor_reduction = 0;
        if (ap > 0)
            armor_reduction = (int)((float)amount * (1.0f / 3.0f));

        if(armor_reduction > 0)
        {
            int calc_armor_reduction = armor_reduction;
            for(int i=0; i<calc_armor_reduction; i++)
            {
                if (ap == 0)
                    armor_reduction--;
                else
                    AP--;
            }
        }

        HP = hp - amount + armor_reduction;
        if (hp > 0)
        {
            damage_vignette.color = Color.Lerp(new Color(1.0f, 0.0f, 0.0f, 0.0f), Color.red, Mathf.Clamp01(amount / 100.0f));
            int animation = Random.Range(2, 4);
            face.OneTimeAnimationDriver(animation);
            if (!no_painsound)
                PlayPainSound(amount);
        }
        else
        {
            if (!already_dead)
                last_damage_dealer = dealer;
            PlayDeathSound();
        }
    }

    public void TakeDamage(int amount, Vector3 direction, GameObject dealer = null, bool no_painsound = false)
    {
        PlayHitSound();
        if (invincible) return;
        int armor_reduction = 0;
        if (ap > 0)
            armor_reduction = (int)((float)amount * (1.0f / 3.0f));

        if (armor_reduction > 0)
        {
            int calc_armor_reduction = armor_reduction;
            for (int i = 0; i < calc_armor_reduction; i++)
            {
                if (ap == 0)
                    armor_reduction--;
                else
                    AP--;
            }
        }

        HP = hp - amount + armor_reduction;
        GetComponent<Rigidbody>().AddForce(direction.normalized * ((float)amount / knockback_resistance));
        if (hp > 0)
        {
            damage_vignette.color = Color.Lerp(new Color(1.0f, 0.0f, 0.0f, 0.0f), Color.red, Mathf.Clamp01(amount / 100.0f));
            int animation = Random.Range(2, 4);
            face.OneTimeAnimationDriver(animation);
            if (!no_painsound)
                PlayPainSound(amount);
        }
        else
        {
            if(!already_dead)
                last_damage_dealer = dealer;
            PlayDeathSound();
        }
    }

    void UpdateAnnounceText()
    {
        announce_ui.color = new Color(announce_ui.color.r, announce_ui.color.g, announce_ui.color.b, Mathf.Clamp01(announce_text_fade));
        if (announce_text_fade > 0.0f)
            announce_text_fade -= Time.deltaTime * announce_text_fade_increment;
        else
            announce_text_fade = 0.0f;
    }

    void UpdatePowerupTimer()
    {
        powerup_ui.transform.GetChild(0).GetComponent<Image>().fillAmount = powerup_time / powerup_timer_max;
        if (powerup_time > 0.0f)
        {
            powerup_ui.color = new Color(powerup_ui.color.r, powerup_ui.color.g, powerup_ui.color.b, .760784f);
            powerup_ui.transform.GetChild(0).GetComponent<Image>().color = Color.white;
            powerup_time -= Time.deltaTime;
        }
        else
        {
            powerup_ui.color = new Color(powerup_ui.color.r, powerup_ui.color.g, powerup_ui.color.b, 0.0f);
            powerup_ui.transform.GetChild(0).GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            powerup_time = 0.0f;
            EShells = false;
            Invincible = false;
        }
    }

    void UpdateDamageVignette()
    {
        if (damage_vignette.color.a == 0.0f) return;
        float progress = damage_vignette.color.a;
        if (progress > 0.0f)
            progress -= Time.deltaTime * damage_vignette_increment;
        else
            progress = 0.0f;
        damage_vignette.color = new Color(1.0f, 0.0f, 0.0f, progress);
    }

    void UpdateTimerText()
    {
        int time_hours = (int)(time / 3600.0f);
        int time_minutes = (int)((time - time_hours * 3600.0f) / 60.0f);
        int time_seconds = (int)(time % 60.0f);
        timer_text.text = $"{time_hours.ToString("00")}:{time_minutes.ToString("00")}:{time_seconds.ToString("00")}";
    }

    public void ToggleFade() { fade_direction = !fade_direction; }
    
    void Fade()
    {
        //fade in
        if (!fade_direction)
        {
            if (fade_vignette.color.a == 0.0f)
            {
                finished_fade = true;
                return;
            }
            float progress = fade_vignette.color.a;
            if (progress > 0.0f)
                progress -= Time.deltaTime * fade_vignette_increment;
            else
                progress = 0.0f;
            fade_vignette.color = new Color(0.0f, 0.0f, 0.0f, progress);
            finished_fade = false;
        }
        //fade out
        else
        {
            if (fade_vignette.color.a == 1.0f)
            {
                finished_fade = true;
                return;
            }
            float progress = fade_vignette.color.a;
            if (progress < 1.0f)
                progress += Time.deltaTime * fade_vignette_increment;
            else
                progress = 1.0f;
            fade_vignette.color = new Color(0.0f, 0.0f, 0.0f, progress);
            finished_fade = false;
        }
    }

    public void ResetTime()
    {
        time = 0;
        PlayerPrefs.SetFloat("Time", 0);
    }

    public void StartTime()
    {
        timer_go = true;
    }

    public void StopTime(bool clock_time = false)
    {
        timer_go = false;
        if (clock_time)
            PlayerPrefs.SetFloat("Time", time);
    }

    private void Awake()
    {
        fade_direction = false;
        hp_text = transform.GetChild(1).GetChild(2).GetComponent<Text>();
        ap_text = transform.GetChild(1).GetChild(3).GetComponent<Text>();
        announce_ui = transform.GetChild(1).GetChild(6).GetComponent<Text>();
        shell_ui = transform.GetChild(1).GetChild(5).GetChild(0).GetComponent<Image>();
        powerup_ui = transform.GetChild(1).GetChild(7).GetComponent<Image>();
        damage_vignette = transform.GetChild(1).GetChild(8).GetComponent<Image>();
        field = transform.GetChild(3).GetComponent<PlayerDamagingFieldBehavior>();
        face = transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<FaceBehavior>();
        timer_text = transform.GetChild(1).GetChild(10).GetComponent<Text>();
        fade_vignette = transform.GetChild(1).GetChild(11).GetComponent<Image>();

        HP = 100;
        AP = 0;
        Shells = 2;

        announce_ui.color = new Color(announce_ui.color.r, announce_ui.color.g, announce_ui.color.b, 0.0f);
        damage_vignette.color = new Color(1.0f, 0.0f, 0.0f, 0.0f);

        if (!has_shotgun)
            AnnounceText = "Punch their lights out with 'F'!";

        ResetTime();
        StartTime();
    }

    private void Update()
    {
        UpdateAnnounceText();
        UpdatePowerupTimer();
        UpdateDamageVignette();
        Fade();
        UpdateTimerText();

        if (timer_go)
            time += Time.deltaTime;
    }
}
