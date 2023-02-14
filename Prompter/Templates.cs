namespace Prompter
{
    /// <summary>
    /// The Template class represents a <see langword="static"/> template with some limit to the number of tokens which can be used for each variable in that template.
    /// </summary>
    public class Template
    {

    }
    
    // Templates
    public static class Templates 
    {
        public static string Respond => @"### Instructions
{instructions}

### Memories
{memories}

### Conversation
{conversation}

### Response

";
        public static string Recall => @"Answer the question as truthfully as possible using the provided context, and if the answer is not contained within the text below, say ""I don't know.""

### Memories
{memories}

### Question to Answer

Q: {question}
A: ";

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
input: 'I want to go to the moon'
1. Decision: Yes
   Topic: Astronautics
   Reasoning: The user is expressing interest in space flight.
input: 'lol ok'
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