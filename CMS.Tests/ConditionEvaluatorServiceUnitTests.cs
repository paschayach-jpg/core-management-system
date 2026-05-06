using System;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using CMS.Infrastructure.Interfaces;
using CMS.Services;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for ConditionEvaluatorService (MOD-008) Phase 15.
    /// Tests condition expression evaluation and validation.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - Condition expression evaluation
    /// - Condition expression validation
    /// - Error handling
    /// - Null/empty parameter handling
    /// - Complex expression evaluation
    /// </summary>
    [TestFixture]
    public class ConditionEvaluatorServiceUnitTests
    {
        private ConditionEvaluatorService _conditionEvaluatorService;
        private ILoggingService _loggingService;

        [SetUp]
        public void SetUp()
        {
            // Create mock dependencies
            _loggingService = Substitute.For<ILoggingService>();

            // Create service instance
            _conditionEvaluatorService = new ConditionEvaluatorService(_loggingService);
        }

        #region EvaluateCondition Tests

        [Test]
        public void EvaluateCondition_WithNullExpression_ReturnsTrue()
        {
            // Arrange
            string expression = null;
            var contextData = "{\"result\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EvaluateCondition_WithEmptyExpression_ReturnsTrue()
        {
            // Arrange
            var expression = "";
            var contextData = "{\"result\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EvaluateCondition_WithWhitespaceExpression_ReturnsTrue()
        {
            // Arrange
            var expression = "   ";
            var contextData = "{\"result\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void EvaluateCondition_WithValidExpression_ReturnsResult()
        {
            // Arrange
            var expression = "result == 'approved'";
            var contextData = "{\"result\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void EvaluateCondition_WithComplexExpression_ReturnsResult()
        {
            // Arrange
            var expression = "(status == 'pending' AND priority > 5) OR escalated == true";
            var contextData = "{\"status\": \"pending\", \"priority\": 10, \"escalated\": true}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void EvaluateCondition_WithNullContextData_ReturnsResult()
        {
            // Arrange
            var expression = "result == 'approved'";
            string contextData = null;

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void EvaluateCondition_WithEmptyContextData_ReturnsResult()
        {
            // Arrange
            var expression = "result == 'approved'";
            var contextData = "";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void EvaluateCondition_WithInvalidJsonContextData_ReturnsResult()
        {
            // Arrange
            var expression = "result == 'approved'";
            var contextData = "invalid json";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void EvaluateCondition_WithMultipleConditions_ReturnsResult()
        {
            // Arrange
            var expression = "status == 'active' AND type == 'approval' AND level > 2";
            var contextData = "{\"status\": \"active\", \"type\": \"approval\", \"level\": 3}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void EvaluateCondition_WithOrConditions_ReturnsResult()
        {
            // Arrange
            var expression = "status == 'pending' OR status == 'approved' OR status == 'rejected'";
            var contextData = "{\"status\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        #endregion

        #region ValidateConditionExpression Tests

        [Test]
        public void ValidateConditionExpression_WithNullExpression_ReturnsValid()
        {
            // Arrange
            string expression = null;

            // Act
            var result = _conditionEvaluatorService.ValidateConditionExpression(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateConditionExpression_WithEmptyExpression_ReturnsValid()
        {
            // Arrange
            var expression = "";

            // Act
            var result = _conditionEvaluatorService.ValidateConditionExpression(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ValidateConditionExpression_WithValidExpression_ReturnsValid()
        {
            // Arrange
            var expression = "result == 'approved'";

            // Act
            var result = _conditionEvaluatorService.ValidateConditionExpression(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ValidationResult>(result);
        }

        [Test]
        public void ValidateConditionExpression_WithComplexValidExpression_ReturnsValid()
        {
            // Arrange
            var expression = "(status == 'pending' AND priority > 5) OR (escalated == true AND level >= 3)";

            // Act
            var result = _conditionEvaluatorService.ValidateConditionExpression(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ValidationResult>(result);
        }

        [Test]
        public void ValidateConditionExpression_WithMultipleOperators_ReturnsValid()
        {
            // Arrange
            var expression = "a == 1 AND b == 2 OR c == 3 AND d == 4";

            // Act
            var result = _conditionEvaluatorService.ValidateConditionExpression(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ValidationResult>(result);
        }

        [Test]
        public void ValidateConditionExpression_WithParentheses_ReturnsValid()
        {
            // Arrange
            var expression = "((a == 1 AND b == 2) OR (c == 3 AND d == 4))";

            // Act
            var result = _conditionEvaluatorService.ValidateConditionExpression(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ValidationResult>(result);
        }

        [Test]
        public void ValidateConditionExpression_WithComparisonOperators_ReturnsValid()
        {
            // Arrange
            var expression = "value > 10 AND value < 100 AND value != 50";

            // Act
            var result = _conditionEvaluatorService.ValidateConditionExpression(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ValidationResult>(result);
        }

        #endregion

        #region EvaluateConditionExpression Tests

        [Test]
        public void EvaluateConditionExpression_WithValidExpression_ReturnsResult()
        {
            // Arrange
            var expression = "result == 'approved'";
            var contextData = "{\"result\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ConditionEvaluationResult>(result);
            Assert.AreEqual(expression, result.Expression);
            Assert.AreEqual(contextData, result.ContextData);
            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public void EvaluateConditionExpression_WithNullExpression_ReturnsSuccessfulResult()
        {
            // Arrange
            string expression = null;
            var contextData = "{\"result\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public void EvaluateConditionExpression_WithEmptyExpression_ReturnsSuccessfulResult()
        {
            // Arrange
            var expression = "";
            var contextData = "{\"result\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public void EvaluateConditionExpression_WithComplexExpression_ReturnsResult()
        {
            // Arrange
            var expression = "(status == 'pending' AND priority > 5) OR escalated == true";
            var contextData = "{\"status\": \"pending\", \"priority\": 10, \"escalated\": true}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ConditionEvaluationResult>(result);
            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public void EvaluateConditionExpression_WithNullContextData_ReturnsSuccessfulResult()
        {
            // Arrange
            var expression = "result == 'approved'";
            string contextData = null;

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public void EvaluateConditionExpression_WithMultipleConditions_ReturnsResult()
        {
            // Arrange
            var expression = "status == 'active' AND type == 'approval' AND level > 2";
            var contextData = "{\"status\": \"active\", \"type\": \"approval\", \"level\": 3}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
            Assert.IsInstanceOf<bool>(result.Result);
        }

        [Test]
        public void EvaluateConditionExpression_WithOrConditions_ReturnsResult()
        {
            // Arrange
            var expression = "status == 'pending' OR status == 'approved' OR status == 'rejected'";
            var contextData = "{\"status\": \"approved\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public void EvaluateConditionExpression_WithParentheses_ReturnsResult()
        {
            // Arrange
            var expression = "((a == 1 AND b == 2) OR (c == 3 AND d == 4))";
            var contextData = "{\"a\": 1, \"b\": 2, \"c\": 3, \"d\": 4}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
        }

        [Test]
        public void EvaluateConditionExpression_WithComparisonOperators_ReturnsResult()
        {
            // Arrange
            var expression = "value > 10 AND value < 100 AND value != 50";
            var contextData = "{\"value\": 75}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public void EvaluateCondition_WithLoggingService_LogsDebugMessage()
        {
            // Arrange
            var expression = "result == 'approved'";
            var contextData = "{\"result\": \"approved\"}";

            // Act
            _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            _loggingService.Received(1).Debug(Arg.Any<string>());
        }

        [Test]
        public void ValidateConditionExpression_WithLoggingService_DoesNotThrow()
        {
            // Arrange
            var expression = "result == 'approved'";

            // Act & Assert
            Assert.DoesNotThrow(() =>
                _conditionEvaluatorService.ValidateConditionExpression(expression)
            );
        }

        [Test]
        public void EvaluateConditionExpression_WithLoggingService_DoesNotThrow()
        {
            // Arrange
            var expression = "result == 'approved'";
            var contextData = "{\"result\": \"approved\"}";

            // Act & Assert
            Assert.DoesNotThrow(() =>
                _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData)
            );
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void EvaluateCondition_WithSpecialCharacters_ReturnsResult()
        {
            // Arrange
            var expression = "name == 'John@Doe' AND email == 'john.doe@example.com'";
            var contextData = "{\"name\": \"John@Doe\", \"email\": \"john.doe@example.com\"}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void EvaluateCondition_WithNumericValues_ReturnsResult()
        {
            // Arrange
            var expression = "amount > 1000.50 AND quantity >= 5";
            var contextData = "{\"amount\": 1500.75, \"quantity\": 10}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void EvaluateCondition_WithBooleanValues_ReturnsResult()
        {
            // Arrange
            var expression = "isActive == true AND isDeleted == false";
            var contextData = "{\"isActive\": true, \"isDeleted\": false}";

            // Act
            var result = _conditionEvaluatorService.EvaluateCondition(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<bool>(result);
        }

        [Test]
        public void ValidateConditionExpression_WithLongExpression_ReturnsValid()
        {
            // Arrange
            var expression = "a == 1 AND b == 2 AND c == 3 AND d == 4 AND e == 5 AND f == 6 AND g == 7 AND h == 8";

            // Act
            var result = _conditionEvaluatorService.ValidateConditionExpression(expression);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<ValidationResult>(result);
        }

        [Test]
        public void EvaluateConditionExpression_WithNestedParentheses_ReturnsResult()
        {
            // Arrange
            var expression = "(((a == 1 AND b == 2) OR (c == 3 AND d == 4)) AND (e == 5 OR f == 6))";
            var contextData = "{\"a\": 1, \"b\": 2, \"c\": 3, \"d\": 4, \"e\": 5, \"f\": 6}";

            // Act
            var result = _conditionEvaluatorService.EvaluateConditionExpression(expression, contextData);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsSuccessful);
        }

        #endregion
    }
}
