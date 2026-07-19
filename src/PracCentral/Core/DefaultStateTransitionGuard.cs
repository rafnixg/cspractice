using PracCentral.Core.Contracts;

namespace PracCentral.Core;

public sealed class DefaultStateTransitionGuard : IStateTransitionGuard
{
    public void ValidateTransition(PracMode currentMode, PracMode nextMode)
    {
        if (nextMode == PracMode.Transition)
        {
            throw new InvalidOperationException("Transition mode is reserved for internal orchestration.");
        }

        if (currentMode == PracMode.Transition)
        {
            throw new InvalidOperationException("Cannot transition while another transition is in progress.");
        }
    }
}
