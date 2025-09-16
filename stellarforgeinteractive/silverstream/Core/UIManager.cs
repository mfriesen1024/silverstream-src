using ca.stellarforgeinteractive.silverstream.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.UI
{
    public class UIManager:MonoBehaviour
    {
        private InputActionAsset input;
        [SerializeField] Slider ProgressBar;
        [SerializeField] ButtonHelper play;
        [SerializeField] ButtonHelper resume;
        [SerializeField] ButtonHelper pauseReturn;
        [SerializeField] ButtonHelper resultsContinue;
        [SerializeField] ButtonHelper resultsQuit;
        [SerializeField] ButtonHelper upgradeContinue;

        void Start()
        {
            play.Clicked += PlayClicked;
            resume.Clicked += ResumeClicked;
            pauseReturn.Clicked += PauseReturn;
            resultsContinue.Clicked += ResultsContinue;
            resultsQuit.Clicked += ResultsQuit;
            upgradeContinue.Clicked += UpgradeContinue;
        }

        private void UpgradeContinue()
        {
            throw new System.NotImplementedException();
        }

        private void ResultsQuit()
        {
            throw new System.NotImplementedException();
        }

        private void ResultsContinue()
        {
            throw new System.NotImplementedException();
        }

        private void PauseReturn()
        {
            throw new System.NotImplementedException();
        }

        private void ResumeClicked()
        {
            throw new System.NotImplementedException();
        }

        private void PlayClicked()
        {
            throw new System.NotImplementedException();
        }
    }
}