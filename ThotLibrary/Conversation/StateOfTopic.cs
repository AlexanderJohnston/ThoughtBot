using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThotLibrary.Conversation
{
    //public enum State { Asleep, Listening, WaitingForCommand, Memorize }
    //public enum Input { Acknowledge, Start, Continue, End }
    //public class StateOfTopic
    //{
    //    public State Current;

    //    public State NextState(Input input)
    //    {
    //        var newState = (Current, input) switch
    //        {
    //            (State.Asleep, Input.Acknowledge) => State.Listening,
    //            (State.Listening, Input.Start) => State.WaitingForCommand,
    //            (State.WaitingForCommand, Input.Continue) => State.WaitingForCommand,
    //            (State.WaitingForCommand, Input.End) => State.Memorize,
    //            _ => throw new NotSupportedException(
    //                $"{Current} has no transition on {input}")
    //        };
    //        Current = newState;
    //        return newState;
    //    }

        //public void Test(State current, Input input)
        //{
        //    var test = new Dictionary<ValueTuple<State, Input>, State>() 
        //    {
        //        { (State.Asleep, Input.Acknowledge), State.Listening },
        //        { (State.Listening, Input.Start), State.WaitingForCommand },
        //        { (State.WaitingForCommand, Input.Continue), State.WaitingForCommand },
        //        { (State.WaitingForCommand, Input.End), State.Memorize }
        //    };
        //    var output = (current, input);
        //    if (test.ContainsKey(output))
        //    {
        //        var yes = "ok";
        //    }
        //}
    //}
}
