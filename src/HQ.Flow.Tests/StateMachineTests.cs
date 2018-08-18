using HQ.Flow.Tests.Fixtures;
using HQ.Flow.Tests.States;
using Xunit;

namespace HQ.Flow.Tests
{
	public class StateMachineTests
	{
		[Fact]
		public void Can_transition_between_states()
		{
			using (new StateProviderFixture())
			{
				StateProvider.Setup<ThreeValidStaticStates>();

				var data = new ThreeValidStatesData();
				var actor = new ThreeValidStaticStates();

				actor.SetState<ThreeValidStaticStates.StateA>(data);
				actor.SetState<ThreeValidStaticStates.StateB>(data);
				actor.SetState<ThreeValidStaticStates.StateC>(data);

				Assert.True(data.BeginStateA);
				Assert.True(data.EndStateA);
				Assert.True(data.BeginStateB);
				Assert.True(data.EndStateB);
			}
		}

		[Fact]
		public void Can_inherit_states_and_pass_null_context()
		{
			using (new StateProviderFixture())
			{
				StateProvider.Setup<StateInheritance>();
				var actor = new StateInheritance();
				actor.SetState<StateInheritance.StateA>();
			}
		}

		[Fact]
		public void Can_inherit_state_data()
		{
			using (new StateProviderFixture())
			{
				StateProvider.Setup<StateDataInheritance>();

				var data = new BaseStateData {A = true};
				var actor = new StateDataInheritance();
				actor.SetState<StateDataInheritance.StateA>(data);
				Assert.False(data.A);
			}
		}
	}
}
