using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenSkiJumping.Competition.Persistent;
using OpenSkiJumping.Data;
using OpenSkiJumping.ScriptableObjects;
using OpenSkiJumping.UI.CalendarEditor.Competitors;
using OpenSkiJumping.UI.CalendarEditor;
using OpenSkiJumping.UI.JumpersMenu;
using OpenSkiJumping.UI.ListView;
using UnityEngine.UI;
using System.Linq;
using System;
using UnityEditor.PackageManager;

namespace OpenSkiJumping.UI.TournamentMenu.IndResultsMenu
{
    public class JumpersSelectionView : MonoBehaviour, IJumpersSelectionView
    {
        // Start is called before the first frame update
        [SerializeField] private Toggle allElementsToggle;

        [SerializeField] private FlagsData flagsData;
        [SerializeField] private IconsData iconsData;
        private bool initialized;

        private List<Competitor> jumpers;
        [SerializeField] private CompetitorsRuntime jumpersRuntime;

        [SerializeField] private JumpersListView listView;
        private CalendarEditorJumpersPresenter presenter;

        private HashSet<Competitor> selectedJumpers = new HashSet<Competitor>();
        [SerializeField] private ToggleGroupExtension toggleGroup;


        public Competitor selectedJumper
        {
            get => listView.SelectedIndex < 0 ? null : jumpers[listView.SelectedIndex];
            set => SelectJumper(value);
        }

        public IEnumerable<Competitor> SelectedJumpers
        {
            get => selectedJumpers.ToList();
            set => selectedJumpers = new HashSet<Competitor>(value);
        }

        public IEnumerable<Competitor> Jumpers
        {
            set
            {
                jumpers = value.ToList();
                listView.Items = jumpers;
                listView.Refresh();
            }
        }

        private void SelectJumper(Competitor item)
        {
            listView.SelectedIndex =
                item == null ? listView.SelectedIndex : jumpers.IndexOf(item);

            listView.ClampSelectedIndex();
            listView.ScrollToIndex(listView.SelectedIndex);
            listView.RefreshShownValue();
            //  eventResultsHeader.UpdateAccordingToSelectedEvent(item);
        }



        public event Action OnDataSave;
        public event Action OnDataReload;
        public event Action OnSelectionChanged;
    }
}
