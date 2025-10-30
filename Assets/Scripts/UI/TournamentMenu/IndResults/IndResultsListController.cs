using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.ListView;
using UnityEngine;
namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public enum ResultBackgroundStyle
    {
        NoResult,
        Top30,
        Bottom20,
        Qualification,
        Trial,
        Place1,
        Place2,
        Place3
    }

    public class IndResultsListItem
    {
        public string competitionID;
        public string hillName;
        public string name;
        public string countryCode;
        public int rank;
        public decimal value;
        public ResultBackgroundStyle backgroundStyle;
    }

    public class IndResultsListController : MonoBehaviour
    {
        [SerializeField] private IndResultsListView listView;
        [SerializeField] private FlagsData flagsData;

        private List<IndResultsListItem> results;

        public IEnumerable<IndResultsListItem> Results
        {
            get => results;
            set
            {
                results = value.ToList();
                listView.Items = results;
                listView.Refresh();
            }
        }

        public void Initialize()
        {
            listView.SelectionType = SelectionType.None;
            listView.Initialize(BindListItem);
            Debug.Log("IndResultsListController initialized");
        }

        private void BindListItem(int index, IndResultsListItemUI uiItem)
        {
            if (index < 0 || index >= results.Count)
            {
                Debug.LogWarning($"[BindListItem] Index {index} out of range. results.Count = {results.Count}");
                return;
            }

            var data = results[index];

            // 🏔️ Set hill name with compID
            uiItem.nameText.text = $" ({data.competitionID}) {data.hillName}";

            // 🥇 Rank or "-"
            uiItem.rankText.text = data.rank > 0 ? data.rank.ToString() : "-";

            // ❌ Reset text colors
            uiItem.nameText.color = Color.white;
            uiItem.rankText.color = Color.white;

            // ❌ Disable all overlays first
            uiItem.overlayTop30.gameObject.SetActive(false);
            uiItem.overlayBottom20.gameObject.SetActive(false);
            uiItem.overlayQualification.gameObject.SetActive(false);
            uiItem.overlayTrial.gameObject.SetActive(false);
            uiItem.overlayNoResult.gameObject.SetActive(false);
            uiItem.overlayPlace1.gameObject.SetActive(false);
            uiItem.overlayPlace2.gameObject.SetActive(false);
            uiItem.overlayPlace3.gameObject.SetActive(false);

            // ✅ Show overlay according to backgroundStyle
            switch (data.backgroundStyle)
            {
                case ResultBackgroundStyle.Qualification:
                    uiItem.overlayQualification.gameObject.SetActive(true);
                    if (ColorUtility.TryParseHtmlString("#666666", out Color qColor))
                    {
                        uiItem.nameText.color = qColor;
                        uiItem.rankText.color = qColor;
                    }
                    uiItem.rankText.text += " (Q)";
                    break;

                case ResultBackgroundStyle.Trial:
                    uiItem.overlayTrial.gameObject.SetActive(true);
                    uiItem.rankText.text += " (T)";
                    break;

                case ResultBackgroundStyle.Place1:
                    uiItem.overlayPlace1.gameObject.SetActive(true);
                    break;

                case ResultBackgroundStyle.Place2:
                    uiItem.overlayPlace2.gameObject.SetActive(true);
                    break;

                case ResultBackgroundStyle.Place3:
                    uiItem.overlayPlace3.gameObject.SetActive(true);
                    break;

                case ResultBackgroundStyle.Top30:
                    uiItem.overlayTop30.gameObject.SetActive(true);
                    break;

                case ResultBackgroundStyle.Bottom20:
                    uiItem.overlayBottom20.gameObject.SetActive(true);
                    if (ColorUtility.TryParseHtmlString("#d9d9d9", out Color btColor))
                    {
                        uiItem.nameText.color = btColor;
                        uiItem.rankText.color = btColor;
                    }
                    break;

                case ResultBackgroundStyle.NoResult:
                    uiItem.overlayNoResult.gameObject.SetActive(true);
                    if (ColorUtility.TryParseHtmlString("#464646", out Color nrColor))
                    {
                        uiItem.nameText.color = nrColor;
                        uiItem.rankText.color = nrColor;
                    }
                    break;
            }
        }




    }
}
