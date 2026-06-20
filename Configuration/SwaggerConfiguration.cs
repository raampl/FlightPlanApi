namespace FlightPlanApi.Configuration;

public static class SwaggerConfiguration
{
    public const string DocName = "flightplan";
    public const string SecuritySchemeName = "basicAuth";

    public static void Configure(SwaggerGenOptions options)
    {
        options.SwaggerDoc(DocName, new OpenApiInfo
        {
            Title = "Flight Plan API",
            Version = "v3",
            Description = "Pluralsight Web API Demo Project"
        });

        options.AddSecurityDefinition(SecuritySchemeName, new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "basic",
            In = ParameterLocation.Header,
            Description = "Basic authorization header using Bearer scheme"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = SecuritySchemeName
                    }
                },
                []
            }
        });

        options.EnableAnnotations();

        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    }
}
