using UnityEngine;
using System;

namespace razz
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ConditionalAttribute : PropertyAttribute
    {
        public Condition Action { get; private set; }
        public Op Operator { get; private set; }
        public string[] Conditions { get; private set; }

        public ConditionalAttribute(Condition action, Op conditionOperator, params string[] conditions)
        {
            Action = action;
            Operator = conditionOperator;
            Conditions = conditions;
        }

        public ConditionalAttribute(Condition action, params string[] conditions)
        {
            Action = action;
            Conditions = conditions;
        }
    }

    public enum Condition
    {
        Show,
        Enable,
    }

    public enum Op
    {
        And,
        Or,
    }
}
