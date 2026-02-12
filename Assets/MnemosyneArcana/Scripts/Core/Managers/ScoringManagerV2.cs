using System.Collections.Generic;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class ScoringManagerV2 : IScoringService
    {
        public ServiceResult<ScoreBreakdown> EvaluateHand(IReadOnlyList<PlayedCard> cards, RunModifiers modifiers)
        {
            return ServiceResult<ScoreBreakdown>.Fail(ErrorCode.NotImplemented);
        }
    }
}
