using System;
using System.Collections.Generic;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Runtime
{
    public enum RunPhase
    {
        Boot,
        RunStart,
        BlindStart,
        HandSelect,
        HandResolve,
        BlindResult,
        Shop,
        AnteAdvance,
        BossResolve,
        RunComplete,
        RunFail
    }

    public sealed class RunState
    {
        public Guid RunId { get; set; } = Guid.NewGuid();
        public RunPhase Phase { get; set; } = RunPhase.Boot;
        public int Ante { get; set; } = 1;
        public BlindType BlindType { get; set; } = BlindType.Small;
        public int TargetScore { get; set; } = 250;
        public int CurrentScore { get; set; }
        public int PlaysLeft { get; set; } = 4;
        public int DiscardsLeft { get; set; } = 3;
        public int Money { get; set; } = 8;
        public IReadOnlyList<string> ActiveModifiers { get; set; } = Array.Empty<string>();
        public int Seed { get; set; }
    }

    public sealed class RuntimeEvent
    {
        public string EventName { get; set; } = string.Empty;
        public int SchemaVersion { get; set; } = 1;
        public object Payload { get; set; }
    }

    public sealed class BlindResolution
    {
        public bool Passed { get; set; }
        public BlindType BlindType { get; set; }
        public int Ante { get; set; }
        public int CurrentScore { get; set; }
        public int TargetScore { get; set; }
        public RunPhase NextPhase { get; set; }
    }
}
