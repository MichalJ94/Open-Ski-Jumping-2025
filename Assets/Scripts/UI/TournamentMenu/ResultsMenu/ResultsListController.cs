using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.TVGraphics.SideResults;
using OpenSkiJumping.UI.ListView;
using UnityEngine;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    [Serializable]
    public class ResultsListItem
    {
        public string countryCode;
        public string name;
        public int rank;
        public decimal value;
        public decimal distance;
        public decimal previousRoundDistance;
        public decimal style;
        public decimal previousRoundStyle;
        public int firstPlaces;
    }

    public interface IResultsListController
    {
        IEnumerable<ResultsListItem> Results { set; }
    }

    public class ResultsListController : MonoBehaviour, IResultsListController
    {
        [SerializeField] private FlagsData flagsData;
        [SerializeField] private ResultsListView listView;
        [SerializeField] private EventsSelectionView eventsSelectionView;
        private List<ResultsListItem> results;


        public IEnumerable<ResultsListItem> Results
        {
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
            listView.Initialize(BindListViewItem);
        }

        private void BindListViewItem(int index, SideResultsListItem listItem)
        {
            var item = results[index];

            listItem.rankText.text = $"{item.rank}";
            listItem.nameText.text = item.name;
            listItem.countryFlagText.text = item.countryCode;
            listItem.countryFlagImage.sprite = flagsData.GetFlag(item.countryCode);
            listItem.resultText.text = $"{item.value.ToString("F1", CultureInfo.InvariantCulture)}";
            listItem.distanceText.text = $"{item.distance.ToString("F1", CultureInfo.InvariantCulture)} m";
            if(item.previousRoundDistance > 0 && item.previousRoundDistance < 1000) { 
            listItem.previousRoundDistanceText.text = $"{item.previousRoundDistance.ToString("F1", CultureInfo.InvariantCulture)} m";
            }
            else
            {
                listItem.previousRoundDistanceText.text = "";
            }

            if(item.style > 0)
            {
                listItem.styleText.text = $"{item.style.ToString("F1", CultureInfo.InvariantCulture)}";
            }
            else
            {
                listItem.styleText.text = "";
            }

            // Number of third places in Classifications view
            if (item.distance == null)
            {

                listItem.distanceText.text = $" 1 ";
            }



            if (item.previousRoundStyle > 0 && item.previousRoundStyle <= 60)
            {
                listItem.previousRoundStyleText.text = $"{item.previousRoundStyle.ToString("F1", CultureInfo.InvariantCulture)}";
            }
            else
            {
                listItem.previousRoundStyleText.text = "";
            }

            for (int i = 0; i < flagsData.data.countryList.Count; i++)
            {
                if (listItem.nameText.text.Contains(flagsData.data.countryList[i].en))
                {

                    listItem.distanceText.enabled = false;
                    listItem.previousRoundDistanceText.enabled = false;
                    listItem.styleText.enabled = false;
                    listItem.previousRoundStyleText.enabled = false;
                    return;
                }
                else
                {
                    listItem.distanceText.enabled = true;
                    listItem.previousRoundDistanceText.enabled = true;
                    listItem.styleText.enabled = true;
                    listItem.previousRoundStyleText.enabled = true;

                }

            }

            listItem.firstPlacesText.text = item.firstPlaces.ToString();

        }

               /* if (name == )
                {
                    Debug.Log("Teraz .");
                    /*              listItem.distanceText.enabled = false;
                                  listItem.previousRoundDistanceText.enabled = false;
                                  listItem.styleText.enabled = false;
                                  listItem.previousRoundStyleText.enabled = false;
                              }
                              else
                              {
                                  listItem.distanceText.enabled = true;
                                  listItem.previousRoundDistanceText.enabled = true;
                                  listItem.styleText.enabled = true;
                                  listItem.previousRoundStyleText.enabled = true;

                }*/
            
            /*listItem.distanceText.text = $"{item.lastRoundDistance.ToString("F1", CultureInfo.InvariantCulture)} m";
            listItem.previousRoundDistanceText.text = $"{item.previousRoundDistance.ToString("F1", CultureInfo.InvariantCulture)} m";
            /*if (roundNumber == 0)
            {
                listItem.previousRoundDistanceText.enabled = false;
            }
            else
            {
                listItem.previousRoundDistanceText.enabled = true;
            }*/
        }
    }
