using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class LearningManagerV2 : ILearningService
    {
        public ServiceResult<LearningResult> ApplyAnswer(string wordId, AnswerResult answer, RunContext runContext)
        {
            return ServiceResult<LearningResult>.Fail(ErrorCode.NotImplemented);
        }
    }
}
