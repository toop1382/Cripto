using VContainer.Unity;
using Cripto.Game.Services;

namespace Cripto.Game.ViewModels
{
    /// <summary>
    /// VContainer entry point that shows the welcome/tutorial overlay on game start (first run only).
    /// </summary>
    public class StartupTutorialEntryPoint : IStartable
    {
        private readonly IWelcomeTutorialService _service;

        public StartupTutorialEntryPoint(IWelcomeTutorialService service)
        {
            _service = service;
        }

        public void Start()
        {
            _service.ShowIfNeeded();
        }
    }
}
