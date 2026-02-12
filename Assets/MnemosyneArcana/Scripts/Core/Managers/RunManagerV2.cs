using System.Collections.Generic;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Runtime;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class RunManagerV2
    {
        private static readonly IReadOnlyDictionary<int, (int Small, int Big, int Boss)> BlindTargets =
            new Dictionary<int, (int Small, int Big, int Boss)>
            {
                { 1, (100, 150, 250) },
                { 2, (300, 500, 800) },
                { 3, (800, 1200, 2000) },
                { 4, (2500, 4000, 6000) },
                { 5, (6000, 9000, 15000) },
                { 6, (15000, 22000, 35000) },
                { 7, (30000, 45000, 65000) },
                { 8, (50000, 75000, 100000) }
            };

        public RunState CurrentState { get; private set; } = new RunState();

        public void StartRun(int seed)
        {
            CurrentState = new RunState
            {
                Seed = seed,
                Ante = 1,
                BlindType = BlindType.Small,
                PlaysLeft = 4,
                DiscardsLeft = 3,
                Money = 8,
                CurrentScore = 0,
                Phase = RunPhase.BlindStart
            };
            CurrentState.TargetScore = GetBlindTarget(CurrentState.Ante, CurrentState.BlindType);
            CurrentState.Phase = RunPhase.HandSelect;
        }

        public ServiceResult<RunState> SubmitHandScore(int handScore)
        {
            if (handScore < 0)
            {
                return ServiceResult<RunState>.Fail(ErrorCode.InvalidInput);
            }

            if (CurrentState.Phase != RunPhase.HandSelect || CurrentState.PlaysLeft <= 0)
            {
                return ServiceResult<RunState>.Fail(ErrorCode.StateConflict);
            }

            CurrentState.Phase = RunPhase.HandResolve;
            CurrentState.CurrentScore += handScore;
            CurrentState.PlaysLeft -= 1;

            if (CurrentState.CurrentScore >= CurrentState.TargetScore || CurrentState.PlaysLeft == 0)
            {
                CurrentState.Phase = RunPhase.BlindResult;
            }
            else
            {
                CurrentState.Phase = RunPhase.HandSelect;
            }

            return ServiceResult<RunState>.Ok(CurrentState);
        }

        public ServiceResult<BlindResolution> ResolveBlindResult()
        {
            if (CurrentState.Phase != RunPhase.BlindResult)
            {
                return ServiceResult<BlindResolution>.Fail(ErrorCode.StateConflict);
            }

            var passed = CurrentState.CurrentScore >= CurrentState.TargetScore;
            RunPhase nextPhase;

            if (!passed)
            {
                CurrentState.Phase = RunPhase.RunFail;
                nextPhase = RunPhase.RunFail;
            }
            else if (CurrentState.BlindType == BlindType.Boss && CurrentState.Ante >= 8)
            {
                CurrentState.Phase = RunPhase.RunComplete;
                nextPhase = RunPhase.RunComplete;
            }
            else
            {
                CurrentState.Phase = RunPhase.Shop;
                nextPhase = RunPhase.Shop;
            }

            return ServiceResult<BlindResolution>.Ok(new BlindResolution
            {
                Passed = passed,
                BlindType = CurrentState.BlindType,
                Ante = CurrentState.Ante,
                CurrentScore = CurrentState.CurrentScore,
                TargetScore = CurrentState.TargetScore,
                NextPhase = nextPhase
            });
        }

        public ServiceResult<RunState> AdvanceAfterShop()
        {
            if (CurrentState.Phase != RunPhase.Shop)
            {
                return ServiceResult<RunState>.Fail(ErrorCode.StateConflict);
            }

            var nextBlind = CurrentState.BlindType;
            var nextAnte = CurrentState.Ante;

            switch (CurrentState.BlindType)
            {
                case BlindType.Small:
                    nextBlind = BlindType.Big;
                    break;
                case BlindType.Big:
                    nextBlind = BlindType.Boss;
                    break;
                case BlindType.Boss:
                    nextBlind = BlindType.Small;
                    nextAnte += 1;
                    break;
            }

            CurrentState.Phase = RunPhase.AnteAdvance;
            CurrentState.Ante = nextAnte;
            CurrentState.BlindType = nextBlind;
            CurrentState.TargetScore = GetBlindTarget(CurrentState.Ante, CurrentState.BlindType);
            CurrentState.CurrentScore = 0;
            CurrentState.PlaysLeft = 4;
            CurrentState.DiscardsLeft = 3;
            CurrentState.Phase = RunPhase.HandSelect;

            return ServiceResult<RunState>.Ok(CurrentState);
        }

        private static int GetBlindTarget(int ante, BlindType blindType)
        {
            if (!BlindTargets.TryGetValue(ante, out var entry))
            {
                return 100;
            }

            return blindType switch
            {
                BlindType.Small => entry.Small,
                BlindType.Big => entry.Big,
                BlindType.Boss => entry.Boss,
                _ => entry.Small
            };
        }
    }
}
