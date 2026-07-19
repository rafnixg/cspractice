namespace PracCentral.Core.Contracts;

public interface IStateTransitionGuard
{
    void ValidateTransition(PracMode currentMode, PracMode nextMode);
}
