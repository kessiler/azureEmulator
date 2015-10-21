#region

using System.Collections.Generic;
using System.Linq;
using Azure.Messages;

#endregion

namespace Azure.HabboHotel.Polls
{
    /// <summary>
    ///     Class PollQuestion.
    /// </summary>
    internal class PollQuestion
    {
        /// <summary>
        ///     The answers
        /// </summary>
        internal List<string> Answers = new List<string>();

        /// <summary>
        ///     a type
        /// </summary>
        internal PollAnswerType AType;

        /// <summary>
        ///     The correct answer
        /// </summary>
        internal string CorrectAnswer;

        /// <summary>
        ///     The index
        /// </summary>
        internal uint Index;

        /// <summary>
        ///     The question
        /// </summary>
        internal string Question;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PollQuestion" /> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="question">The question.</param>
        /// <param name="aType">a type.</param>
        /// <param name="answers">The answers.</param>
        /// <param name="correctAnswer">The correct answer.</param>
        internal PollQuestion(uint index, string question, int aType, IEnumerable<string> answers, string correctAnswer)
        {
            Index = index;
            Question = question;
            AType = (PollAnswerType) aType;
            Answers = answers.ToList();
            CorrectAnswer = correctAnswer;
        }

        /// <summary>
        ///     Serializes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="questionNumber">The question number.</param>
        public void Serialize(ServerMessage message, int questionNumber)
        {
            message.AppendInteger(Index);
            message.AppendInteger(questionNumber);
            message.AppendInteger((int) AType);
            message.AppendString(Question);
            if (AType != PollAnswerType.Selection && AType != PollAnswerType.RadioSelection)
            {
                return;
            }
            message.AppendInteger(1);
            message.AppendInteger(Answers.Count);
            foreach (var current in Answers)
            {
                message.AppendString(current);
                message.AppendString(current);
            }
        }

        /// <summary>
        ///     Enum PollAnswerType
        /// </summary>
        internal enum PollAnswerType
        {
            /// <summary>
            ///     The radio selection
            /// </summary>
            RadioSelection = 1,

            /// <summary>
            ///     The selection
            /// </summary>
            Selection = 2,

            /// <summary>
            ///     The text
            /// </summary>
            Text = 3
        }
    }
}