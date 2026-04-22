using Mapster;

namespace Rag.Services.Backend.Application.Mappings
{
    public static class MapsterConfig
    {
        public static void Configure()
        {
            // Mapster configuration  - add custom mappings here if needed
            // Example: 
            // TypeAdapterConfig<Source, Destination>
            //     .NewConfig()
            //     .Map(dest => dest.PropertyName, src => src.SomeOtherProperty);
        }
    }
}