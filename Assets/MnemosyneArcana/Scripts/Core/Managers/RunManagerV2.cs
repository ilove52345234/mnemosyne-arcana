using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Runtime;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class RunManagerV2
    {
        public RunState CurrentState { get; private set; } = new RunState();

        public void StartRun(int seed)
        {
            CurrentState = new RunState { Seed = seed };
        }
    }
}
