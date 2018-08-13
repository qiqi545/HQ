using Xunit;

namespace HQ.Flow.Tests
{
	public class StateMachineTests
    {
        [Fact]
        public void Can_transition_between_states()
        {
			StateProvider.Setup(typeof(Actor).Assembly, typeof(AlwaysNullCheckedAttribute).Assembly);

	        var userData = new UserData();

			var bo = new Actor();

	        bo.SetState<Actor.StateA>(userData);
	        bo.SetState<Actor.StateB>(userData);
			bo.SetState<Actor.StateC>(userData);

			Assert.True(userData.BeginStateA);
	        Assert.True(userData.EndStateA);
			Assert.True(userData.BeginStateB);
	        Assert.True(userData.EndStateB);
		}

	    public class UserData
	    {
		    public bool BeginStateA;
		    public bool EndStateA;
		    public bool BeginStateB;
		    public bool EndStateB;
	    }

		public class Actor : StateMachine<UserData>
	    {
		    #region StateA

		    public class StateA : State { }

		    private void State_StateA_BeginState(UserData userData, State previousState)
		    {
			    userData.BeginStateA = true;
		    }

		    private void State_StateA_EndState(UserData userData, State nextState)
		    {
			    userData.EndStateA = true;
		    }

		    #endregion

		    #region StateB

		    public class StateB : State { }

		    private void State_StateB_BeginState(UserData userData, State previousState)
		    {
			    userData.BeginStateB = true;
		    }

		    private void State_StateB_EndState(UserData userData, State nextState)
		    {
			    userData.EndStateB = true;
		    }

			#endregion

		    #region StateC

		    public class StateC : State { }

		    private void State_StateC_BeginState(UserData userData, State previousState)
		    {
				
			}

		    private void State_StateC_EndState(UserData userData, State nextState)
		    {
			    
		    }

		    #endregion
		}
	}
}
