namespace AzureTest.e2eTest;

[CollectionDefinition("E2E Tests", DisableParallelization = true)]
public class E2ETestCollection : ICollectionFixture<E2ETestWebAppFactory>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
