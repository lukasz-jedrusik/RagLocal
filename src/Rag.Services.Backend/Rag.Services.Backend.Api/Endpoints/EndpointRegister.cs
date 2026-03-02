namespace Rag.Services.Backend.Api.Endpoints
{
    public static class EndpointRegister
    {
        public static void MapEndpoints(this IEndpointRouteBuilder app)
        {
            app.AddAskEndpoints();
            app.AddIngestEndpoints();
        }
    }
}