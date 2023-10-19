using TodoApp.Data.Domain.Todos;

namespace TodoApp.Data.Tests.EntityTests;

[TestFixture]
internal class TodoEntityTests
{
    const string DESCRIPTION_ISNT_BEING_TESTED = "_";
    static readonly DateTime DUEDATE_ISNT_BEING_TESTED = new(1, 1, 1);
    static readonly DateTime NOW = new(2023, 10, 3);


    [Test]
    public void Constructor_Id_AssignsAGuidToTheNewInstance()
    {
        var newTodo = new TodoEntity(DESCRIPTION_ISNT_BEING_TESTED, DUEDATE_ISNT_BEING_TESTED);

        Assert.That(newTodo.Id, Is.Not.EqualTo(Guid.Empty));
    }

    [Test]
    public void Constructor_TodoState_SetsTodoStateToDue()
    {
        var newTodo = new TodoEntity(DESCRIPTION_ISNT_BEING_TESTED, DUEDATE_ISNT_BEING_TESTED);

        Assert.That(newTodo.TodoState, Is.EqualTo(TodoState.Due));
    }

    [TestCase("Pick up the kids at school.")]
    [TestCase("run away")]
    [TestCase("Cover all edge cases")]
    public void Constructor_Description_AssignsGivenDescriptionToDescriptionProperty(string expectedDescription)
    {
        var newTodo = new TodoEntity(expectedDescription, DUEDATE_ISNT_BEING_TESTED);

        Assert.That(newTodo.Description, Is.EqualTo(expectedDescription));
    }

    static readonly DateTime[] CONSTRUCTOR_DUEDATES =
    {
        NOW,
        NOW.AddDays(1),
        NOW.AddMonths(1),
        NOW.AddYears(1)
    };
    [TestCaseSource(nameof(CONSTRUCTOR_DUEDATES))]
    public void Constructor_DueDate_AssignsGivenDateTimeToDueDateProperty(DateTime expectedDueDate)
    {
        var newTodo = new TodoEntity(DESCRIPTION_ISNT_BEING_TESTED, expectedDueDate);

        Assert.That(newTodo.CurrentStateDate, Is.EqualTo(expectedDueDate));
    }

    [TestCase(TodoState.Done)]
    [TestCase(TodoState.Cancelled)]
    public void Update_CurrentStateIsDueAndNewStateIsDoneOrCancelled_UpdatesTodoStateAndRespectiveDate(TodoState newState)
    {
        var initialDueDate = new DateTime(2023, 10, 8, 20, 24, 4);
        var initialDescription = "Test completion and cancellation of todos";
        var todo = new TodoEntity(initialDescription, initialDueDate);
        var expectedCurrentStateDate = new DateTime(2023, 10, 8, 20, 27, 48);

        todo.Update(newState, expectedCurrentStateDate);

        Assert.Multiple(() =>
        {
            Assert.That(todo.TodoState, Is.EqualTo(newState));
            Assert.That(todo.CurrentStateDate, Is.EqualTo(expectedCurrentStateDate));
            Assert.That(todo.DueDate, Is.EqualTo(initialDueDate));
        });
    }

    [TestCase(TodoState.Done)]
    [TestCase(TodoState.Cancelled)]
    public void Update_CurrentStateIsDoneOrCancelledAndNewStateIsDue_RevertsTodoStateBackToDue(TodoState currentState)
    {
        var initialDueDate = new DateTime(2023, 10, 8, 20, 29, 27);
        var todo = new TodoEntity("Test reverting cancelled and done todos back to due", initialDueDate);
        var modificationDate = new DateTime(2023, 10, 8, 20, 30, 43);
        var reversionDate = new DateTime(2023, 10, 8, 20, 32, 48);

        todo.Update(currentState, modificationDate);
        todo.Update(TodoState.Due, reversionDate);

        Assert.Multiple(() =>
        {
            Assert.That(todo.TodoState, Is.EqualTo(TodoState.Due));
            Assert.That(todo.DueDate, Is.EqualTo(initialDueDate));
            Assert.That(todo.CurrentStateDate, Is.EqualTo(initialDueDate));
        });
    }

    [TestCase(TodoState.Done, TodoState.Cancelled)]
    [TestCase(TodoState.Cancelled, TodoState.Done)]
    public void Update_SwitchingBetweenIncompatibleStates_RaisesNotSupportedException
        (TodoState currentState, TodoState newState)
    {
        var todo = new TodoEntity(DESCRIPTION_ISNT_BEING_TESTED, DUEDATE_ISNT_BEING_TESTED);

        todo.Update(currentState, DUEDATE_ISNT_BEING_TESTED);

        Assert.Throws<NotSupportedException>(() => todo.Update(newState, DUEDATE_ISNT_BEING_TESTED));
    }
}
