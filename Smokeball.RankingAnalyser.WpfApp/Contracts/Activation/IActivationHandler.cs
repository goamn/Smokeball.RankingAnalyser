namespace Smokeball.RankingAnalyser.WpfApp.Contracts.Activation;

public interface IActivationHandler
{
    bool CanHandle();

    Task HandleAsync();
}
