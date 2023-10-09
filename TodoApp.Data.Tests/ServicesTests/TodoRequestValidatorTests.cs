using TodoApp.Data.Domain.Todos;
using TodoApp.Data.Services;

namespace TodoApp.Data.Tests.ServicesTests;

[TestFixture]
internal class TodoRequestValidatorTests
{
    const string DESCRIPTION_ISNT_BEING_TESTED = "_";
    readonly DateTime DUEDATE_ISNT_BEING_TESTED = new(1, 1, 1);

    [Test]
    public void Validate_InvalidPostRequest_ReturnsFalseIfDescriptionLengthIsGreaterThan100()
    {
        var tooLongDescription = new string('a', 101);
        var request = new TodoPostRequest(tooLongDescription, DUEDATE_ISNT_BEING_TESTED);
        var validator = new TodoRequestValidator();

        var isRequestValid = validator.Validate(request);

        Assert.That(isRequestValid, Is.False);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("     ")]
    [TestCase("\r\n")]
    [TestCase("\r")]
    [TestCase("\n")]
    [TestCase("\t")]
    [TestCase("\0")]
    public void Validate_InvalidPostRequest_ReturnsFalseIfDescriptionIsNullOrBlankOrWhitespace(string? description)
    {
        var request = new TodoPostRequest(description, DUEDATE_ISNT_BEING_TESTED);
        var validator = new TodoRequestValidator();

        var isRequestValid = validator.Validate(request);

        Assert.That(isRequestValid, Is.False);
    }

    [Test]
    public void Validate_InvalidPostRequest_ReturnsFalseIfDueDateIsNull()
    {
        var request = new TodoPostRequest(DESCRIPTION_ISNT_BEING_TESTED, null);
        var validator = new TodoRequestValidator();

        var isRequestValid = validator.Validate(request);

        Assert.That(isRequestValid, Is.False);
    }


    static readonly DateTime NOW = new(2023, 10, 5, 13, 2, 50);
    static readonly (string, DateTime)[] VALID_POST_REQUEST_PARAMS =
    {
        ("Write down all of my todos", NOW),
        ("Pick up the kids at school", NOW.AddHours(3d)),
        ("Go to the gym", NOW.AddDays(1d)),
        ("Delayed todo request", NOW.AddHours(-3d)),
        ("Very delayed todo request", NOW.AddHours(-23d)),
    };
    [TestCaseSource(nameof(VALID_POST_REQUEST_PARAMS))]
    public void Validate_ValidPostRequest_ReturnsTrue((string, DateTime) postRequestParams)
    {
        (var description, var dueDate) = postRequestParams;
        var request = new TodoPostRequest(description, dueDate);
        var validator = new TodoRequestValidator();

        var isRequestValid = validator.Validate(request);

        Assert.That(isRequestValid, Is.True);
    }

    [TestCase(null)]
    [TestCase(TodoRequestValidator.MIN_PAGE_VAL - 1)]
    [TestCase(TodoRequestValidator.MIN_PAGE_VAL - 500)]
    public void Validate_PagingRanges_ReturnedPageIsAlwaysGreaterThanOrEqualToTheMinPageValue(int? page)
    {
        var validator = new TodoRequestValidator();

        (page, _) = validator.Validate(page, 0);

        Assert.That(page, Is.GreaterThanOrEqualTo(TodoRequestValidator.MIN_PAGE_VAL));
    }

    [TestCase(null)]
    [TestCase(TodoRequestValidator.MIN_ROWS_VAL - 1)]
    [TestCase(TodoRequestValidator.MAX_ROWS_VAL + 2)]
    public void Validate_PagingRanges_ReturnedPageIsAlwaysBetweenMinRowsValAndMaxRowsVal(int? rows)
    {
        var validator = new TodoRequestValidator();

        (_, rows) = validator.Validate(0, rows);

        Assert.Multiple(() =>
        {
            Assert.That(rows, Is.GreaterThanOrEqualTo(TodoRequestValidator.MIN_ROWS_VAL));
            Assert.That(rows, Is.LessThanOrEqualTo(TodoRequestValidator.MAX_ROWS_VAL));
        });
    }
}
