using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace SlimUI.ModernMenu{
	public class OptionsMenuNew : MonoBehaviour
	{

		public enum Platform { Desktop, Mobile };
		public Platform platform;
		// toggle buttons
		[Header("MOBILE SETTINGS")]
		public GameObject mobileSFXtext;
		public GameObject mobileMusictext;
		public GameObject mobileShadowofftextLINE;
		public GameObject mobileShadowlowtextLINE;
		public GameObject mobileShadowhightextLINE;

		[Header("VIDEO SETTINGS")]
		// sliders
		public GameObject musicSlider;
		public GameObject soundSlider;


		public void Start()
		{

			// check slider values
			musicSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVolume");
			soundSlider.GetComponent<Slider>().value = PlayerPrefs.GetFloat("SoundVolume");

		}

		public void SoundSlider()
		{
			PlayerPrefs.SetFloat("SoundVolume", soundSlider.GetComponent<Slider>().value);
		}

		public void MusicSlider()
		{
			//PlayerPrefs.SetFloat("MusicVolume", sliderValue);
			PlayerPrefs.SetFloat("MusicVolume", musicSlider.GetComponent<Slider>().value);
		}

		// the playerprefs variable that is checked to enable mobile sfx while in game
		public void MobileSFXMute()
		{
			if (PlayerPrefs.GetInt("Mobile_MuteSfx") == 0)
			{
				PlayerPrefs.SetInt("Mobile_MuteSfx", 1);
				mobileSFXtext.GetComponent<TMP_Text>().text = "on";
			}
			else if (PlayerPrefs.GetInt("Mobile_MuteSfx") == 1)
			{
				PlayerPrefs.SetInt("Mobile_MuteSfx", 0);
				mobileSFXtext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void MobileMusicMute()
		{
			if (PlayerPrefs.GetInt("Mobile_MuteMusic") == 0)
			{
				PlayerPrefs.SetInt("Mobile_MuteMusic", 1);
				mobileMusictext.GetComponent<TMP_Text>().text = "on";
			}
			else if (PlayerPrefs.GetInt("Mobile_MuteMusic") == 1)
			{
				PlayerPrefs.SetInt("Mobile_MuteMusic", 0);
				mobileMusictext.GetComponent<TMP_Text>().text = "off";
			}
		}

		public void MobileShadowsOff()
		{
			PlayerPrefs.SetInt("MobileShadows", 0);
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0;
			mobileShadowofftextLINE.gameObject.SetActive(true);
			mobileShadowlowtextLINE.gameObject.SetActive(false);
			mobileShadowhightextLINE.gameObject.SetActive(false);
		}

		public void MobileShadowsLow()
		{
			PlayerPrefs.SetInt("MobileShadows", 1);
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 75;
			mobileShadowofftextLINE.gameObject.SetActive(false);
			mobileShadowlowtextLINE.gameObject.SetActive(true);
			mobileShadowhightextLINE.gameObject.SetActive(false);
		}

		public void MobileShadowsHigh()
		{
			PlayerPrefs.SetInt("MobileShadows", 2);
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 500;
			mobileShadowofftextLINE.gameObject.SetActive(false);
			mobileShadowlowtextLINE.gameObject.SetActive(false);
			mobileShadowhightextLINE.gameObject.SetActive(true);
		}
	}
}