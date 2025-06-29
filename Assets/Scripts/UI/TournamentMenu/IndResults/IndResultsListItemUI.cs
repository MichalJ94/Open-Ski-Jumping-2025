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
        public Image overlayTop30;
        public Image overlayBottom20;
        public Image overlayQualification;
        public Image overlayTrial;
        public Image overlayNoResult;
        public Image overlayPlace1;
        public Image overlayPlace2;
        public Image overlayPlace3;
    }
}
