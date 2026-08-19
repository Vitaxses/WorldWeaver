using System.Collections;
using GlobalEnums;
using TeamCherry.Localization;
using UnityEngine;

namespace WorldWeaver.Data.MonoBehaviours
{
    [System.Serializable]
    public class Pane
    {
        [SerializeField]
        public LocalisedString Label;
        [SerializeField]
        public LocalisedString Name;

        [Space]

        [SerializeField]
        public float StartDelay;
        [SerializeField]
        public float FadeTime;
        [SerializeField]
        public float DisplayTime;
    }

    [AddComponentMenu("WorldWeaver/Credits")]
    public class WeaverCredits : MonoBehaviour
    {
        [SerializeField]
        private Pane[] panes;

        [Space]

        [SerializeField]
        private TMProOld.TMP_Text labelText;

        [SerializeField]
        private TMProOld.TMP_Text nameText;

        [Space]

        [SerializeField]
        private bool dontDestroyOnLoad;

        private MeshRenderer labelTextRenderer;
        private MeshRenderer nameTextRenderer;

        void Start()
        {
            labelText.transform.SetParent(transform, false);
            nameText.transform.SetParent(transform, false);

            labelTextRenderer = labelText.GetComponent<MeshRenderer>();
            nameTextRenderer = nameText.GetComponent<MeshRenderer>();

            StartCoroutine(Begin());
        }

        private IEnumerator Begin()
        {
            if (dontDestroyOnLoad)
            {
                transform.SetParent(null);
                yield return new WaitForEndOfFrame();
                DontDestroyOnLoad(gameObject);
            }

            labelText.alpha = 0f;
            nameText.alpha = 0f;

            foreach (var pane in panes)
            {
                labelText.text = string.IsNullOrEmpty(pane.Label.Key) ? "" : pane.Label.ToString();
                nameText.text = string.IsNullOrEmpty(pane.Name.Key) ? "" : pane.Name.ToString();

                yield return new WaitForSeconds(pane.StartDelay);

                yield return StartCoroutine(FadeTexts(0f, 1f, pane.FadeTime));

                yield return new WaitForSeconds(pane.DisplayTime);

                yield return StartCoroutine(FadeTexts(1f, 0f, pane.FadeTime));
            }

            Destroy(gameObject);
        }

        private IEnumerator WaitForGame()
        {
            while (GameManager.instance.GameState != GameState.PLAYING)
            {
                if (GameManager.instance.GameState == GameState.MAIN_MENU)
                {
                    Destroy(gameObject);
                    yield break;
                }

                nameTextRenderer.enabled = labelTextRenderer.enabled = GameManager.instance.GameState == GameState.PAUSED;
                yield return null;
            }

            nameTextRenderer.enabled = labelTextRenderer.enabled = true;
        }

        private IEnumerator FadeTexts(float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                yield return StartCoroutine(WaitForGame());
                
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / duration);

                labelText.alpha = alpha;
                nameText.alpha = alpha;

                yield return null;
            }

            labelText.alpha = to;
            nameText.alpha = to;
        }
    }
}

/*panes:
  - Label:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_MOD
    Name:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_NAME_01
    StartDelay: 2
    FadeTime: 1
    DisplayTime: 2
  - Label:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: 
    Name:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_NAME_02
    StartDelay: 0.5
    FadeTime: 1
    DisplayTime: 2
  - Label:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: 
    Name:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_NAME_03
    StartDelay: 0.5
    FadeTime: 1
    DisplayTime: 2
  - Label:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: 
    Name:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_NAME_04
    StartDelay: 0.5
    FadeTime: 1
    DisplayTime: 2
  - Label:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_ART_01
    Name:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_ART_NAME_01
    StartDelay: 1.5
    FadeTime: 1
    DisplayTime: 2
  - Label:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: 
    Name:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_ART_NAME_02
    StartDelay: 0.5
    FadeTime: 1
    DisplayTime: 2
  - Label:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: 
    Name:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_ART_NAME_03
    StartDelay: 0.5
    FadeTime: 1
    DisplayTime: 2
  - Label:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: TUTORIAL_CREDIT_SCORE
    Name:
      Sheet: Mods.io.github.jeceratops.silksoul
      Key: CREDITS_SCORE_NAME_01
    StartDelay: 1.5
    FadeTime: 1
    DisplayTime: 2
  labelText: {fileID: 1430437130}
  nameText: {fileID: 698569530}
  dontDestroyOnLoad: 1*/