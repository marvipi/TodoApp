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

    [Test]
    public void Update_CurrentAndNewStatesAreBothDue_UpdatesTheDescriptionAndTheDueDate()
    {
        var todo = new TodoEntity("Test the updating of due to due", new DateTime(2023, 10, 8, 19, 44, 15));
        var newDescription = "Test all 9 possible cases of the update method";
        var modificationDate = new DateTime(2023, 10, 8, 19, 45, 28);

        todo.Update(newDescription, TodoState.Due, modificationDate);

        Assert.Multiple(() =>
        {
            Assert.That(todo.Description, Is.EqualTo(newDescription));
            Assert.That(todo.DueDate, Is.EqualTo(modificationDate));
            Assert.That(todo.CurrentStateDate, Is.EqualTo(modificationDate));
            Assert.That(todo.CompletionDate, Is.Null);
            Assert.That(todo.CancellationDate, Is.Null);
            Assert.That(todo.TodoState, Is.EqualTo(TodoState.Due));
        });
    }

    [Test]
    public void Update_CurrentAndNewStatesAreBothDone_UpdatesTheDescriptionAndTheCompletionDate()
    {
        var initialDueDate = new DateTime(2023, 10, 8, 19, 51, 11);
        var todo = new TodoEntity("Test the update method with a todo state of done", initialDueDate);
        var expectedDescription = "Refactor all tests to be more readable";
        var expectedCompletionDate = new DateTime(2023, 10, 8, 19, 53, 28);

        todo.Update("Doesn't matter", TodoState.Done, new DateTime(2023, 10, 8, 19, 51, 27));
        todo.Update(expectedDescription, TodoState.Done, expectedCompletionDate);

        Assert.Multiple(() =>
        {
            Assert.That(todo.Description, Is.EqualTo(expectedDescription));
            Assert.That(todo.CompletionDate, Is.EqualTo(expectedCompletionDate));
            Assert.That(todo.CurrentStateDate, Is.EqualTo(expectedCompletionDate));
            Assert.That(todo.DueDate, Is.EqualTo(initialDueDate));
            Assert.That(todo.CancellationDate, Is.Null);
            Assert.That(todo.TodoState, Is.EqualTo(TodoState.Done));
        });
    }

    [Test]
    public void Update_CurrentAndNewStatesAreBothCancelled_UpdatesTheDescriptionAndTheCancelledDate()
    {
        var initialDueDate = new DateTime(2023, 10, 8, 19, 58, 55);
        var todo = new TodoEntity("Test updating a cancelled todo to a cancelled state", initialDueDate);
        var expectedDescription = "Find edge cases for the update method";
        var expectedCancellationDate = new DateTime(2023, 10, 8, 20, 1, 44);

        todo.Update("Doesn't matter in the slightest", TodoState.Cancelled, new DateTime(2023, 10, 8, 20, 0, 0));
        todo.Update(expectedDescription, TodoState.Cancelled, expectedCancellationDate);

        Assert.Multiple(() =>
        {
            Assert.That(todo.Description, Is.EqualTo(expectedDescription));
            Assert.That(todo.CancellationDate, Is.EqualTo(expectedCancellationDate));
            Assert.That(todo.CurrentStateDate, Is.EqualTo(expectedCancellationDate));
            Assert.That(todo.DueDate, Is.EqualTo(initialDueDate));
            Assert.That(todo.CompletionDate, Is.Null);
            Assert.That(todo.TodoState, Is.EqualTo(TodoState.Cancelled));
        });
    }

    [TestCase(TodoState.Done)]
    [TestCase(TodoState.Cancelled)]
    public void Update_CurrentStateIsDueAndNewStateIsDoneOrCancelled_UpdatesTodoStateAndRespectiveDate(TodoState newState)
    {
        var initialDueDate = new DateTime(2023, 10, 8, 20, 24, 4);
        var initialDescription = "Test completion and cancellation of todos";
        var todo = new TodoEntity(initialDescription, initialDueDate);
        var expectedCurrentStateDate = new DateTime(2023, 10, 8, 20, 27, 48);

        todo.Update("Description shouldn't be updated when switching states", newState, expectedCurrentStateDate);

        Assert.Multiple(() =>
        {
            Assert.That(todo.Description, Is.EqualTo(initialDescription));
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
        var initialDescription = "Test reverting cancelled and done todos back to due";
        var todo = new TodoEntity(initialDescription, initialDueDate);
        var modificationDate = new DateTime(2023, 10, 8, 20, 30, 43);
        var reversionDate = new DateTime(2023, 10, 8, 20, 32, 48);

        todo.Update("Description shouldn't update when switching states", currentState, modificationDate);
        todo.Update("Still should be equal to the initial description", TodoState.Due, reversionDate);

        Assert.Multiple(() =>
        {
            Assert.That(todo.Description, Is.EqualTo(initialDescription));
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

        todo.Update(DESCRIPTION_ISNT_BEING_TESTED, currentState, DUEDATE_ISNT_BEING_TESTED);

        Assert.Throws<NotSupportedException>(() => todo.Update(DESCRIPTION_ISNT_BEING_TESTED,
                                                                newState,
                                                                DUEDATE_ISNT_BEING_TESTED));
    }
}
