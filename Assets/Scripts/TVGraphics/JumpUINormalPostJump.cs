using System.Globalization;
using DG.Tweening;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace OpenSkiJumping.TVGraphics
{
    public class JumpUINormalPostJump : PostJumpUIManager
    {
        [FormerlySerializedAs("bib")] [SerializeField] protected TMP_Text bibText;
        [SerializeField] protected TMP_Text jumperName;
        [FormerlySerializedAs("rank")] [SerializeField] protected TMP_Text rankText;
        [SerializeField] protected TMP_Text total;
        [SerializeField] protected TMP_Text[] meters;
        [SerializeField] protected CountryInfo countryInfo;
        [SerializeField] protected RectTransform judgesMarksTransform;
        [SerializeField] protected JudgesUIData judgesUIData;

        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected CanvasGroup canvasGroup;

        private void OnEnable()
        {
            InstantHide();
        }


        private void SetCountry()
        {
            var id = resultsManager.Value.GetCurrentJumperId();
            var competitor = competitors.competitors[id];
            countryInfo.FlagImage.sprite = flagsData.GetFlag(competitor.countryCode);
            countryInfo.CountryName.text = competitor.countryCode;
        }

        public override void Show()
        {
            canvasGroup.alpha = 1;
            SetCountry();

            rectTransform.localScale = new Vector3(0, 1, 1);
            judgesMarksTransform.localScale = new Vector3(1, 0, 1);
            DOTween.Sequence().Append(rectTransform.DOScaleX(1, 0.5f)).Append(judgesMarksTransform.DOScaleY(1, 0.5f));

            var jumpsCount = resultsManager.Value.RoundIndex + 1;
            var competitorId = resultsManager.Value.GetCurrentCompetitorLocalId();
            var bib = resultsManager.Value.Results[competitorId].Bibs[resultsManager.Value.RoundIndex];
            var rank = resultsManager.Value.CompetitorRank(competitorId);
            var competitor = GetCompetitorById(competitorId, resultsManager.Value.SubroundIndex);
            var jumpResults = resultsManager.Value.GetResultById(competitorId, resultsManager.Value.SubroundIndex);

            jumperName.text = TvGraphicsUtils.JumperNameText(competitor);
            bibText.text = bib.ToString();
            rankText.text = rank.ToString();

            var xx = jumpsCount - meters.Length;

            var jump = jumpResults.results[resultsManager.Value.RoundIndex];
            total.text = TvGraphicsUtils.PointsText(resultsManager.Value.Results[competitorId].TotalPoints);
            wind.SetValues(jump.windPoints);
            gate.SetValues(jump.gatePoints);

            for (var i = 0; i < judgesMarks.Length; i++)
            {
                judgesMarks[i].SetValues(jump.judgesMarks[i], judgesUIData.countries[i], flagsData.GetFlag(judgesUIData.countries[i]), jump.judgesMask[i]);
            }

            foreach (var item in meters)
            {
                item.text = xx < 0 ? "" : TvGraphicsUtils.DistanceText(jumpResults.results[xx].distance);
                xx++;
            }
            //Problems occur with jumper from the last qualified position in the first round
        }

        public override void Hide()
        {
            canvasGroup.DOFade(0, 0.5f);
        }

        public override void InstantHide()
        {
            canvasGroup.alpha = 0;
        }


    }
}
