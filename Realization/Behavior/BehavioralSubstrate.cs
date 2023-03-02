using Prompter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Behavior
{
    /// <summary>
    /// This dictates the flow of behavior for the bot.
    /// </summary>
    public class BehavioralSubstrate
    {
        // A dictionary of IExpression behaviors.
        public Dictionary<string, IExpression> Behaviors { get; set; } = new();
        
        // Initialize the class by default with a single behavior.
        public BehavioralSubstrate()
        {
            Behaviors.Add("zero", new Prompt(Templates.Respond, "zero"));
        }

        // This is the method that will be called by the bot to determine what to do.
        public IExpression DetermineBehavior(string input)
        {
            // If the input is empty, return the default behavior.
            if (string.IsNullOrEmpty(input))
            {
                return Behaviors["zero"];
            }

            // If the input is not empty, return the behavior that matches the input.
            return Behaviors[input];
        }

        public void AddBehavior(string name, IExpression behavior)
        {
            Behaviors.Add(name, behavior);
        }
    }

    /// <summary>
    ///  This tracks the intent of the user's input and facilitates the creation of new behaviors in the substrate.
    /// </summary>
    public class DefaultModeNetwork
    {
        // This is the default behavior that will be used if the user's input is empty.
        public IExpression DefaultBehavior { get; set; } = new Prompt(Templates.Respond, "zero");

        // This is the behavior that will be used if the user's input is not empty.
        public IExpression CurrentBehavior { get; set; } = new Prompt(Templates.Respond, "zero");

        // This is the method that will be called by the bot to determine what to do.
        public IExpression DetermineBehavior(string input)
        {
            // If the input is empty, return the default behavior.
            if (string.IsNullOrEmpty(input))
            {
                return DefaultBehavior;
            }

            // If the input is not empty, return the behavior that matches the input.
            return CurrentBehavior;
        }

        // This is the method that will be called by the bot to determine what to do.
        public void SetBehavior(string input)
        {
            // If the input is empty, return the default behavior.
            if (string.IsNullOrEmpty(input))
            {
                CurrentBehavior = DefaultBehavior;
            }

            // If the input is not empty, return the behavior that matches the input.
            CurrentBehavior = new Prompt(Templates.Respond, input);
        }

        public void SetBehavior(IExpression behavior)
        {
            CurrentBehavior = behavior;
        }

        public void SetDefaultBehavior(IExpression behavior)
        {
            DefaultBehavior = behavior;
        }

        public void SetDefaultBehavior(string input)
        {
            DefaultBehavior = new Prompt(Templates.Respond, input);
        }
    }
}
