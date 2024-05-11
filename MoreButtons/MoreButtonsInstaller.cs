using Zenject;

namespace MoreButtons
{
    internal class MoreButtonsInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<MoreButtonsController>().AsSingle();
        }
    }
}