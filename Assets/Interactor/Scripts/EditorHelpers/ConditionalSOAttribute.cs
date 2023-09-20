using UnityEngine;
using System;

namespace razz
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ConditionalSOAttribute : PropertyAttribute
    {
        public Condition Action { get; private set; }
        public Op Operator { get; private set; }
        public string[] Conditions { get; private set; }

        public ConditionalSOAttribute(Condition action, Op conditionOperator, params string[] conditions)
        {
            Action = action;
            Operator = conditionOperator;
            Conditions = conditions;
        }

        public ConditionalSOAttribute(Condition action, params string[] conditions)
        {
            Action = action;
            Conditions = conditions;
        }
    }
}
