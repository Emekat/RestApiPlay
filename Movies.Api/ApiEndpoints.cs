namespace Movies.Api;

public static class ApiEndpoints
{
    private const string ApiBase = "api";

    public static class Movies
    {
        private const string BasePath = $"{ApiBase}/movies";
        public const string Create = BasePath;
        public const string GetAll = BasePath;
        public const string Get = $"{BasePath}/{{id:guid}}";
        public const string Update = $"{BasePath}/{{id:guid}}";
        public const string Delete = $"{BasePath}/{{id:guid}}";
    }
}