using OpenSkiJumping.UI.ListView;
using TMPro;
using UnityEngine.UI;

namespace OpenSkiJumping.TVGraphics.SideResults
{
    public class RoundResultsListItem : ListItemBehaviour
    {
        public TMP_Text rankText;
        public TMP_Text nameText;
        public TMP_Text countryFlagText;
        public Image countryFlagImage;
        public TMP_Text resultText;
        public TMP_Text distanceText;
        public TMP_Text previousRoundDistanceText;
        public TMP_Text styleText;
        public TMP_Text previousRoundStyleText;
        public TMP_Text gateText;
        public TMP_Text previousRoundGateText;
        public TMP_Text windText;
        public TMP_Text previousRoundWindText;
        public TMP_Text rankChange;
    }
}
