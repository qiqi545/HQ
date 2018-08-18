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
		        StateProvider.Setup(typeof(ThreeValidStates));

		        var userData = new ThreeValidStatesData();
		        var bo = new ThreeValidStates();

		        bo.SetState<ThreeValidStates.StateA>(userData);
		        bo.SetState<ThreeValidStates.StateB>(userData);
		        bo.SetState<ThreeValidStates.StateC>(userData);

		        Assert.True(userData.BeginStateA);
		        Assert.True(userData.EndStateA);
		        Assert.True(userData.BeginStateB);
		        Assert.True(userData.EndStateB);
			}
		}
	}
}
