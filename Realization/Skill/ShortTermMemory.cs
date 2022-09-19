﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Realization.Intent;
using ThotLibrary;

namespace Realization.Skill
{
    public enum MemoryType 
    {
        Command,
        Question,
        FormulatedIntent,
        LearnedSkill
    }
    public class MemoryContext
    {
        public string ContextId;
        public string RelatedToContextId;
        public string Author;
        public MemoryType Type;
        public ulong Channel;
        public Intention Intention;
        
        public MemoryContext(string userName, MemoryType type, ulong channel, Intention intention, string relatedContext = "")
        {
            Author = userName;
            Type = type;
            Channel = channel;
            RelatedToContextId = relatedContext;
            ContextId = Guid.NewGuid().ToString();
            Intention = intention;
        }
    }
    public class Memory<T>
    {
        public T Value;
        public MemoryContext Context;
    }
    public class ShortTermMemory<T>
    {
        private Dictionary<ulong, string> FocusedMemory;
        private Dictionary<ulong, List<Memory<T>>> Users;

        public ShortTermMemory()
        {
            FocusedMemory = new Dictionary<ulong, string>();
            Users = new Dictionary<ulong, List<Memory<T>>>();
        }

        public void Remember(T value, IUser user, MemoryType type, ulong channel, Intention intention, string relatedContext = "")
        {
            var userId = user.DiscriminatorValue;

            if (!Remembers(userId))
            {
                Users.Add(userId, new List<Memory<T>>());
            }

            var memories = Recall(userId);
            var memory = new Memory<T>();
            memory.Value = value;
            memory.Context = new MemoryContext(user.Username, type, channel, intention, relatedContext);
            memories.Add(memory);
        }

        public string GetFocusedMemory(SocketUser user) => FocusedMemory.FirstOrDefault(memory => memory.Key == user.DiscriminatorValue).Value;

        public void SaveFocusedMemory(SocketUser user, string memory) => FocusedMemory[user.DiscriminatorValue] = memory;

        public Memory<T> MemoryBeforeLast(SocketUser user)
        {
            var memory = Recall(user.DiscriminatorValue);
            return memory[memory.Count - 2];
        }

        public Memory<T> LastMemoryObject(SocketUser user) => Recall(user.DiscriminatorValue).Last();

        public string LastMemory(IUser user)
        {
            var userId = user.DiscriminatorValue;
            if (Remembers(userId))
            {
                var lastMemory = Recall(userId).Last();
                return string.Format("The last thing I remember from {0} is the {1}: {2}",
                    lastMemory.Context.Author,
                    lastMemory.Context.Type,
                    lastMemory.Value.ToString());
            }
            else return string.Format("I don't remember anything yet for {0}", user.Username);
        }

        public List<string> AllMemories(IUser user)
        {
            var explainedMemories = new List<string>();
            var userId = user.DiscriminatorValue;
            if (Remembers(userId))
            {
                var memories = Recall(userId);
                foreach (var memory in memories)
                {
                    var explained = string.Format("I remember {0} providing the intent {1} to {2}: {3}",
                        memory.Context.Author,
                        memory.Context.Intention.Name,
                        memory.Context.Type,
                        memory.Value.ToString());
                    explainedMemories.Add(explained);
                }
            }
            return explainedMemories;
        }
        
        private bool Matches(Memory<T> memory, string context) => memory.Context.ContextId == context;
        public bool Remembers(ulong userId) => Users.Any(user => user.Key == userId);
        private List<Memory<T>> Recall(ulong userId) => Users.First(user => user.Key == userId).Value;
        private Memory<T> Specific(string context, ulong userId) => 
            Recall(userId).First(memory => Matches(memory, context));
    }
}
