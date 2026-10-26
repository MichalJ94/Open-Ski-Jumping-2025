using OpenSkiJumping.Competition;
using UnityEngine;
using EventType = OpenSkiJumping.Competition.EventType;

namespace OpenSkiJumping.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Data/IconsData")]
    public class IconsData : ScriptableObject
    {
        [SerializeField] private Sprite[] genderIcons;
        [SerializeField] private Sprite[] classificationTypeIcons;
        [SerializeField] private Sprite[] eventTypeIcons;
        [SerializeField] private Sprite[] bibSprites;
        

        public Sprite GetGenderIcon(Gender arg)
        {
            return genderIcons[(int) arg];
        }

        public Sprite GetClassificationTypeIcon(ClassificationType arg)
        {
            return classificationTypeIcons[(int) arg];
        }

        /// <summary>
        /// Returns the icon for the event type. Falls back to the Team icon when no dedicated icon (e.g. Duos) is assigned.
        /// </summary>
        public Sprite GetEventTypeIcon(EventType arg)
        {
            var index = (int) arg;
            if (index < eventTypeIcons.Length && eventTypeIcons[index] != null) return eventTypeIcons[index];
            return eventTypeIcons[Mathf.Min((int) EventType.Team, eventTypeIcons.Length - 1)];
        }

        public Sprite GetBibIcon(int arg)
        {
            return bibSprites[arg];
        }
    }
}