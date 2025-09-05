using Cripto.Game.Gameplay.Character;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Cripto.Game.Services;
using Cripto.Game.ViewModels;
using Cripto.Game.Views;

public class GAmeInstaller : LifetimeScope
{
    [SerializeField] private Joystick joystick;

    protected override void Configure(IContainerBuilder builder)
    {
        Application.targetFrameRate = 60;
        // Services (Singleton)
        builder.Register<IMarketService, MarketService>(Lifetime.Singleton);
        builder.Register<ITradingService, TradingService>(Lifetime.Singleton);
        builder.Register<IWelcomeTutorialService, WelcomeTutorialService>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<WelcomeTutorialView>();

        // ViewModels (Scoped) and initialize on scope start
        builder.Register<MarketViewModel>(Lifetime.Scoped);
        builder.RegisterEntryPoint<MarketViewModel>(Lifetime.Scoped);
        builder.RegisterEntryPoint<StartupTutorialEntryPoint>(Lifetime.Scoped);

        // Views in scene hierarchy will get injected
        builder.RegisterComponentInHierarchy<MarketView>();
        builder.RegisterComponentInHierarchy<TopDownCharacterController>();
        builder.RegisterInstance(joystick);
    }
}