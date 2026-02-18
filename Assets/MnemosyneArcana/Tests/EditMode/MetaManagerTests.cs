using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class MetaManagerTests
    {
        private MetaManagerV2 _meta;

        [SetUp]
        public void SetUp()
        {
            _meta = new MetaManagerV2();
        }

        [Test]
        public void SettleRun_Ante5Clear_XP150_LP15()
        {
            var result = _meta.SettleRun(
                new RunResult { IsClear = true, HighestAnte = 5, ScoreTotal = 50000 },
                new MetaProgress { Xp = 0, Lp = 0 });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(150, result.Value.XpGained);
            Assert.AreEqual(15, result.Value.LpGainedBase);
        }

        [Test]
        public void SettleRun_Ante3Fail_XP60_LP6()
        {
            var result = _meta.SettleRun(
                new RunResult { IsClear = false, HighestAnte = 3, ScoreTotal = 1500 },
                new MetaProgress { Xp = 100, Lp = 20 });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(60, result.Value.XpGained);
            Assert.AreEqual(6, result.Value.LpGainedBase);
        }

        [Test]
        public void SettleRun_Ante1Fail_XP20_LP2()
        {
            var result = _meta.SettleRun(
                new RunResult { IsClear = false, HighestAnte = 1, ScoreTotal = 50 },
                new MetaProgress());
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(20, result.Value.XpGained);
            Assert.AreEqual(2, result.Value.LpGainedBase);
        }

        [Test]
        public void SettleRun_NullRunResult_Error()
        {
            var result = _meta.SettleRun(null, new MetaProgress());
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }

        [Test]
        public void GenerateContracts_SameSeed_SameResult()
        {
            var meta = new MetaProgress { Lp = 50 };
            var first = _meta.GenerateContracts(meta, 12345);
            var second = _meta.GenerateContracts(meta, 12345);
            Assert.IsTrue(first.IsSuccess);
            Assert.IsTrue(second.IsSuccess);
            Assert.AreEqual(first.Value.Count, second.Value.Count);
            for (int i = 0; i < first.Value.Count; i++)
            {
                Assert.AreEqual(first.Value[i].ContractId, second.Value[i].ContractId);
                Assert.AreEqual(first.Value[i].LpReward, second.Value[i].LpReward);
            }
        }

        [Test]
        public void GenerateContracts_Returns3()
        {
            var result = _meta.GenerateContracts(new MetaProgress(), 999);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.Value.Count);
        }

        [Test]
        public void GenerateContracts_AllFieldsPopulated()
        {
            var result = _meta.GenerateContracts(new MetaProgress(), 42);
            Assert.IsTrue(result.IsSuccess);
            foreach (var c in result.Value)
            {
                Assert.IsFalse(string.IsNullOrEmpty(c.ContractId));
                Assert.IsFalse(string.IsNullOrEmpty(c.ContractType));
                Assert.That(c.Tier, Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(3));
                Assert.That(c.LpReward, Is.GreaterThan(0));
            }
        }

        [Test]
        public void SettleContract_Completed_ReturnsLpReward()
        {
            var contract = new Contract { ContractId = "CT_NAT_001", LpReward = 8, Tier = 2 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var result = _meta.SettleContract(contract, telemetry);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Completed);
            Assert.AreEqual(8, result.Value.LpBonusRaw);
        }

        [Test]
        public void SettleContract_NotCompleted_ZeroLp()
        {
            var contract = new Contract { ContractId = "CT_NAT_001", LpReward = 8, Tier = 2 };
            var telemetry = new RunTelemetry { ContractCompleted = false };
            var result = _meta.SettleContract(contract, telemetry);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.Completed);
            Assert.AreEqual(0, result.Value.LpBonusRaw);
            Assert.AreEqual(0, result.Value.LpBonusCapped);
        }

        [Test]
        public void SettleContractWithCap_UnderCap_NoAdjustment()
        {
            var contract = new Contract { ContractId = "CT_01", LpReward = 6, Tier = 1 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var result = _meta.SettleContractWithCap(contract, telemetry, lpBase: 15);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(6, result.Value.LpBonusRaw);
            Assert.AreEqual(6, result.Value.LpBonusCapped);
            Assert.IsFalse(result.Value.CapApplied);
        }

        [Test]
        public void SettleContractWithCap_OverCap_Capped()
        {
            var contract = new Contract { ContractId = "CT_02", LpReward = 15, Tier = 3 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var result = _meta.SettleContractWithCap(contract, telemetry, lpBase: 6);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(15, result.Value.LpBonusRaw);
            Assert.AreEqual(4, result.Value.LpBonusCapped);  // floor(6*45/55) = 4
            Assert.IsTrue(result.Value.CapApplied);
        }

        [Test]
        public void SettleContractWithCap_ZeroBase_CapIsZero()
        {
            var contract = new Contract { ContractId = "CT_03", LpReward = 10, Tier = 2 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var result = _meta.SettleContractWithCap(contract, telemetry, lpBase: 0);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Value.LpBonusCapped);
            Assert.IsTrue(result.Value.CapApplied);
        }

        [Test]
        public void SettleRun_WithContractLp_TotalCorrect()
        {
            var result = _meta.SettleRun(
                new RunResult { IsClear = true, HighestAnte = 4, ScoreTotal = 5000 },
                new MetaProgress());
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(130, result.Value.XpGained);
            Assert.AreEqual(13, result.Value.LpGainedBase);
            Assert.AreEqual(13, result.Value.LpGainedTotal);
        }

        [Test]
        public void TryUnlockNode_EnoughLpAndNoPrereq_Succeeds()
        {
            var result = _meta.TryUnlockNode("FLU_01", new MetaProgress
            {
                Lp = 30,
                CurriculumNodes = new string[0]
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Success);
            Assert.AreEqual("FLU_01", result.Value.NodeId);
            Assert.AreEqual(20, result.Value.SpentLp);
            Assert.AreEqual(10, result.Value.RemainingLp);
            CollectionAssert.Contains(result.Value.UnlockedNodes, "FLU_01");
        }

        [Test]
        public void TryUnlockNode_MissingPrereq_ReturnsStateConflict()
        {
            var result = _meta.TryUnlockNode("FLU_02", new MetaProgress
            {
                Lp = 50,
                CurriculumNodes = new string[0]
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void TryUnlockNode_MutexConflict_ReturnsStateConflict()
        {
            var result = _meta.TryUnlockNode("FLU_03B", new MetaProgress
            {
                Lp = 100,
                CurriculumNodes = new[] { "FLU_01", "FLU_02", "FLU_03A" }
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void TryUnlockNode_InsufficientLp_ReturnsStateConflict()
        {
            var result = _meta.TryUnlockNode("MAS_01", new MetaProgress
            {
                Lp = 10,
                CurriculumNodes = new string[0]
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void TryUnlockNode_AlreadyUnlocked_ReturnsStateConflict()
        {
            var result = _meta.TryUnlockNode("LEX_01", new MetaProgress
            {
                Lp = 100,
                CurriculumNodes = new[] { "LEX_01" }
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void Curriculum_AllDefinedNodes_CanUnlockWithSatisfiedConditions()
        {
            foreach (var node in GetCurriculumNodeDefs())
            {
                var prereqs = node.RequiredAnyOfGroups
                    .Where(group => group.Length > 0)
                    .Select(group => group[0])
                    .Distinct()
                    .ToArray();

                var result = _meta.TryUnlockNode(node.NodeId, new MetaProgress
                {
                    Lp = node.Cost + 10,
                    CurriculumNodes = prereqs
                });

                Assert.IsTrue(result.IsSuccess, $"Node {node.NodeId} should unlock when prerequisites are met.");
                Assert.AreEqual(node.Cost, result.Value.SpentLp);
                CollectionAssert.Contains(result.Value.UnlockedNodes, node.NodeId);
            }
        }

        [Test]
        public void Curriculum_AllDefinedNodes_EnforcePrereqAndMutexConstraints()
        {
            foreach (var node in GetCurriculumNodeDefs())
            {
                if (node.RequiredAnyOfGroups.Length > 0)
                {
                    var missingPrereq = _meta.TryUnlockNode(node.NodeId, new MetaProgress
                    {
                        Lp = node.Cost + 10,
                        CurriculumNodes = Array.Empty<string>()
                    });
                    Assert.IsFalse(missingPrereq.IsSuccess, $"Node {node.NodeId} should fail without prerequisites.");
                    Assert.AreEqual(ErrorCode.StateConflict, missingPrereq.Error);
                }

                if (node.MutexWith.Length > 0)
                {
                    var prereqs = node.RequiredAnyOfGroups
                        .Where(group => group.Length > 0)
                        .Select(group => group[0])
                        .Distinct()
                        .ToList();
                    prereqs.Add(node.MutexWith[0]);

                    var mutexConflict = _meta.TryUnlockNode(node.NodeId, new MetaProgress
                    {
                        Lp = node.Cost + 10,
                        CurriculumNodes = prereqs.ToArray()
                    });
                    Assert.IsFalse(mutexConflict.IsSuccess, $"Node {node.NodeId} should fail when mutex node is unlocked.");
                    Assert.AreEqual(ErrorCode.StateConflict, mutexConflict.Error);
                }
            }
        }

        private static IReadOnlyList<CurriculumNodeInfo> GetCurriculumNodeDefs()
        {
            var defsField = typeof(MetaManagerV2).GetField("CurriculumNodeDefs", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(defsField);

            var defsObject = defsField.GetValue(null);
            Assert.NotNull(defsObject);

            var result = new List<CurriculumNodeInfo>();
            var enumerable = (System.Collections.IEnumerable)defsObject;
            foreach (var entry in enumerable)
            {
                var entryType = entry.GetType();
                var keyProp = entryType.GetProperty("Key");
                var valueProp = entryType.GetProperty("Value");
                Assert.NotNull(keyProp);
                Assert.NotNull(valueProp);

                var nodeId = (string)keyProp.GetValue(entry);
                var value = valueProp.GetValue(entry);
                var valueType = value.GetType();

                var cost = (int)valueType.GetProperty("Cost").GetValue(value);
                var groupsRaw = (System.Collections.IEnumerable)valueType.GetProperty("RequiredAnyOfGroups").GetValue(value);
                var mutexRaw = (System.Collections.IEnumerable)valueType.GetProperty("MutexWith").GetValue(value);

                var groups = new List<string[]>();
                foreach (var groupObj in groupsRaw)
                {
                    var groupItems = new List<string>();
                    foreach (var groupItem in (System.Collections.IEnumerable)groupObj)
                    {
                        groupItems.Add(groupItem.ToString());
                    }

                    groups.Add(groupItems.ToArray());
                }

                var mutex = new List<string>();
                foreach (var m in mutexRaw)
                {
                    mutex.Add(m.ToString());
                }

                result.Add(new CurriculumNodeInfo
                {
                    NodeId = nodeId,
                    Cost = cost,
                    RequiredAnyOfGroups = groups.ToArray(),
                    MutexWith = mutex.ToArray()
                });
            }

            Assert.IsNotEmpty(result);
            return result;
        }

        private sealed class CurriculumNodeInfo
        {
            public string NodeId { get; set; }
            public int Cost { get; set; }
            public string[][] RequiredAnyOfGroups { get; set; }
            public string[] MutexWith { get; set; }
        }
    }
}
