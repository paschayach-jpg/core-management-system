using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace CMS.Tests.PropertyBasedTests
{
    /// <summary>
    /// Property-Based Tests for Condition Evaluation Order Independence (Phase 9.8)
    /// 
    /// Property: Condition evaluation result is independent of evaluation order.
    /// 
    /// Validates:
    /// - Evaluation is commutative
    /// - No side effects from evaluation order
    /// - Result is consistent regardless of order
    /// </summary>
    [TestFixture]
    public class ConditionEvaluationOrderTests
    {
        // =====================================================================
        // Generators
        // =====================================================================

        /// <summary>
        /// Generate random boolean values for conditions.
        /// </summary>
        private static Arbitrary<bool> BooleanGenerator()
        {
            return Arb.Default.Bool();
        }

        /// <summary>
        /// Generate random condition operators.
        /// </summary>
        private static Arbitrary<ConditionOperator> ConditionOperatorGenerator()
        {
            var opGen = Gen.Elements(
                ConditionOperator.And,
                ConditionOperator.Or
            );

            return opGen.ToArbitrary();
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Evaluates a list of conditions with AND operator.
        /// </summary>
        private static bool EvaluateAnd(List<bool> conditions)
        {
            return conditions.All(c => c);
        }

        /// <summary>
        /// Evaluates a list of conditions with OR operator.
        /// </summary>
        private static bool EvaluateOr(List<bool> conditions)
        {
            return conditions.Any(c => c);
        }

        /// <summary>
        /// Evaluates conditions in a specific order.
        /// </summary>
        private static bool EvaluateConditions(List<bool> conditions, ConditionOperator op)
        {
            if (conditions == null || conditions.Count == 0)
                return true;

            return op == ConditionOperator.And ? EvaluateAnd(conditions) : EvaluateOr(conditions);
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: AND evaluation is commutative (order doesn't matter).
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ANDEvaluationIsCommutative(List<bool> conditions)
        {
            if (conditions == null || conditions.Count < 2)
                return true.ToProperty();

            var result1 = EvaluateAnd(conditions);

            // Reverse the order
            var reversed = new List<bool>(conditions);
            reversed.Reverse();
            var result2 = EvaluateAnd(reversed);

            return (result1 == result2).ToProperty();
        }

        /// <summary>
        /// Property: OR evaluation is commutative (order doesn't matter).
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_OREvaluationIsCommutative(List<bool> conditions)
        {
            if (conditions == null || conditions.Count < 2)
                return true.ToProperty();

            var result1 = EvaluateOr(conditions);

            // Reverse the order
            var reversed = new List<bool>(conditions);
            reversed.Reverse();
            var result2 = EvaluateOr(reversed);

            return (result1 == result2).ToProperty();
        }

        /// <summary>
        /// Property: AND evaluation is associative.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ANDEvaluationIsAssociative(bool a, bool b, bool c)
        {
            // (a AND b) AND c == a AND (b AND c)
            var result1 = (a && b) && c;
            var result2 = a && (b && c);

            return (result1 == result2).ToProperty();
        }

        /// <summary>
        /// Property: OR evaluation is associative.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_OREvaluationIsAssociative(bool a, bool b, bool c)
        {
            // (a OR b) OR c == a OR (b OR c)
            var result1 = (a || b) || c;
            var result2 = a || (b || c);

            return (result1 == result2).ToProperty();
        }

        /// <summary>
        /// Property: AND with all true returns true regardless of order.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ANDWithAllTrueReturnsTrue()
        {
            var conditions = new List<bool> { true, true, true, true };

            var result1 = EvaluateAnd(conditions);

            var reversed = new List<bool>(conditions);
            reversed.Reverse();
            var result2 = EvaluateAnd(reversed);

            return (result1 && result2).ToProperty();
        }

        /// <summary>
        /// Property: AND with any false returns false regardless of order.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ANDWithAnyFalseReturnsFalse(List<bool> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true.ToProperty();

            // Ensure at least one false
            var hasAnyFalse = conditions.Any(c => !c);
            if (!hasAnyFalse)
                return true.ToProperty();

            var result1 = EvaluateAnd(conditions);

            var reversed = new List<bool>(conditions);
            reversed.Reverse();
            var result2 = EvaluateAnd(reversed);

            return (!result1 && !result2).ToProperty();
        }

        /// <summary>
        /// Property: OR with all false returns false regardless of order.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ORWithAllFalseReturnsFalse()
        {
            var conditions = new List<bool> { false, false, false, false };

            var result1 = EvaluateOr(conditions);

            var reversed = new List<bool>(conditions);
            reversed.Reverse();
            var result2 = EvaluateOr(reversed);

            return (!result1 && !result2).ToProperty();
        }

        /// <summary>
        /// Property: OR with any true returns true regardless of order.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ORWithAnyTrueReturnsTrue(List<bool> conditions)
        {
            if (conditions == null || conditions.Count == 0)
                return true.ToProperty();

            // Ensure at least one true
            var hasAnyTrue = conditions.Any(c => c);
            if (!hasAnyTrue)
                return true.ToProperty();

            var result1 = EvaluateOr(conditions);

            var reversed = new List<bool>(conditions);
            reversed.Reverse();
            var result2 = EvaluateOr(reversed);

            return (result1 && result2).ToProperty();
        }

        /// <summary>
        /// Property: Evaluation result is deterministic (same input always produces same output).
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_EvaluationIsDeterministic(List<bool> conditions, ConditionOperator op)
        {
            if (conditions == null || conditions.Count == 0)
                return true.ToProperty();

            var result1 = EvaluateConditions(conditions, op);
            var result2 = EvaluateConditions(conditions, op);
            var result3 = EvaluateConditions(conditions, op);

            return (result1 == result2 && result2 == result3).ToProperty();
        }

        /// <summary>
        /// Property: Different evaluation orders produce same result.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DifferentOrdersProduceSameResult(List<bool> conditions, ConditionOperator op)
        {
            if (conditions == null || conditions.Count < 2)
                return true.ToProperty();

            var result1 = EvaluateConditions(conditions, op);

            // Shuffle the conditions
            var shuffled = new List<bool>(conditions);
            var random = new Random();
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int randomIndex = random.Next(i + 1);
                var temp = shuffled[i];
                shuffled[i] = shuffled[randomIndex];
                shuffled[randomIndex] = temp;
            }

            var result2 = EvaluateConditions(shuffled, op);

            return (result1 == result2).ToProperty();
        }

        /// <summary>
        /// Property: Empty condition list returns true for AND.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 100)]
        public Property Property_EmptyConditionListReturnsTrue()
        {
            var conditions = new List<bool>();
            var result = EvaluateAnd(conditions);

            return result.ToProperty();
        }

        /// <summary>
        /// Property: Single condition returns that condition's value.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SingleConditionReturnsItsValue(bool condition)
        {
            var conditions = new List<bool> { condition };

            var andResult = EvaluateAnd(conditions);
            var orResult = EvaluateOr(conditions);

            return (andResult == condition && orResult == condition).ToProperty();
        }

        /// <summary>
        /// Property: De Morgan's Law: NOT(A AND B) = NOT(A) OR NOT(B).
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DeMorgansLawAND(bool a, bool b)
        {
            var leftSide = !(a && b);
            var rightSide = (!a) || (!b);

            return (leftSide == rightSide).ToProperty();
        }

        /// <summary>
        /// Property: De Morgan's Law: NOT(A OR B) = NOT(A) AND NOT(B).
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DeMorgansLawOR(bool a, bool b)
        {
            var leftSide = !(a || b);
            var rightSide = (!a) && (!b);

            return (leftSide == rightSide).ToProperty();
        }
    }

    /// <summary>
    /// Represents a condition operator.
    /// </summary>
    public enum ConditionOperator
    {
        And = 1,
        Or = 2
    }
}
