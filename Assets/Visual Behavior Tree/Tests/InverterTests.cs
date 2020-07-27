using Assets.Scripts.AI;
using Assets.Scripts.AI.Decorators;
using NUnit.Framework;
using System.Collections.Generic;
using UniRx;

namespace Assets.Visual_Behavior_Tree.Tests
{
    class InverterTests
    {
        [Test, Description("Inverter should return succeed when child fails)")]
        public void TestInverterInvertsFail()
        {
            var inverter = new Inverter("", 1, 1);
            inverter.Children.Add(TestingResources.GetRunRunFail());

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Success);

            inverter.Start().Subscribe((x) => actual.Add(x));

            Assert.AreEqual(expected, actual);
        }

        [Test, Description("Inverter should return fail when child succeeds")]
        public void TestInverterInvertsSuccess()
        {
            var inverter = new Inverter("", 1, 1);
            inverter.Children.Add(TestingResources.GetRunRunSuccess());

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Fail);

            inverter.Start().Subscribe((x) => actual.Add(x));

            Assert.AreEqual(expected, actual);
        }
    }
}
