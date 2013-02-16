/*
 * [The "BSD license"]
 *  Copyright (c) 2013 Terence Parr
 *  Copyright (c) 2013 Sam Harwell
 *  All rights reserved.
 *
 *  Redistribution and use in source and binary forms, with or without
 *  modification, are permitted provided that the following conditions
 *  are met:
 *
 *  1. Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 *  2. Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 *  3. The name of the author may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 *  THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 *  IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 *  OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 *  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 *  INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 *  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 *  DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 *  THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 *  (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 *  THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using System;
using System.Collections.Generic;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Sharpen;

namespace Antlr4.Runtime.Atn
{
    /// <summary>
    /// The following images show the relation of states and
    /// <see cref="transitions">transitions</see>
    /// for various grammar constructs.
    /// <ul>
    /// <li>Solid edges marked with an &#0949; indicate a required
    /// <see cref="EpsilonTransition">EpsilonTransition</see>
    /// .</li>
    /// <li>Dashed edges indicate locations where any transition derived from
    /// <see cref="Transition">Transition</see>
    /// might appear.</li>
    /// <li>Dashed nodes are place holders for either a sequence of linked
    /// <see cref="BasicState">BasicState</see>
    /// states or the inclusion of a block representing a nested
    /// construct in one of the forms below.</li>
    /// <li>Nodes showing multiple outgoing alternatives with a
    /// <code>...</code>
    /// support
    /// any number of alternatives (one or more). Nodes without the
    /// <code>...</code>
    /// only
    /// support the exact number of alternatives shown in the diagram.</li>
    /// </ul>
    /// <h2>Basic Blocks</h2>
    /// <h3>Rule</h3>
    /// <embed src="images/Rule.svg" type="image/svg+xml"/>
    /// <h3>Block of 1 or more alternatives</h3>
    /// <embed src="images/Block.svg" type="image/svg+xml"/>
    /// <h2>Greedy Loops</h2>
    /// <h3>Greedy Closure:
    /// <code>(...)*</code>
    /// </h3>
    /// <embed src="images/ClosureGreedy.svg" type="image/svg+xml"/>
    /// <h3>Greedy Positive Closure:
    /// <code>(...)+</code>
    /// </h3>
    /// <embed src="images/PositiveClosureGreedy.svg" type="image/svg+xml"/>
    /// <h3>Greedy Optional:
    /// <code>(...)?</code>
    /// </h3>
    /// <embed src="images/OptionalGreedy.svg" type="image/svg+xml"/>
    /// <h2>Non-Greedy Loops</h2>
    /// <h3>Non-Greedy Closure:
    /// <code>(...)*?</code>
    /// </h3>
    /// <embed src="images/ClosureNonGreedy.svg" type="image/svg+xml"/>
    /// <h3>Non-Greedy Positive Closure:
    /// <code>(...)+?</code>
    /// </h3>
    /// <embed src="images/PositiveClosureNonGreedy.svg" type="image/svg+xml"/>
    /// <h3>Non-Greedy Optional:
    /// <code>(...)??</code>
    /// </h3>
    /// <embed src="images/OptionalNonGreedy.svg" type="image/svg+xml"/>
    /// </summary>
    public abstract class ATNState
    {
        public const int InitialNumTransitions = 4;

        public const int InvalidType = 0;

        public const int Basic = 1;

        public const int RuleStart = 2;

        public const int BlockStart = 3;

        public const int PlusBlockStart = 4;

        public const int StarBlockStart = 5;

        public const int TokenStart = 6;

        public const int RuleStop = 7;

        public const int BlockEnd = 8;

        public const int StarLoopBack = 9;

        public const int StarLoopEntry = 10;

        public const int PlusLoopBack = 11;

        public const int LoopEnd = 12;

        public static readonly IList<string> serializationNames = Sharpen.Collections.UnmodifiableList
            (Arrays.AsList("INVALID", "BASIC", "RULE_START", "BLOCK_START", "PLUS_BLOCK_START"
            , "STAR_BLOCK_START", "TOKEN_START", "RULE_STOP", "BLOCK_END", "STAR_LOOP_BACK"
            , "STAR_LOOP_ENTRY", "PLUS_LOOP_BACK", "LOOP_END"));

        public const int InvalidStateNumber = -1;

        /// <summary>Which ATN are we in?</summary>
        public ATN atn = null;

        public int stateNumber = InvalidStateNumber;

        public int ruleIndex;

        public bool epsilonOnlyTransitions = false;

        /// <summary>Track the transitions emanating from this ATN state.</summary>
        /// <remarks>Track the transitions emanating from this ATN state.</remarks>
        protected internal readonly IList<Antlr4.Runtime.Atn.Transition> transitions = new 
            List<Antlr4.Runtime.Atn.Transition>(InitialNumTransitions);

        protected internal IList<Antlr4.Runtime.Atn.Transition> optimizedTransitions;

        /// <summary>Used to cache lookahead during parsing, not used during construction</summary>
        public IntervalSet nextTokenWithinRule;

        // constants for serialization
        // at runtime, we don't have Rule objects
        /// <summary>Gets the state number.</summary>
        /// <remarks>Gets the state number.</remarks>
        /// <returns>the state number</returns>
        public int GetStateNumber()
        {
            return stateNumber;
        }

        /// <summary>
        /// For all states except
        /// <see cref="RuleStopState">RuleStopState</see>
        /// , this returns the state
        /// number. Returns -1 for stop states.
        /// </summary>
        /// <returns>
        /// -1 for
        /// <see cref="RuleStopState">RuleStopState</see>
        /// , otherwise the state number
        /// </returns>
        public virtual int GetNonStopStateNumber()
        {
            return GetStateNumber();
        }

        public override int GetHashCode()
        {
            return stateNumber;
        }

        public override bool Equals(object o)
        {
            // are these states same object?
            if (o is ATNState)
            {
                return stateNumber == ((ATNState)o).stateNumber;
            }
            return false;
        }

        public virtual bool IsNonGreedyExitState()
        {
            return false;
        }

        public override string ToString()
        {
            return stateNumber.ToString();
        }

        public virtual Antlr4.Runtime.Atn.Transition[] GetTransitions()
        {
            return Sharpen.Collections.ToArray(transitions, new Antlr4.Runtime.Atn.Transition
                [transitions.Count]);
        }

        public virtual int GetNumberOfTransitions()
        {
            return transitions.Count;
        }

        public virtual void AddTransition(Antlr4.Runtime.Atn.Transition e)
        {
            if (transitions.IsEmpty())
            {
                epsilonOnlyTransitions = e.IsEpsilon;
            }
            else
            {
                if (epsilonOnlyTransitions != e.IsEpsilon)
                {
                    System.Console.Error.Format("ATN state %d has both epsilon and non-epsilon transitions.\n"
                        , stateNumber);
                    epsilonOnlyTransitions = false;
                }
            }
            transitions.AddItem(e);
        }

        public virtual Antlr4.Runtime.Atn.Transition Transition(int i)
        {
            return transitions[i];
        }

        public virtual void SetTransition(int i, Antlr4.Runtime.Atn.Transition e)
        {
            transitions.Set(i, e);
        }

        public virtual Antlr4.Runtime.Atn.Transition RemoveTransition(int index)
        {
            return transitions.Remove(index);
        }

        public abstract int GetStateType();

        public bool OnlyHasEpsilonTransitions()
        {
            return epsilonOnlyTransitions;
        }

        public virtual void SetRuleIndex(int ruleIndex)
        {
            this.ruleIndex = ruleIndex;
        }

        public virtual bool IsOptimized()
        {
            return optimizedTransitions != transitions;
        }

        public virtual int GetNumberOfOptimizedTransitions()
        {
            return optimizedTransitions.Count;
        }

        public virtual Antlr4.Runtime.Atn.Transition GetOptimizedTransition(int i)
        {
            return optimizedTransitions[i];
        }

        public virtual void AddOptimizedTransition(Antlr4.Runtime.Atn.Transition e)
        {
            if (!IsOptimized())
            {
                optimizedTransitions = new List<Antlr4.Runtime.Atn.Transition>();
            }
            optimizedTransitions.AddItem(e);
        }

        public virtual void SetOptimizedTransition(int i, Antlr4.Runtime.Atn.Transition e
            )
        {
            if (!IsOptimized())
            {
                throw new InvalidOperationException();
            }
            optimizedTransitions.Set(i, e);
        }

        public virtual void RemoveOptimizedTransition(int i)
        {
            if (!IsOptimized())
            {
                throw new InvalidOperationException();
            }
            optimizedTransitions.Remove(i);
        }

        public ATNState()
        {
            optimizedTransitions = transitions;
        }
    }
}
