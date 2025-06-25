namespace OpenSkiJumping.UI.TournamentMenu.JumpersSelection
{
    public class IndResJumpersSelectionPresenter
    {
        private readonly IIndResJumpersSelectionView view;
        private readonly TournamentMenuData model;

        public IndResJumpersSelectionPresenter(IIndResJumpersSelectionView view, TournamentMenuData model)
        {
            this.model = model;
            this.view = view;

            InitEvents();
            SetInitValues();
        }

        private void PresentList()
        {
            view.Jumpers = model.Competitors;
        }

        private void ChangeJumperState(CompetitorData item, bool value)
        {
            item.registered = value;
        }

        private void InitEvents()
        {

            view.OnDataReload += SetInitValues;
        }

        private void SetInitValues()
        {
            PresentList();
        }
    }
}