namespace GinPair.Tests;
public class ModelTests {
    [Fact]
    public void ApiResponse_Creation_WorksAsExpected() {
        var response = new ApiResponse();

        response.StatusCode.ShouldBe(200);
        response.StatusMessage.ShouldBeEmpty();
    }

    [Fact]
    public void ErrorViewModel_Creation_WorksAsExpected() {
        var errorModel = new ErrorViewModel();
        errorModel.RequestId.ShouldBeNullOrEmpty();
        errorModel.ShowRequestId.ShouldBeFalse();
    }

    [Theory]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("req-67890", true)]
    public void ErrorViewModel_ShowRequestId_WorksAsExpected(string? requestId, bool expectedShowRequestId) {
        var errorModel = new ErrorViewModel {
            RequestId = requestId
        };

        errorModel.ShowRequestId.ShouldBe(expectedShowRequestId);
    }

    [Fact]
    public void AddGntVM_Creation_WorksAsExpected() {
        var addVm = new AddGntVM();
        addVm.GinId.ShouldBe(0);
        addVm.TonicId.ShouldBe(0);
        addVm.GinDescription.ShouldBeEmpty();
    }
}
