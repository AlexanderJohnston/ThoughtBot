namespace Memory.Learning
{
    public static class Prompts
    {
        public static string Intent = @"
###Goal
Predict the current intent of the following chat.

###Conversation
{0}

###Intent
";

        public static string TopicShift = @"
###Goal
Determine if the topic has changed with a yes or no answer. If this topic is not yet known, yes is a good answer in almost all cases. You must provide the decision, the topic, and the reasoning in your answer. Keep reasoning to one sentence at most.
###Example Answers
Do not re-use the examples in your own answers.
1. Decision: Yes
   Topic: Astronautics
   Reasoning: The user is expressing interest in space flight.
2. Decision: No
   Topic: Still being determined.
   Reasoning: The user's request is unclear.

###Previous Topic
{0}

###Conversation
{1}

###Answer
";

        public static string OldTopicShift = @"
###Goal
Determine if the topic has changed with a yes or no answer.

###Previous Topic
{0}

###Conversation
{1}

###Answer
";

    }
}
