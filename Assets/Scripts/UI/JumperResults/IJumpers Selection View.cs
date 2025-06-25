using OpenSkiJumping.UI.TournamentMenu.ResultsMenu;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenSkiJumping.Competition.Persistent;

namespace OpenSkiJumping.UI.TournamentMenu.IndResultsMenu
{


    public interface IJumpersSelectionView
    {
        IEnumerable<Competitor> SelectedJumpers { get; set; }
        IEnumerable<Competitor> Jumpers { set; }

        event Action OnDataSave;
        event Action OnDataReload;

        event Action OnSelectionChanged;
    }
}
