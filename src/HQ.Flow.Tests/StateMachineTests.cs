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
		        StateProvider.Setup(typeof(ThreeValidStaticStates));

		        var userData = new ThreeValidStatesData();
		        var bo = new ThreeValidStaticStates();

		        bo.SetState<ThreeValidStaticStates.StateA>(userData);
		        bo.SetState<ThreeValidStaticStates.StateB>(userData);
		        bo.SetState<ThreeValidStaticStates.StateC>(userData);

		        Assert.True(userData.BeginStateA);
		        Assert.True(userData.EndStateA);
		        Assert.True(userData.BeginStateB);
		        Assert.True(userData.EndStateB);
			}
		}
	}
}
