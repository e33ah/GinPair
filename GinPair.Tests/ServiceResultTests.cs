namespace GinPair.Tests;

public class ServiceResultTests {
    private const string SUCCESS_MESSAGE = "Operation successful";
    private const string WARNING_MESSAGE = "Warning message";
    private const string FAILURE_MESSAGE = "Failure message";

    [Fact]
    public void ServiceResult_CanBeInstantiated() {
        var result = new ServiceResult { };
        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe("");
        result.BsColor.ShouldBe(BsColor.Primary);
    }

    [Fact]
    public void Success_CreatesSuccessfulResult_WithCorrectProperties() {
        var result = ServiceResult.Success(SUCCESS_MESSAGE);

        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBe(SUCCESS_MESSAGE);
        result.BsColor.ShouldBe(BsColor.Success);
    }

    [Fact]
    public void Warning_CreatesWarningResult_WithCorrectProperties() {
        var result = ServiceResult.Warning(WARNING_MESSAGE);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(WARNING_MESSAGE);
        result.BsColor.ShouldBe(BsColor.Warning);
    }

    [Fact]
    public void Failure_CreatesFailureResult_WithCorrectProperties() {
        var result = ServiceResult.Failure(FAILURE_MESSAGE);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(FAILURE_MESSAGE);
        result.BsColor.ShouldBe(BsColor.Danger);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("A")]
    [InlineData("This is a longer message with multiple words")]
    public void Success_AcceptsVariousMessageFormats_WhenCalled(string message) {
        var result = ServiceResult.Success(message);

        result.Message.ShouldBe(message);
        result.IsSuccess.ShouldBeTrue();
        result.BsColor.ShouldBe(BsColor.Success);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Short")]
    [InlineData("A very long warning message that contains multiple sentences and provides detailed information.")]
    public void Warning_AcceptsVariousMessageFormats_WhenCalled(string message) {
        var result = ServiceResult.Warning(message);

        result.Message.ShouldBe(message);
        result.IsSuccess.ShouldBeFalse();
        result.BsColor.ShouldBe(BsColor.Warning);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Error!")]
    [InlineData("An error occurred while processing your request. Please try again later.")]
    public void Failure_AcceptsVariousMessageFormats_WhenCalled(string message) {
        var result = ServiceResult.Failure(message);

        result.Message.ShouldBe(message);
        result.IsSuccess.ShouldBeFalse();
        result.BsColor.ShouldBe(BsColor.Danger);
    }
}

public class ServiceResultGenericTests {
    private const string SUCCESS_MESSAGE = "Data retrieved successfully";
    private const string WARNING_MESSAGE = "Warning occurred";
    private const string FAILURE_MESSAGE = "Operation failed";
    private const int TEST_DATA = 42;

    [Fact]
    public void Success_CreatesSuccessfulResult_WithDataAndCorrectProperties() {
        var result = ServiceResult<int>.Success(SUCCESS_MESSAGE, TEST_DATA);

        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBe(SUCCESS_MESSAGE);
        result.BsColor.ShouldBe(BsColor.Success);
        result.Data.ShouldBe(TEST_DATA);
    }

    [Fact]
    public void Warning_CreatesWarningResult_WithNullDataAndCorrectProperties() {
        var result = ServiceResult<string>.Warning(WARNING_MESSAGE);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(WARNING_MESSAGE);
        result.BsColor.ShouldBe(BsColor.Warning);
        result.Data.ShouldBeNull();
    }

    [Fact]
    public void Failure_CreatesFailureResult_WithNullDataAndCorrectProperties() {
        var result = ServiceResult<string>.Failure(FAILURE_MESSAGE);

        result.IsSuccess.ShouldBeFalse();
        result.Message.ShouldBe(FAILURE_MESSAGE);
        result.BsColor.ShouldBe(BsColor.Danger);
        result.Data.ShouldBeNull();
    }

    [Fact]
    public void Success_ReturnsCorrectData_WhenCalledWithComplexTypes() {
        const string MESSAGE = "Gin retrieved";
        var gin = CreateGin(1, "Test Gin", "Test Distillery");

        var result = ServiceResult<Gin>.Success(MESSAGE, gin);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.GinId.ShouldBe(1);
        result.Data.GinName.ShouldBe("Test Gin");
        result.Data.Distillery.ShouldBe("Test Distillery");
    }

    [Fact]
    public void Success_ReturnsCorrectData_WhenCalledWithCollectionTypes() {
        const string MESSAGE = "Gins retrieved";
        var gins = new List<Gin> {
            CreateGin(1, "Gin 1", "Distillery 1"),
            CreateGin(2, "Gin 2", "Distillery 2")
        };

        var result = ServiceResult<List<Gin>>.Success(MESSAGE, gins);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data.Count.ShouldBe(2);
        result.Data[0].GinId.ShouldBe(1);
        result.Data[1].GinId.ShouldBe(2);
    }

    [Fact]
    public void Success_AcceptsNullData_WhenCalledWithNullableType() {
        const string MESSAGE = "Success with null data";

        var result = ServiceResult<string?>.Success(MESSAGE, null);

        result.IsSuccess.ShouldBeTrue();
        result.Message.ShouldBe(MESSAGE);
        result.Data.ShouldBeNull();
        result.BsColor.ShouldBe(BsColor.Success);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Success_ReturnsCorrectData_WhenCalledWithVariousPrimitiveValues(int value) {
        const string MESSAGE = "Primitive value";

        var result = ServiceResult<int>.Success(MESSAGE, value);

        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldBe(value);
        result.BsColor.ShouldBe(BsColor.Success);
    }

    [Fact]
    public void Warning_InheritsFromBaseServiceResult_WhenCreated() {
        var result = ServiceResult<int>.Warning(WARNING_MESSAGE);

        result.ShouldBeAssignableTo<ServiceResult>();
    }

    [Fact]
    public void Failure_InheritsFromBaseServiceResult_WhenCreated() {
        var result = ServiceResult<string>.Failure(FAILURE_MESSAGE);

        result.ShouldBeAssignableTo<ServiceResult>();
    }

    [Fact]
    public void Success_InheritsFromBaseServiceResult_WhenCreated() {
        var result = ServiceResult<double>.Success(SUCCESS_MESSAGE, 3.14);

        result.ShouldBeAssignableTo<ServiceResult>();
    }

    [Fact]
    public void Success_ReturnsCorrectData_WhenCalledWithBooleanData() {
        var resultTrue = ServiceResult<bool>.Success("True value", true);
        var resultFalse = ServiceResult<bool>.Success("False value", false);

        resultTrue.Data.ShouldBeTrue();
        resultTrue.IsSuccess.ShouldBeTrue();
        resultFalse.Data.ShouldBeFalse();
        resultFalse.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Success_PreservesMessage_WhenCalledWithSpecialCharacters() {
        const string MESSAGE = "Success! \"Test\" gin & <tonic> was added: £100";

        var result = ServiceResult<int>.Success(MESSAGE, 1);

        result.Message.ShouldBe(MESSAGE);
        result.IsSuccess.ShouldBeTrue();
    }
}
