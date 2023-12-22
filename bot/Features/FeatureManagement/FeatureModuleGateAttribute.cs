using System;
using Microsoft.FeatureManagement;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace bot.Features.FeatureManagement;

[AttributeUsage(AttributeTargets.Class)]
public class FeatureModuleGateAttribute : Attribute
{
    //
    // Summary:
    //     The name of the features that the feature attribute will activate for.
    public IEnumerable<string> Features { get; }

    //
    // Summary:
    //     Controls whether any or all features in Microsoft.FeatureManagement.Mvc.FeatureGateAttribute.Features
    //     should be enabled to pass.
    public RequirementType RequirementType { get; }

    public FeatureModuleGateAttribute(params string[] features) : this(RequirementType.All, features) { }

    //
    // Summary:
    //     Creates an attribute that can be used to gate actions or pages. The gate can
    //     be configured to require all or any of the provided feature(s) to pass.
    //
    // Parameters:
    //   requirementType:
    //     Specifies whether all or any of the provided features should be enabled in order
    //     to pass.
    //
    //   features:
    //     The names of the features that the attribute will represent.
    public FeatureModuleGateAttribute(RequirementType requirementType, params string[] features)
    {
        if (features == null || features.Length == 0)
        {
            throw new ArgumentNullException(nameof(features));
        }

        Features = features;
        RequirementType = requirementType;
    }

    //
    // Summary:
    //     Creates an attribute that will gate actions or pages unless all the provided
    //     feature(s) are enabled.
    //
    // Parameters:
    //   features:
    //     A set of enums representing the features that the attribute will represent.
    public FeatureModuleGateAttribute(params object[] features)
        : this(RequirementType.All, features)
    {
    }

    //
    // Summary:
    //     Creates an attribute that can be used to gate actions or pages. The gate can
    //     be configured to require all or any of the provided feature(s) to pass.
    //
    // Parameters:
    //   requirementType:
    //     Specifies whether all or any of the provided features should be enabled in order
    //     to pass.
    //
    //   features:
    //     A set of enums representing the features that the attribute will represent.
    public FeatureModuleGateAttribute(RequirementType requirementType, params object[] features)
    {
        if (features == null || features.Length == 0)
        {
            throw new ArgumentNullException(nameof(features));
        }

        List<string> list = new List<string>();
        foreach (object obj in features)
        {
            if (!obj.GetType().IsEnum)
            {
                throw new ArgumentException("The provided features must be enums.", "features");
            }

            list.Add(Enum.GetName(obj.GetType(), obj));
        }

        Features = list;
        RequirementType = requirementType;
    }
}

public class DevEnvironmentFilter : IFeatureFilter
{
    private readonly IWebHostEnvironment _env;

    public DevEnvironmentFilter(IWebHostEnvironment env)
    {
        _env = env;
    }

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        return Task.FromResult(_env.IsDevelopment());
    }
}

public class ProdTestAccountsFilter : IFeatureFilter
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public ProdTestAccountsFilter(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env;
        _config = config;
    }

    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        var listOfAllowedUsers = context.Parameters.Get<string[]>();
        return Task.FromResult(_env.IsProduction() && listOfAllowedUsers != null && listOfAllowedUsers.Contains("123456789012345678"));
    }
}
