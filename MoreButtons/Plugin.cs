using IPA;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace MoreButtons
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    [NoEnableDisable]
    public class Plugin
    {
        [Init]
        public void Init(IPALogger logger, Zenjector zenjector)
        {
            zenjector.UseLogger(logger);
            zenjector.Install<MoreButtonsInstaller>(Location.App);
            logger.Info("MoreButtons initialized.");
        }
    }
}
