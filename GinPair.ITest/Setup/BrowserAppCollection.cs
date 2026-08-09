namespace GinPair.ITest.Setup;

[CollectionDefinition(COLLECTION_NAME)]
public sealed class BrowserAppCollection : ICollectionFixture<BrowserAppFixture> {
    public const string COLLECTION_NAME = "browser-app";
}
