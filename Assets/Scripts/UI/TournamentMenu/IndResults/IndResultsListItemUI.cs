// Example: IndResultsListItemUI.cs
using OpenSkiJumping.UI.ListView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OpenSkiJumping.UI.TournamentMenu.ResultsMenu
{
    public class IndResultsListItemUI : ListItemBehaviour
    
    {
        public TMP_Text nameText;
        public TMP_Text rankText;
        public TMP_Text valueText;
        public TMP_Text countryCodeText;
        public TMP_Text competitionID;
        public TMP_Text hillName;
        public Image countryFlagImage;
        [SerializeField] public Image background;
    }
}
