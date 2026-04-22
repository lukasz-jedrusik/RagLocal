using Mapster;

namespace Rag.Services.Backend.Application.Mappings
{
    public static class MapsterConfig
    {
        public static void Configure()
        {
            // Mapster konfiguracja - dodawaj tutaj custom mappingi jeśli potrzebne
            // Przykład: 
            // TypeAdapterConfig<Source, Destination>
            //     .NewConfig()
            //     .Map(dest => dest.PropertyName, src => src.SomeOtherProperty);
        }
    }
}