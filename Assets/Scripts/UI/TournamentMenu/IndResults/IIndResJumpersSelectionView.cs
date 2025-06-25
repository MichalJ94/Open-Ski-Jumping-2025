using System;
using System.Collections.Generic;

namespace OpenSkiJumping.UI.TournamentMenu.JumpersSelection
{
    public interface IIndResJumpersSelectionView
    {
        IEnumerable<CompetitorData> Jumpers { set; }


        event Action OnDataReload;
    }
}