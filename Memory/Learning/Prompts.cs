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
Determine if the topic has changed with a yes or no answer.

###Current Topic
{0}

###Conversation
{1}

###Answer
";

    }
}
