using OpenSkiJumping.UI.ListView;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OpenSkiJumping.UI.TournamentMenu
{
    public class RandomEventListItem : ListItemBehaviour
    {
        [Header("UI References")]
        public TMP_Text eventText;
        public Image flagImage;
        public Image iconImage;

        [Header("Icons")]
        public Sprite upArrow;
        public Sprite downArrow;
        public Sprite injuryCross;
    }
}
